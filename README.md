# **!!FOR THE TIME BEING, THIS PIECE OF SOFTWARE IS IN A BUILD STATE. THE SOFTWARE CURRENTLY DOESN'T WORK!!**

# KeyDrop-BOT

KeyDrop-BOT is a bot designed to interact with the **KeyDrop** platform, automate and perform tasks related to giveaways. This project includes a **client-side JavaScript** script and a **server-side C#** application, which work together to perform the desired tasks.

## Features

### KeyDrop Bot Overview

- **Server-side C# Application**: 
  The server handles the core logic and operations, interacting with the Client-side JavaScript. It hosts a WebSocket server that allows users to set parameters like cooldowns, giveaways to join, and other configuration options.

- **Client-side JavaScript**: 
  The client script, running through Tampermonkey, automates interactions on the KeyDrop website. It listens for instructions from the server and performs automated tasks on the KeyDrop website.

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
│   └── KeyDropBOT_client.js        # The main JavaScript client script
├── .gitignore                      # Git ignore configuration
├── README.md                       # Project readme (this file)
```

## Getting Started

### For the Script

1. Download the latest `KeyDropBOT_client.js` from [releases](https://github.com/mrFavoslav/KeyDrop-BOT/releases).

2. Install Tampermonkey:

   ![Tampermonkey Image 1](https://api.favoslav.cz/v1/assets/keydropbot/monkey/1.png)

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

9. Then go to KeyDrop, click on the extension, and you should see the script. Activate it.

### For the App

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

## Referral

1. Support me!

    <a href="https://key-drop.com/?code=FVSLV_">
        <img src="https://api.favoslav.cz/v1/assets/keydropbot/banner/1.png" alt="KeyDrop PROMO" width="200">
    </a>

    [Click here to visit KeyDrop with my referral code!](https://key-drop.com/?code=FVSLV_)
