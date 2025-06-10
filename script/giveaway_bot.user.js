// ==UserScript==
// @name         keydrop_giveaway_script
// @namespace    http://tampermonkey.net/
// @homepageURL  https://www.favoslav.cz/
// @version      1.2.1-beta
// @description  KeyDrop Giveaway Bot
// @author       Favoslav_ & Pr0Xy
// @include      *://*key*drop*/*
// @grant        GM_info
// @updateURL    https://github.com/mrFavoslav/KeyDrop-Giveaway-Bot/raw/main/script/giveaway_bot.user.js
// @downloadURL  https://github.com/mrFavoslav/KeyDrop-Giveaway-Bot/raw/main/script/giveaway_bot.user.js
// ==/UserScript==

/**
 * SECTION 1: VERSION CHECKING
 * Checks if script has been updated and prompts user to download new version
 */
if (!localStorage.getItem('script_version')) {
    localStorage.setItem('script_version', GM_info.script.version);
}

if (localStorage.getItem('script_version') != GM_info.script.version) {
    if (confirm("New version available! To access new features, please update your app. Click 'OK' to visit the download page.")) {
        window.location.href = "https://github.com/mrFavoslav/KeyDrop-Giveaway-Bot";
    }
    localStorage.setItem('script_version', GM_info.script.version);
}

/**
 * SECTION 2: CONFIGURATION & DEFAULT SETTINGS
 * Default values and configuration parameters
 */
// WebSocket toggle configuration
let BYPASS_WEBSOCKET = localStorage.getItem('bypass_websocket') === 'true' ? true : false;

// Set default to true for first-time users
if (!localStorage.getItem('bypass_websocket')) {
    localStorage.setItem('bypass_websocket', 'true');
}

// Default label settings
const labelFlagsDefault = {
    AMATEUR: [60000, false],
    CONTENDER: [300000, false],
    // LEGEND: [900000, false],  // Commented out in original
    CHALLENGER: [3600000, false],
    CHAMPION: [21600000, false],
};

const minPricesDefault = {
    AMATEUR: 0,
    CONTENDER: 0,
    CHALLENGER: 0,
    CHAMPION: 0,
  };

// Default cooldown times (in milliseconds)
const woccDefault = 15000;  // Default cooldown (without captcha)
const wccDefault = 30000;   // Captcha cooldown

// Initialize settings if they don't exist and websocket is not bypassed
if ((!localStorage.getItem('labels') || !localStorage.getItem('wocc') || !localStorage.getItem('wcc') || !localStorage.getItem('minPrices') ) && !BYPASS_WEBSOCKET) {
    localStorage.setItem('labels', JSON.stringify(labelFlagsDefault));
    localStorage.setItem('minPrices', JSON.stringify(minPricesDefault));
    localStorage.setItem('wocc', woccDefault);
    localStorage.setItem('wcc', wccDefault);
}

// Connection status tracking
let socketConnected = false;

/**
 * SECTION 3: LOGGER & UTILITY FUNCTIONS
 * Enhanced logging and helper functions
 */
// Setup automatic console clearing
function setupConsoleClearing() {
    setInterval(() => {
        console.clear();
        console.log("%c 🧹 Console cleared automatically! Keep bot running. 🌈", "color: #ff69b4; font-weight: bold; font-size: 14px;");
    }, 60000); // Clear every minute
}

