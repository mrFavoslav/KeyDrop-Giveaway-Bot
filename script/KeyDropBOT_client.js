// ==UserScript==
// @name         keydrop_giveaway_script
// @namespace    https://www.favoslav.cz/
// @version      1.0.6
// @description  KeyDrop Giveaway Bot
// @author       Favoslav_ & Pr0Xy
// @include      *://*key*drop*/*
// @grant        GM_info
// @updateURL    https://raw.githubusercontent.com/mrFavoslav/KeyDrop-Giveaway-Bot/refs/heads/main/script/KeyDropBOT_client.js
// @downloadURL  https://raw.githubusercontent.com/mrFavoslav/KeyDrop-Giveaway-Bot/refs/heads/main/script/KeyDropBOT_client.js
// ==/UserScript==

if (!localStorage.getItem('script_version')) {
    localStorage.setItem('script_version', GM_info.script.version);
}

if (localStorage.getItem('script_version') != GM_info.script.version) {
    if (confirm("New version available! To access new features, please update your app. Click 'OK' to visit the download page.")) {
        window.location.href = "https://github.com/mrFavoslav/KeyDrop-Giveaway-Bot";
    }
    localStorage.setItem('script_version', GM_info.script.version);
}

// Toggle this to true to bypass WebSocket requirement
const BYPASS_WEBSOCKET = false;

let socketConnected = false;

const labelFlagsDefault = {
    AMATEUR: [60000, false],
    CONTENDER: [300000, false],
    //LEGEND: [900000, false],
    CHALLENGER: [3600000, false],
    CHAMPION: [21600000, false],
};

const woccDefault = 15000;
const wccDefault = 30000;

if ((!localStorage.getItem('labels') || !localStorage.getItem('wocc') || !localStorage.getItem('wcc')) && !BYPASS_WEBSOCKET) {
    localStorage.setItem('labels', JSON.stringify(labelFlagsDefault));
    localStorage.setItem('wocc', woccDefault);
    localStorage.setItem('wcc', wccDefault);
} else if (BYPASS_WEBSOCKET) {
    localStorage.setItem('labels', JSON.stringify(labelFlagsDefault));
    localStorage.setItem('wocc', woccDefault);
    localStorage.setItem('wcc', wccDefault);
}

async function setupWebSocket() {
    if (BYPASS_WEBSOCKET) {
        console.log('WebSocket bypassed. Running in standalone mode.');
        socketConnected = true;
        handlePage();
        return;
    }

    const socket = new WebSocket('ws://localhost:54321');

    socket.onopen = () => {
        console.log('WebSocket connection established.');
        socketConnected = true;
        handlePage();
    };

    socket.onerror = (error) => {
        console.error('WebSocket error detected:', error);
    };

    socket.onclose = (event) => {
        console.warn(
            `WebSocket closed (Code: ${event.code}, Reason: ${event.reason}). Retrying in 5 seconds...`
        );
        setTimeout(() => setupWebSocket(), 5000);
    };

    socket.onmessage = (event) => {
        try {
            const data = JSON.parse(event.data);
            console.log('Received WebSocket data:', data);

            if (data.action === 'get_labels') {
                const labels = JSON.parse(localStorage.getItem('labels'));

                const responseData = {
                    action: 'set_labels',
                    labels: [
                        ['AMATEUR', labels.AMATEUR[0], labels.AMATEUR[1]],
                        ['CONTENDER', labels.CONTENDER[0], labels.CONTENDER[1]],
                        ['CHAMPION', labels.CHAMPION[0], labels.CHAMPION[1]],
                        //['LEGEND', labels.LEGEND[0], labels.LEGEND[1]],
                        ['CHALLENGER', labels.CHALLENGER[0], labels.CHALLENGER[1]]
                    ],
                    wo_captcha_cooldown: parseInt(localStorage.getItem('wocc')),
                    w_captcha_cooldown: parseInt(localStorage.getItem('wcc')),
                };

                socket.send(JSON.stringify(responseData));
                console.log('Sent labels back:', responseData);
            } else if (data.action === "update_labels") {
                const labelsObject = Object.fromEntries(data.labels.map(([key, value1, value2]) => [key, [value1, value2]]));
                localStorage.setItem('labels', JSON.stringify(labelsObject));
                localStorage.setItem('wocc', data.wo_captcha_cooldown);
                localStorage.setItem('wcc', data.w_captcha_cooldown);
                location.reload();
            }
        } catch (error) {
            console.error('Error parsing WebSocket message:', error);
        }
    };
}

