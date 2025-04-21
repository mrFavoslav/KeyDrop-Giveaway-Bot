// ==UserScript==
// @name         keydrop_giveaway_script
// @namespace    https://www.favoslav.cz/
// @version      1.0.5
// @description  KeyDrop Giveaway Bot
// @author       Favoslav_ & Pr0Xy
// @include      *://*key*drop*/*
// @grant        GM_info
// @require      placeholder
// ==/UserScript==

// Toggle this to true to bypass WebSocket requirement
const BYPASS_WEBSOCKET = false;

if (!localStorage.getItem('script_version')) {
    localStorage.setItem('script_version', GM_info.script.version);
}

if (localStorage.getItem('script_version') != GM_info.script.version) {
    if (confirm("A new script version is available! If you experience any issues, please update your app. Click 'OK' to visit the download page. This message will continue to appear until you update to the new version.")) {
        window.location.href = "https://github.com/mrFavoslav/KeyDrop-Giveaway-Bot";
        localStorage.setItem('script_version', GM_info.script.version);
    }
}