// Pretty console logging with emojis
const logger = {
    info: (message, ...args) => {
        console.log(`%c 💫 INFO: ${message}`, 'color: #3498db; font-weight: bold;', ...args);
    },
    success: (message, ...args) => {
        console.log(`%c ✅ SUCCESS: ${message}`, 'color: #2ecc71; font-weight: bold;', ...args);
    },
    warn: (message, ...args) => {
        console.warn(`%c ⚠️ WARNING: ${message}`, 'color: #f39c12; font-weight: bold;', ...args);
    },
    error: (message, ...args) => {
        console.error(`%c ❌ ERROR: ${message}`, 'color: #e74c3c; font-weight: bold;', ...args);
    },
    debug: (message, ...args) => {
        console.debug(`%c 🔍 DEBUG: ${message}`, 'color: #9b59b6; font-weight: bold;', ...args);
    },
    giveaway: (message, ...args) => {
        console.log(`%c 🎁 GIVEAWAY: ${message}`, 'color: #ff69b4; font-weight: bold;', ...args);
    },
    captcha: (message, ...args) => {
        console.log(`%c 🤖 CAPTCHA: ${message}`, 'color: #ff9800; font-weight: bold;', ...args);
    },
    ws: (message, ...args) => {
        console.log(`%c 🔌 WEBSOCKET: ${message}`, 'color: #00bcd4; font-weight: bold;', ...args);
    }
};

// Create promise-based delay
async function timeout(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
}

// Convert milliseconds to human-readable string
function msToStr(ms) {
    if (ms % 3600000 === 0) return (ms / 3600000) + "h";
    if (ms % 60000 === 0) return (ms / 60000) + "m";
    if (ms % 1000 === 0) return (ms / 1000) + "s";
    return ms + "ms";
}

// Convert human-readable time string to milliseconds
function strToMs(str) {
    str = str.trim().toLowerCase();
    if (str.endsWith("h")) return parseFloat(str) * 3600000;
    if (str.endsWith("m")) return parseFloat(str) * 60000;
    if (str.endsWith("s")) return parseFloat(str) * 1000;
    return parseInt(str, 10);
}

