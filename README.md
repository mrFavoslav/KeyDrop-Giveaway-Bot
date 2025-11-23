# KeyDrop Giveaway Bot

KeyDrop-BOT is a bot designed to interact with the **KeyDrop** platform, automate and perform tasks related to giveaways. It can automatically join giveaways based on the set interval. This project includes a **client-side JavaScript** script and a **server-side C# app** application, which work together to perform the desired tasks.

<a id="manual"></a>
# 📑 Manual

This bot supports any language and currency. However, if you encounter any issues or buggy behavior, please try switching to English and EUR.

## Entering Prices for "Min Prices"

The script should accept multiple price formats. If you experience problems, use the following format:

- Do not use commas to separate thousands (e.g., write 10000 instead of 10,000).
- Use a period (.) as the decimal separator for decimal numbers (e.g., 12.50).


## 🛑 Warning

The app may work but is currently not officially supported. If you encounter issues, switch the script to [Standalone Mode](#standalone-mode).

## Features

### KeyDrop Bot Overview

- **Server-side C# Application**: 
  The server handles settings, interacting with the Client-side JavaScript. It hosts a WebSocket server that allows users to set parameters like cooldowns, giveaways to join, and other configuration options.

- **Client-side JavaScript**: 
  The client script, running through Tampermonkey, automates interactions on the KeyDrop website. It listens for settings from the server and performs automated tasks on the KeyDrop website.

- **Release Pipeline**: 
  The release pipeline, powered by GitHub Actions, automates the building and releasing of the latest versions of both the server-side application and the client-side script.

## Project Structure

```
KeyDrop-BOT/
├── .github/
│   └── workflows/
│       └── build-and-release.yml    # GitHub Actions workflow for building and releasing
├── app/
│   └── Guidzgo.sln                 # C# Solution file for the server-side application
├── script/
│   └── giveaway_bot.user.js        # The main JavaScript client script
├── .gitignore                      # Git ignore configuration
├── README.md                       # Project readme (this file)
```

<a id="support"></a>
## Support

If you need help you can contact me, my contact info is here https://www.favoslav.cz

## Getting Started

### Operating Modes

- [Standalone Mode (Script Only)](#standalone-mode)
- [App Control (App + Script)](#server-mode)

---

<a id="standalone-mode"></a>
### Standalone Mode

> **Use this if you only want the script and don’t need the C# app!**

- **Required:**  
  - `KeyDropBOT_client.js` (the Tampermonkey script)

**Instructions:**

- Refer to [Script Instructions](#script)

**Settings**

   > 🛑 Click on the gear icon for settings (for standalone mode the button should say "Standalone")

   ![Standalone 2](https://api.favoslav.cz/v1/assets/keydropbot/standalone/2.png)

   ![Standalone 3](https://api.favoslav.cz/v1/assets/keydropbot/standalone/3.png)

   > This setting will appear on keydrop site after the script is installed
---

<a id="server-mode"></a>
### App Control

> **Use this if you want advanced features and remote control!**

- **Required:**  
  - `KeyDropBOT_client.js` (the Tampermonkey script)  
  - `KeyDropBOT_server.exe` (the C# server application)

**Instructions:**
- Refer to [App Instructions](#app)

**Settings**

   > 🛑 For server mode the button should say "App Control"

   ![Standalone 1](https://api.favoslav.cz/v1/assets/keydropbot/standalone/1.png)

   > This button will appear on keydrop site after the script is installed

---

*You can jump between [Standalone Mode](#standalone-mode) and [Server Mode](#server-mode) anytime! If you need help, just [contact me](#support)*

---

<a id="script"></a>

### Script

1. Download the latest `KeyDropBOT_client.js` from [releases](https://github.com/mrFavoslav/KeyDrop-BOT/releases).

2. Install Tampermonkey:

   ![Tampermonkey Image 1](https://api.favoslav.cz/v1/assets/keydropbot/monkey/1.png)

   You need to enable Developer mode so Tempermonkey works properly
   
      2.1. for Chrome

      Open this page: Chrome://extensions
      And on the top right you see the toggle button to enable the "Developer mode".

      2.2. For other browsers

      Search it on the internet or follow the tempermonkey guide how to enable dev mode for your browser, if you need help you can contact me, my contact info is here https://www.favoslav.cz


3. Find Tampermonkey in extensions:

   ![Tampermonkey Image 2](https://api.favoslav.cz/v1/assets/keydropbot/monkey/2.png)

4. Pin Tampermonkey:

   ![Tampermonkey Image 3](https://api.favoslav.cz/v1/assets/keydropbot/monkey/3.png)

5. Click on the pinned extension and go to the dashboard:

   ![Tampermonkey Image 4](https://api.favoslav.cz/v1/assets/keydropbot/monkey/4.png)

6. Go to **Utilities**:

   ![Tampermonkey Image 5](https://api.favoslav.cz/v1/assets/keydropbot/monkey/5.png)

7. Import the downloaded `KeyDropBOT_client.js` file:

   ![Tampermonkey Image 6](https://api.favoslav.cz/v1/assets/keydropbot/monkey/6.png)

8. Install the script:

   ![Tampermonkey Image 7](https://api.favoslav.cz/v1/assets/keydropbot/monkey/7.png)

9. Then go to KeyDrop, click on the extension, and you should see the script. Activate it and refresh your page with F5.

10. (This should be optional, if you experience issues with captcha try installing this) You also need this extension [NopeCHA Chrome](https://chromewebstore.google.com/detail/nopecha-captcha-solver/dknlfmjaanfblgfdfebhijalfmhmjjjo). For other browser search "your browser nopecha extension" eg. "firefox nopecha extension"

<a id="app"></a>
### App

1. Download the latest `KeyDropBOT_server.exe` from [releases](https://github.com/mrFavoslav/KeyDrop-BOT/releases).

2. Find the downloaded exe file and run it.

   2.0 Why does this happen?
   
   - Windows Defender may block the application due to it being signed but not by a well-known authority. In this case, you can click on "More info" and then "Run anyway".

   2.1. Click on the "More information" button:  
   
      ![App Image 1](https://api.favoslav.cz/v1/assets/keydropbot/app/1.png)

   2.2. Click on the "Run Anyway" button:  
   
      ![App Image 2](https://api.favoslav.cz/v1/assets/keydropbot/app/2.png)

5. How does the app work?:

   ![App Image 1](https://api.favoslav.cz/v1/assets/keydropbot/app/3.png)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support us

1. Referral

    <table>
      <tr>
        <td>
          <a href="https://key-drop.com/?code=FVSLV_">
            <img src="https://api.favoslav.cz/v1/assets/keydropbot/banner/1.png" alt="KeyDrop PROMO" width="150">
          </a>
        </td>
        <td style="vertical-align: middle; padding-left: 20px;">
            <a href="https://key-drop.com/?code=FVSLV_" style="margin-left: 20px; line-height: 200px;">
                Click here to visit KeyDrop with my referral code!
            </a>
        </td>
      </tr>
    </table>

2. KO-FI
   
    <a href="https://ko-fi.com/Y8Y7MIGB1"><img src="https://storage.ko-fi.com/cdn/kofi3.png?v=3" height="40" ></a>