async function timeout(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
}

const labelTexts = Object.keys(labelFlagsDefault).filter((label) => labelFlagsDefault[label]);

function waitForElement(selector, timeoutMs = 15000) {
    return new Promise((resolve, reject) => {
        const startTime = Date.now();
        const element = document.querySelector(selector);
        if (element) return resolve(element);

        const observer = new MutationObserver(() => {
            const el = document.querySelector(selector);
            if (el) {
                observer.disconnect();
                resolve(el);
            } else if (Date.now() - startTime > timeoutMs) {
                observer.disconnect();
                reject(
                    new Error(
                        `Element with selector "${selector}" not found within ${timeoutMs}ms`
                    )
                );
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });
    });
}

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
                    console.log(`Found button for category "${labelText}":`, button);
                    return button;
                }
            }
        }
    }

    console.warn(`No button found for category "${labelText}".`);
    return null;
}

function checkForCaptcha() {
    return document.querySelector(
        'iframe[src*="captcha"], iframe[src*="recaptcha"], .g-recaptcha'
    );
}

function getLabelSettings() {
    const labels = JSON.parse(localStorage.getItem('labels'));
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
        wcc
    };
}

async function handleCaptcha(button) {
    const settings = getLabelSettings();
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
        console.log("CAPTCHA detected. Waiting for bot to solve it...");
        await timeout(settings.wcc - 2000);
        if (button) button.click();
        await timeout(2000);
    } else {
        console.log("No CAPTCHA detected. Proceeding...");
    }

    //window.location.replace(`${window.location.origin}/giveaways/list/`);
    window.history.back();
}

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

    return false;
}

async function handlePage() {
    if (!socketConnected && !BYPASS_WEBSOCKET) {
        return;
    }

    const settings = getLabelSettings();
    const currentPath = window.location.pathname;
    const offset = Math.random() * 1000 + 200;

    if (currentPath.includes("/giveaways/list")) {
        console.log("You are on the /giveaways/list page");
        await waitForElement(
            'p[data-testid="label-single-card-giveaway-category"]'
        );

        let storedIndex = parseInt(
            localStorage.getItem("giveawayIndex") || "0",
            10
        );
        let processed = false;

        while (!processed) {
            const labelText = settings.enabledLabels[storedIndex];

            if (labelText && canProcessLabel(labelText)) {
                console.log(`Processing labelText: ${labelText}`);
                const button = findButtonsByLabelText(labelText);

                if (button) {
                    await timeout(offset);
                    button.click();

                    const currentIndex = (storedIndex + 1) % settings.enabledLabels.length;
                    localStorage.setItem("giveawayIndex", currentIndex);
                    console.log(`Updated index to ${currentIndex}`);
                    processed = true;
                } else {
                    console.log(
                        `No button found for "${labelText}", skipping to next index.`
                    );
                }
            }

            storedIndex = (storedIndex + 1) % settings.enabledLabels.length;
            await timeout(1000);
        }
    } else if (currentPath.includes("/giveaways/keydrop")) {
        console.log("You are on the /giveaways/keydrop page");

        await waitForElement('div[data-testid="div-giveaway-participants-board"]');
        const button = document.querySelector(
            'button[data-testid="btn-giveaway-join-the-giveaway"]'
        );

        if (button && button.disabled) {
            console.log("Giveaway button disabled. Redirecting...");
            //window.location.replace(`${window.location.origin}/giveaways/list/`);
            window.history.back();
        } else if (button) {
            await timeout(offset);
            button.click();
            await handleCaptcha(button);
        } else {
            console.log("No button found. Redirecting...");
            //window.location.replace(`${window.location.origin}/giveaways/list/`);
            window.history.back();
        }
    } else {
        console.log("You are on an unsupported page.");
    }
}

(async () => {
    await setupWebSocket();

    let lastPath = window.location.pathname;

    window.addEventListener('popstate', () => {
        if (window.location.pathname !== lastPath) {
            lastPath = window.location.pathname;
            console.log('URL changed. Re-running script...');
            handlePage();
        }
    });

    window.addEventListener('hashchange', () => {
        if (window.location.pathname !== lastPath) {
            lastPath = window.location.pathname;
            console.log('Hash changed. Re-running script...');
            handlePage();
        }
    });

    new MutationObserver(() => {
        if (window.location.pathname !== lastPath) {
            lastPath = window.location.pathname;
            console.log('Page change detected. Re-running script...');
            handlePage();
        }
    }).observe(document, { subtree: true, childList: true });
})();