// Wait for an element to appear in the DOM
function waitForElement(selector, timeoutMs = 15000) {
    return new Promise((resolve, reject) => {
        const startTime = Date.now();
        const element = document.querySelector(selector);
        if (element) {
            logger.success(`Found element ${selector} immediately! ^w^`);
            return resolve(element);
        }

        logger.info(`Waiting for element ${selector} to appear... 👀`);
        const observer = new MutationObserver(() => {
            const el = document.querySelector(selector);
            if (el) {
                observer.disconnect();
                logger.success(`Element ${selector} found after ${Date.now() - startTime}ms! Yay! 🎉`);
                resolve(el);
            } else if (Date.now() - startTime > timeoutMs) {
                observer.disconnect();
                const errorMsg = `Element with selector "${selector}" not found within ${timeoutMs}ms`;
                logger.error(`${errorMsg} (ಥ﹏ಥ)`);
                reject(new Error(errorMsg));
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });
    });
}

// Toggle WebSocket bypass mode
function toggleBypassWebsocket() {
    BYPASS_WEBSOCKET = !BYPASS_WEBSOCKET;
    localStorage.setItem('bypass_websocket', BYPASS_WEBSOCKET);
    logger.info(`WebSocket mode ${!BYPASS_WEBSOCKET ? 'enabled' : 'disabled'}! Reloading page... 🔄`);
    location.reload(); // Reload to apply changes
}

/**
 * SECTION 4: UI ELEMENTS
 * UI components for user interaction
 */
// WebSocket toggle button
const toggleButton = document.createElement('button');
toggleButton.innerHTML = `WebSocket ${!BYPASS_WEBSOCKET ? 'Enabled' : 'Disabled'}`;
toggleButton.style = `
    position: fixed;
    bottom: 20px;
    right: 20px;
    z-index: 9999;
    padding: 10px;
    background: ${!BYPASS_WEBSOCKET ? '#ff69b4' : '#666'};
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
`;
toggleButton.onclick = toggleBypassWebsocket;
document.body.appendChild(toggleButton);

/**
 * SECTION 5: STANDALONE MODE UI
 * Settings interface when running without WebSocket connection
 */
if (BYPASS_WEBSOCKET) {
    logger.info(`Running in standalone mode! ✨ WebSocket bypassed~`);

    // Load current settings or defaults
    let labels = JSON.parse(localStorage.getItem('labels')) || labelFlagsDefault;
    let minPrices = JSON.parse(localStorage.getItem('minPrices')) || minPricesDefault;
    let wocc = localStorage.getItem('wocc') || woccDefault;
    let wcc = localStorage.getItem('wcc') || wccDefault;

    // Create the settings button (gear icon)
    const gearBtn = document.createElement('div');
    gearBtn.innerHTML = `
        <svg fill="#ffb400" width="32" height="32" viewBox="0 0 45.973 45.973" style="display:block;">
            <g> <g> <path d="M43.454,18.443h-2.437c-0.453-1.766-1.16-3.42-2.082-4.933l1.752-1.756c0.473-0.473,0.733-1.104,0.733-1.774 c0-0.669-0.262-1.301-0.733-1.773l-2.92-2.917c-0.947-0.948-2.602-0.947-3.545-0.001l-1.826,1.815 C30.9,6.232,29.296,5.56,27.529,5.128V2.52c0-1.383-1.105-2.52-2.488-2.52h-4.128c-1.383,0-2.471,1.137-2.471,2.52v2.607 c-1.766,0.431-3.38,1.104-4.878,1.977l-1.825-1.815c-0.946-0.948-2.602-0.947-3.551-0.001L5.27,8.205 C4.802,8.672,4.535,9.318,4.535,9.978c0,0.669,0.259,1.299,0.733,1.772l1.752,1.76c-0.921,1.513-1.629,3.167-2.081,4.933H2.501 C1.117,18.443,0,19.555,0,20.935v4.125c0,1.384,1.117,2.471,2.501,2.471h2.438c0.452,1.766,1.159,3.43,2.079,4.943l-1.752,1.763 c-0.474,0.473-0.734,1.106-0.734,1.776s0.261,1.303,0.734,1.776l2.92,2.919c0.474,0.473,1.103,0.733,1.772,0.733 s1.299-0.261,1.773-0.733l1.833-1.816c1.498,0.873,3.112,1.545,4.878,1.978v2.604c0,1.383,1.088,2.498,2.471,2.498h4.128 c1.383,0,2.488-1.115,2.488-2.498v-2.605c1.767-0.432,3.371-1.104,4.869-1.977l1.817,1.812c0.474,0.475,1.104,0.735,1.775,0.735 c0.67,0,1.301-0.261,1.774-0.733l2.92-2.917c0.473-0.472,0.732-1.103,0.734-1.772c0-0.67-0.262-1.299-0.734-1.773l-1.75-1.77 c0.92-1.514,1.627-3.179,2.08-4.943h2.438c1.383,0,2.52-1.087,2.52-2.471v-4.125C45.973,19.555,44.837,18.443,43.454,18.443z M22.976,30.85c-4.378,0-7.928-3.517-7.928-7.852c0-4.338,3.55-7.85,7.928-7.85c4.379,0,7.931,3.512,7.931,7.85 C30.906,27.334,27.355,30.85,22.976,30.85z"></path> </g> </g>
        </svg>
    `;
    gearBtn.style = `
        position: fixed;
        bottom: 80px;
        right: 20px;
        z-index: 10001;
        cursor: pointer;
        border-radius: 50%;
        box-shadow: 0 2px 8px #0005;
        background: #18191c;
        padding: 2px;
        transition: box-shadow 0.2s;
    `;
    gearBtn.title = "Open KeyDrop-BOT Settings";
    document.body.appendChild(gearBtn);

    // Create the settings panel
    const panel = document.createElement('div');
    panel.style = `
        position: fixed;
        bottom: 120px;
        right: 20px;
        z-index: 10000;
        background: #18191c;
        color: #fff;
        border-radius: 10px;
        box-shadow: 0 2px 16px #000a;
        padding: 16px 20px 16px 16px;
        min-width: 340px;
        font-family: 'Segoe UI', Arial, sans-serif;
        font-size: 15px;
        display: none;
        animation: rollIn 0.3s;
    `;
    panel.innerHTML = `
        <style>
            @keyframes rollIn {
                from { opacity: 0; transform: translateX(60px) scale(0.95);}
                to { opacity: 1; transform: translateX(0) scale(1);}
            }
            .kd-label-row {
                display: flex;
                justify-content: space-between;
                align-items: center;
                margin-bottom: 4px;
                min-width: 200px;
            }
            .kd-label-row label {
                display: flex;
                align-items: center;
                gap: 6px;
                flex: 1;
            }
            .kd-label-row input[type="text"] {
                width: 48px;
                background: #222;
                color: #fff;
                border: 1px solid #333;
                border-radius: 4px;
                text-align: right;
                padding: 2px 6px;
                margin-left: 8px;
            }
            .kd-label-row input[type="checkbox"] {
                accent-color: #ffb400;
                width: 16px;
                height: 16px;
            }
            .kd-update-btn {
                background: #23272b;
                color: #ffb400;
                border: 1px solid #444;
                border-radius: 6px;
                padding: 6px 18px;
                font-size: 15px;
                cursor: pointer;
                margin-top: 10px;
                transition: background 0.2s, color 0.2s;
            }
            .kd-update-btn:hover {
                background: #ffb400;
                color: #23272b;
            }
            .kd-labels-list {
                margin-bottom: 10px;
            }

            .kd-label-row input[type="checkbox"] {
                accent-color: #ffb400 !important;
                width: 18px !important;
                height: 18px !important;
                display: inline-block !important;
                opacity: 1 !important;
                margin-right: 8px !important;
                position: relative !important;
                visibility: visible !important;
                pointer-events: auto !important;
                appearance: auto !important;
                -webkit-appearance: checkbox !important;
                z-index: 100 !important;
            }
        </style>
        <div style="display: flex; gap: 24px;">
            <fieldset style="border: 1px solid #333; border-radius: 6px; padding: 8px 12px; min-width: 180px;">
                <legend style="color:#fff;">Labels | Cooldown | Min. Price</legend>
                <div class="kd-labels-list">
                    ${Object.entries(labelFlagsDefault).map(([label, [defCooldown]]) => `
                        <div class="kd-label-row">
                            <label style="display: flex; align-items: center; gap: 6px;">
                                <input type="checkbox" class="label-toggle" id="enabled_${label}" ${labels[label]?.[1] ? "checked" : ""}>
                                <span>${label}</span>
                            </label>
                            <input type="text" id="cooldown_${label}" value="${msToStr(labels[label]?.[0] ?? defCooldown)}" style="width: 48px; margin-left: 8px;">

                            <input type="text" id="price_${label}" value="${minPrices[label] ?? 0}" style="width: 48px; margin-left: 8px;">
                        </div>
                    `).join('')}
                </div>
            </fieldset>
            <div style="display: flex; flex-direction: column; gap: 8px; min-width: 120px;">
                <label>
                    <span>Default Cooldown</span><br>
                    <input type="text" id="default_cooldown" value="${msToStr(wocc)}" style="width: 60px; color: black;">
                </label>
                <label>
                    <span>Captcha Cooldown</span><br>
                    <input type="text" id="captcha_cooldown" value="${msToStr(wcc)}" style="width: 60px; color: black;">
                </label>
                <button class="kd-update-btn" id="form_send">Update</button>
            </div>
        </div>
    `;
    document.body.appendChild(panel);

    // Add checkbox event listeners
    panel.querySelectorAll('.label-toggle').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const labelId = this.id.replace('enabled_', '');
            logger.debug(`Toggle ${labelId}: ${this.checked ? 'enabled' : 'disabled'} 🔧`);
        });
    });

    // Toggle panel visibility on gear icon click
    gearBtn.onclick = () => {
        if (panel.style.display == "block") {
            panel.style.display = "none"
            logger.info(`Closing settings panel. 🛠️`);
            return;
        }

        // Reload current settings from localStorage every time we open the panel
        let currentLabels = JSON.parse(localStorage.getItem('labels')) || labelFlagsDefault;
        let minPrices = JSON.parse(localStorage.getItem('minPrices')) || minPricesDefault;
        let currentWocc = parseInt(localStorage.getItem('wocc')) || woccDefault;
        let currentWcc = parseInt(localStorage.getItem('wcc')) || wccDefault

        logger.info(`Opening settings panel. 🛠️`);

        // Update panel HTML with current values
        const labelsList = panel.querySelector('.kd-labels-list');
        labelsList.innerHTML = Object.entries(labelFlagsDefault).map(([label, [defCooldown]]) => `
            <div class="kd-label-row">
                <label style="display: flex; align-items: center; gap: 6px;">
                    <input type="checkbox" class="label-toggle" id="enabled_${label}" ${currentLabels[label]?.[1] ? "checked" : ""}>
                    <span>${label}</span>
                </label>
                <input type="text" id="cooldown_${label}" value="${msToStr(currentLabels[label]?.[0] ?? defCooldown)}"
                    style="width: 48px; margin-left: 8px;">
                <input type="text" id="price_${label}" value="${minPrices[label] ?? 0}" style="width: 48px; margin-left: 8px;">
            </div>
        `).join('');

        // Update cooldown values
        panel.querySelector('#default_cooldown').value = msToStr(currentWocc);
        panel.querySelector('#captcha_cooldown').value = msToStr(currentWcc);

        // Re-attach checkbox event listeners
        panel.querySelectorAll('.label-toggle').forEach(checkbox => {
            checkbox.addEventListener('change', function() {
                const labelId = this.id.replace('enabled_', '');
                const cooldownInput = document.getElementById(`cooldown_${labelId}`);
                cooldownInput.disabled = !this.checked;
                logger.debug(`Toggle ${labelId}: ${this.checked ? 'enabled' : 'disabled'} 🔧`);
            });
        });

        panel.style.display = "block";
    };

    // Hide panel if clicked outside
    document.addEventListener('mousedown', (e) => {
        if (panel.style.display === "block" && !panel.contains(e.target) && !gearBtn.contains(e.target)) {
            panel.style.display = "none";
            logger.info(`Settings panel closed. 🙈`);
        }
    });

    // Handle settings update button
    document.getElementById('form_send').onclick = function() {
        // Gather all values
        const updatedLabels = {};
        for (const label of Object.keys(labelFlagsDefault)) {
            const enabled = document.getElementById(`enabled_${label}`).checked;
            const cooldownStr = document.getElementById(`cooldown_${label}`).value;
            updatedLabels[label] = [strToMs(cooldownStr), enabled];
        }
        const updatedPrices = {};
        for (const label of Object.keys(minPricesDefault)) {
          const priceValue = document.getElementById(`price_${label}`)?.value;
          if (priceValue !== undefined) {
            const priceNum = parseFloat(priceValue);
            updatedPrices[label] = isNaN(priceNum) ? 0 : priceNum;
          }
        }
        const updatedWocc = strToMs(document.getElementById('default_cooldown').value);
        const updatedWcc = strToMs(document.getElementById('captcha_cooldown').value);

        // Save to localStorage
        localStorage.setItem('labels', JSON.stringify(updatedLabels));
        localStorage.setItem('minPrices', JSON.stringify(updatedPrices));
        localStorage.setItem('wocc', updatedWocc);
        localStorage.setItem('wcc', updatedWcc);

        logger.success(`Settings saved successfully! 🎀 Reloading page...`);
        panel.style.display = "none";

        // Show notification and reload
        const notif = document.createElement('div');
        notif.textContent = "✨ Settings updated! Reloading... ✨";
        notif.style = `
            position: fixed; bottom: 120px; right: 10px; background: #23272b; color: #ffb400;
            padding: 10px 18px; border-radius: 8px; font-size: 16px; z-index: 10002;
            box-shadow: 0 2px 8px #0005; opacity: 0.95;`;
        document.body.appendChild(notif);

        // Short delay before reload
        setTimeout(() => location.reload(), 500);
    };
}

