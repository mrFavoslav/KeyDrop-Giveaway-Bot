// ==UserScript==
// @name         keydrop_giveaway_script
// @namespace    https://www.favoslav.cz/
// @version      0.6
// @description  KeyDrop Giveaway Bot with Dynamic Label Updates and WebSocket Support
// @author       Favoslav_ & Pr0Xy
// @include      *://*key*drop*/*
// @grant        none
// ==/UserScript==

const labelFlags = {
    CHAMPION: false,
    CHALLENGER: false,
    LEGEND: true,
    CONTENDER: true,
    AMATEUR: true
}; // true or false

const labelTexts = Object.keys(labelFlags).filter(label => labelFlags[label]);

(async () => {
    function waitForElement(selector, timeout = 15000) {
        return new Promise((resolve, reject) => {
            const startTime = Date.now();
            let element = document.querySelector(selector);
            if (element) return resolve(element);

            const observer = new MutationObserver(() => {
                element = document.querySelector(selector);
                if (element) {
                    observer.disconnect();
                    resolve(element);
                } else if (Date.now() - startTime > timeout) {
                    observer.disconnect();
                    reject(new Error(`Element with selector "${selector}" not found within ${timeout}ms`));
                }
            });

            observer.observe(document.body, { childList: true, subtree: true });
        });
    }

    function findButtonsByLabelText(labelText) {
        const labels = document.querySelectorAll('p[data-testid="label-single-card-giveaway-category"]');
        for (const label of labels) {
            if (label.textContent.trim() === labelText) {
                const parentDiv = label.closest('div[data-testid="div-active-giveaways-list-single-card"]');
                if (parentDiv) {
                    const button = parentDiv.querySelector('a[data-testid="btn-single-card-giveaway-join"]');
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
        const captchaIframe = document.querySelector('iframe[src*="captcha"]');
        const recaptchaIframe = document.querySelector('iframe[src*="recaptcha"]');
        const captchaElement = document.querySelector('.g-recaptcha');

        if (captchaIframe || captchaElement || recaptchaIframe) {
            return true;
        }
        return false;
    }

    async function handlePage() {
        const currentPath = window.location.pathname;
        const offset = (Math.random() + 1.2);

        if (currentPath.includes('/giveaways/list')) {
            console.log("You are on the /giveaways/list page");
            await waitForElement('p[data-testid="label-single-card-giveaway-category"]');

            let storedIndex = localStorage.getItem('giveawayIndex');
            if (storedIndex === null || storedIndex === undefined) {
                storedIndex = 0; // Default to the first index if not set
            } else {
                storedIndex = parseInt(storedIndex, 10); // Ensure it's a valid number
            }
            const labelText = labelTexts[storedIndex];
            console.log(`Current labelText: ${labelText}`);

            const button = findButtonsByLabelText(labelText);

            if (button) {
                await new Promise(r => setTimeout(r, (500 * offset)));
                button.click(); // Trigger click after finding the button

                let currentIndex = (storedIndex + 1) % labelTexts.length;
                localStorage.setItem('giveawayIndex', currentIndex); // Update index in localStorage
                console.log(`Updated index to ${currentIndex}`);
                await new Promise(r => setTimeout(r, (100)));
            } else {
                console.log(`No button found for "${labelText}", skipping to next index.`);
                let currentIndex = (storedIndex + 1) % labelTexts.length;
                localStorage.setItem('giveawayIndex', currentIndex); // Update index even if no button is found
            }
        } else if (currentPath.includes('/giveaways/keydrop')) {
            console.log("You are on the /giveaways/keydrop page");

            await waitForElement('div[data-testid="div-giveaway-participants-board"]');
            const button = document.querySelector('button[data-testid="btn-giveaway-join-the-giveaway"]');

            if (button && button.disabled) {
                await new Promise(r => setTimeout(r, (500)));
                const baseUrl = window.location.origin;
                window.location.replace(`${baseUrl}/giveaways/list/`);
            } else if (button) {
                await new Promise(r => setTimeout(r, (500 * offset)));
                button.click();
                await new Promise(r => setTimeout(r, (10000)));
                if (checkForCaptcha()) {
                    console.log("CAPTCHA detected.");
                    button.click();
                    await new Promise(r => setTimeout(r, (30000)));
                    const baseUrl = window.location.origin;
                    window.location.replace(`${baseUrl}/giveaways/list/`);
                } else {
                    console.log("CAPTCHA not detected.");
                    const baseUrl = window.location.origin;
                    window.location.replace(`${baseUrl}/giveaways/list/`);
                }
            } else {
                await new Promise(r => setTimeout(r, (1000)));
                const baseUrl = window.location.origin;
                window.location.replace(`${baseUrl}/giveaways/list/`);
            }

        } else {
            console.log("You are on an unsupported page.");
        }
    }

    handlePage();

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
            console.log('Detected page change via MutationObserver. Re-running script...');
            handlePage();
        }
    }).observe(document, { subtree: true, childList: true });

})();