/**
 * SECTION 6: WEBSOCKET COMMUNICATION
 * Sets up communication with external control application
 */
async function setupWebSocket() {
    if (BYPASS_WEBSOCKET) {
        logger.info(`WebSocket bypassed. Running in standalone mode. 🦊`);
        socketConnected = false;
        handlePage();
        return;
    }

    logger.ws(`Attempting to connect to WebSocket server... 🔌`);
    const socket = new WebSocket('ws://localhost:54321');

    // Connection established handler
    socket.onopen = () => {
        logger.ws(`Connection established! Yay! 🎉`);
        socketConnected = true;
        handlePage();
    };

    // Error handler
    socket.onerror = (error) => {
        logger.error(`WebSocket error detected: ${error} 💔`);
    };

    // Connection closed handler with retry
    socket.onclose = (event) => {
        logger.warn(
            `WebSocket closed (Code: ${event.code}, Reason: ${event.reason || 'No reason provided'}). Retrying in 5 seconds... 🔄`
        );
        setTimeout(() => setupWebSocket(), 5000);
    };

    // Message handler
    socket.onmessage = (event) => {
        try {
            const data = JSON.parse(event.data);
            logger.ws(`Received data: ${JSON.stringify(data).slice(0, 100)}... 📩`);

            // Handle requests for label information
            if (data.action === 'get_labels') {
                const labels = JSON.parse(localStorage.getItem('labels'));

                const responseData = {
                    action: 'set_labels',
                    labels: [
                        ['AMATEUR', labels.AMATEUR[0], labels.AMATEUR[1]],
                        ['CONTENDER', labels.CONTENDER[0], labels.CONTENDER[1]],
                        ['CHAMPION', labels.CHAMPION[0], labels.CHAMPION[1]],
                        // ['LEGEND', labels.LEGEND[0], labels.LEGEND[1]],
                        ['CHALLENGER', labels.CHALLENGER[0], labels.CHALLENGER[1]]
                    ],
                    wo_captcha_cooldown: parseInt(localStorage.getItem('wocc')),
                    w_captcha_cooldown: parseInt(localStorage.getItem('wcc')),
                };

                socket.send(JSON.stringify(responseData));
                logger.ws(`Sent label settings back to server~ 📤`);
            }
            // Handle label updates from external app
            else if (data.action === "update_labels") {
                logger.ws(`Received updated settings from server! 📝`);
                const labelsObject = Object.fromEntries(data.labels.map(([key, value1, value2]) => [key, [value1, value2]]));
                localStorage.setItem('labels', JSON.stringify(labelsObject));
                localStorage.setItem('wocc', data.wo_captcha_cooldown);
                localStorage.setItem('wcc', data.w_captcha_cooldown);
                logger.success(`Settings updated from server! Reloading page... ✨`);
                location.reload();
            }
        } catch (error) {
            logger.error(`Failed to parse WebSocket message: ${error}.`);
        }
    };
}

/**
 * SECTION 7: CORE FUNCTIONALITY
 * Main logic for handling giveaways
 */
// Get filtered list of label texts
const labelTexts = Object.keys(labelFlagsDefault).filter((label) => labelFlagsDefault[label]);

// Find buttons corresponding to a specific giveaway category
function findButtonsByLabelText(labelText) {
    const labels = document.querySelectorAll(
        'p[data-testid="label-single-card-giveaway-category"]'
    );
    for (const label of labels) {
        if (label.textContent.trim() === labelText) {
            const parentDiv = label.closest(
                'div[data-testid="div-active-giveaways-list-single-card"]'
            );
            if (parentDiv) {
                const button = parentDiv.querySelector(
                    'a[data-testid="btn-single-card-giveaway-join"]'
                );
                if (button) {
                    logger.giveaway(`Found button for category "${labelText}"! 🎯`);
                    return button;
                }
            }
        }
    }

    logger.warn(`No button found for category "${labelText}".`);
    return null;
}

// Find price corresponding to a specific giveaway category
function findPriceByLabelText(labelText) {
    const labels = document.querySelectorAll(
      'p[data-testid="label-single-card-giveaway-category"]'
    );

    for (const label of labels) {
      if (label.textContent.trim() === labelText) {
        const parentDiv = label.closest(
          'div[data-testid="div-active-giveaways-list-single-card"]'
        );
        if (parentDiv) {
          // Find the price div inside the same card
          const priceDiv = parentDiv.querySelector(
            'div[data-testid="label-single-card-giveaway-reward-value-amount"]'
          );
          if (priceDiv) {
            const span = priceDiv.querySelector('span[data-testid=""]');
            if (span) {
              const price = span.textContent.trim();
              logger.giveaway(`Found price for category "${labelText}": ${price} 💰`);

              const numericString = price.replace(/[^\d.]/g, '');
              const priceInt = parseFloat(numericString);

              return priceInt;
            }
          }
        }
      }
    }

    logger.warn(`No price found for category "${labelText}".`);
    return null;
  }

// Check if a captcha is present on the page
function checkForCaptcha() {
    return document.querySelector(
        'iframe[src*="captcha"], iframe[src*="recaptcha"], .g-recaptcha'
    );
}

// Get current label settings from localStorage
function getLabelSettings() {
    const labels = JSON.parse(localStorage.getItem('labels'));
    const minPrices = JSON.parse(localStorage.getItem('minPrices'));
    const wocc = parseInt(localStorage.getItem('wocc'));
    const wcc = parseInt(localStorage.getItem('wcc'));

    const frequencies = {};
    for (const [label, [cooldown, enabled]] of Object.entries(labels)) {
      if (enabled) {
        frequencies[label] = cooldown;
      }
    }

    return {
      labelFrequencies: frequencies,
      enabledLabels: Object.keys(frequencies),
      wocc,
      wcc,
      minPrices
    };
  }

// Handle captcha detection and resolution
async function handleCaptcha(button) {
    const settings = getLabelSettings();
    logger.captcha(`Watching for captcha... 👀`);

    const captchaDetected = await new Promise((resolve) => {
        const observer = new MutationObserver(() => {
            if (checkForCaptcha()) {
                observer.disconnect();
                resolve(true);
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });

        timeout(settings.wocc).then(() => {
            observer.disconnect();
            resolve(false);
        });
    });

    if (captchaDetected) {
        logger.captcha(`Captcha detected! Waiting for solution... ⏳`);
        await timeout(settings.wcc - 2000);
        if (button) {
            logger.captcha(`Clicking giveaway button after captcha delay. 🖱️`);
            button.click();
        }
        await timeout(2000);
    } else {
        logger.captcha(`No captcha detected, proceeding! 🚀`);
    }

    // Return to previous page rather than using replace
    logger.info(`Going back to giveaway list~ 🔙`);
    window.history.back();
}

// Check if a label can be processed based on cooldown
function canProcessLabel(labelText) {
    const lastAttemptKey = `lastAttempt_${labelText}`;
    const lastAttempt = localStorage.getItem(lastAttemptKey);
    const now = Date.now();
    const settings = getLabelSettings();

    if (!lastAttempt ||
        now - parseInt(lastAttempt, 10) >= settings.labelFrequencies[labelText]) {
        localStorage.setItem(lastAttemptKey, now);
        return true;
    }

    const timeLeft = parseInt(lastAttempt) + settings.labelFrequencies[labelText] - now;
    //logger.info(`Cooldown for ${labelText} still active. ${msToStr(timeLeft)} remaining ⏱️`);
    return false;
}

// Main page handler function
async function handlePage() {
    // Skip if using WebSocket mode but not connected
    if (!socketConnected && !BYPASS_WEBSOCKET) {
        logger.warn(`WebSocket mode enabled but not connected! Waiting... ⌛`);
        return;
    }

    const settings = getLabelSettings();
    const currentPath = window.location.pathname;
    const offset = Math.random() * 1000 + 200; // Random delay for human-like behavior

    // Handle giveaways list page
    if (currentPath.includes("/giveaways/list")) {
        logger.info(`You are on the giveaways list page! 📜`);

        await waitForElement(
            'p[data-testid="label-single-card-giveaway-category"]'
        );

        let storedIndex = parseInt(
            localStorage.getItem("giveawayIndex") || "0",
            10
        );
        let processed = false;

        // Try to process giveaways in sequence until one is successful
        while (!processed) {
            const labelText = settings.enabledLabels[storedIndex];

            if (labelText && canProcessLabel(labelText)) {
                logger.giveaway(`Looking for ${labelText} giveaway to join... 🔍`);
                const button = findButtonsByLabelText(labelText);
                const price = findPriceByLabelText(labelText);

                const minPrice = settings.minPrices[labelText];

                if (button && price >= minPrice) {

                    await timeout(offset);
                    logger.giveaway(`Joining a ${labelText} giveaway! 🎮`);
                    button.click();

                    const currentIndex = (storedIndex + 1) % settings.enabledLabels.length;
                    localStorage.setItem("giveawayIndex", currentIndex);
                    logger.debug(`Updated index to ${currentIndex} for next run 🔄`);
                    processed = true;
                } else {
                    logger.warn(`No ${labelText} giveaways available right now.`);
                }
            }

            storedIndex = (storedIndex + 1) % settings.enabledLabels.length;
            if (!processed) await timeout(1000);
        }
    }
    // Handle individual giveaway page
    else if (currentPath.includes("/giveaways/keydrop")) {
        logger.info(`You are on a giveaway details page! 🎁`);

        await waitForElement('div[data-testid="div-giveaway-participants-board"]');
        const button = document.querySelector(
            'button[data-testid="btn-giveaway-join-the-giveaway"]'
        );

        if (button && button.disabled) {
            logger.warn(`Giveaway button is disabled. Already joined? Going back... 🔙`);
            window.history.back();
        } else if (button) {
            await timeout(offset);
            logger.giveaway(`Clicking giveaway join button! 🎯`);
            button.click();
            await handleCaptcha(button);
        } else {
            logger.error(`No giveaway button found! Going back... (◞‸◟；)`);
            window.history.back();
        }
    } else {
        logger.info(`You are on an unsupported page. Looking for giveaways elsewhere. 🔎`);
    }
}

/**
 * SECTION 8: INITIALIZATION AND EVENT HANDLERS
 * Start the script and setup page change detection
 */
(async () => {
    logger.info(`🌈 KeyDrop Giveaway Bot starting up! Version ${GM_info.script.version} 🌈`);
    setupConsoleClearing(); // Set up automatic console clearing
    await setupWebSocket();

    let lastPath = window.location.pathname;

    // Listen for browser navigation (back/forward)
    window.addEventListener('popstate', () => {
        if (window.location.pathname !== lastPath) {
            lastPath = window.location.pathname;
            logger.info(`URL changed via navigation to: ${lastPath} 🔄`);
            handlePage();
        }
    });

    // Listen for hash changes
    window.addEventListener('hashchange', () => {
        if (window.location.pathname !== lastPath) {
            lastPath = window.location.pathname;
            logger.info(`Hash changed, new path: ${lastPath} #️⃣`);
            handlePage();
        }
    });

    // Watch for DOM changes that might indicate page navigation
    new MutationObserver(() => {
        if (window.location.pathname !== lastPath) {
            lastPath = window.location.pathname;
            logger.info(`Page navigation detected to: ${lastPath} 🧭`);
            handlePage();
        }
    }).observe(document, { subtree: true, childList: true });

    logger.success(`Bot initialization complete! Ready to find giveaways.`);

    // === AUTO RELOAD EVERY 15 MINUTES TO REFRESH JWT ===
    setInterval(() => {
        logger.info('Automatic page reload to renew the JWT token. 🔄');
        location.reload();
    }, 15 * 60 * 1000); // 15 minutes
})();
