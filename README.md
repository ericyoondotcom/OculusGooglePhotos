# VR Gallery for Google Photos
Google Photos viewer for Meta Quest.

[<img src="https://gallery.yoonicode.com/resources/sidequest-badge.png" alt="Get it on SideQuest" height="50px">](https://gallery.yoonicode.com/resources/sidequest-badge.png)


- View flat and 360° photos and videos from Google Photos
- Your photos data never leaves your device
- 100% open source

Learn more at [gallery.yoonicode.com](https://gallery.yoonicode.com) • Made by [Eric Yoon](https://yoonicode.com) • Contributions welcome—see LICENSE
## Install the app
### Install from SideQuest
1. Download the SideQuest desktop app and configure your Meta Quest by following [this guide](https://sidequestvr.com/setup-howto).
2. [Get the latest app on SideQuest](https://sidequestvr.com/app/16980/vr-gallery-for-google-photos).

### Install the APK using ADB
1. Make sure you have the `adb` command line tool installed. [(Installation guide)](https://www.xda-developers.com/install-adb-windows-macos-linux/)
2. Make sure your Meta Quest is in developer mode. [(Guide)](https://learn.adafruit.com/sideloading-on-oculus-quest/enable-developer-mode)
3. Download the latest APK from the [Releases tab](https://github.com/ericyoondotcom/OculusGooglePhotos/releases/latest).
4. Plug in your Quest to your computer. Accept the prompt on your Quest.
5. In a new shell, type `adb devices` and verify your Quest is listed. Type `adb install path/to/file.apk`.
6. Your app is installed under Unknown Sources in your Quest. 

## Get started with development
### Install tools
- `npm install -g serve`

### Create your own test Google Cloud project
1. Create a project in the Google Cloud Console
2. Under the API Library, enable the Google Photos Picker library
3. Configure your OAuth Consent Screen
    - Authorize the domain `yoonicode.com`
    - Add the scope `https://www.googleapis.com/auth/photospicker.mediaitems.readonly`
4. Under the Credentials page, create a new credential
    - Type: OAuth Client ID
    - Application type: Web application
    - Add the Javascript origins "https://gallery.yoonicode.com" and "http://localhost:1234"
5. Take note of your client ID and client secret

### Set up Firebase
1. Add Firebase to your Google Cloud project
2. Create a new Web client
3. Take note of the Firebase config shown to you

### Get the website running
- `cd OculusGooglePhotos-Web`
- Copy `config.template.js` to `config.js`
    - Paste in your Google client ID
    - Paste in your Firebase Config JSON object
- `npm install`
- `npm start`

### Get Cloud Functions running
- `cd OculusGooglePhotos-Web/functions`
- Copy `config.template.js` to `config.js`
    - Paste in your Google client ID and client secret
- `npm run deploy`, or to emulate locally, `npm run serve`
    - If you plan on using the emulator, you need to set up your admin credentials locally. See [docs](https://firebase.google.com/docs/functions/local-emulator#set_up_admin_credentials_optional). Download your key as `serviceAccountKey.json` and place it in the `functions` directory.

### Get Unity running
- Follow [this guide](https://developer.oculus.com/documentation/unity/unity-gs-overview/) to get your Meta Quest set up with Unity.
- Copy `OculusGooglePhotos-Unity/Assets/Scripts/Constants.template` to `Constants.cs`
    - Paste in your Cloud Functions base URL (such as `https://us-central1-foobar.cloudfunctions.net`)

> **Known problem**: There seems to be a bug with Unity where OpenXR settings are reset every time assets are imported.
> - Add "Meta Quest Touch Controller Plus" and "Meta Quest Touch Controller Pro" to `Enabled interaction profiles`.
> - Enable "Meta Quest Support" under Feature Groups
> - Go to `Project Settings` -> `OpenXR` -> `Meta Quest Support` (cog wheel) -> deselect `Force Remove Internet`.
>   - Tip: use `apktool d my-build.apk && cat my-build/AndroidManifest.xml | grep "INTERNET"` to quickly check.
## Technical details

### Authentication flow

1. User is prompted visit the companion website and enter a code.
2. Website redirects user to Google login page
3. Once authorized, Google redirects user back to companion website with a query parameter of the auth code
4. Companion website trades authorization code for refresh token
5. Companion website prompts user to open a new tab, where they can select photos
5. Companion website polls for the photos picker result
6. Companion website calls a Firebase Cloud Function, saving the refresh token and picker session ID in the database, paired with the code the user entered
7. Unity scene polls a Firebase Function every 5 seconds given its generated code, Firebase Function returns the refresh token and session ID
8. Unity trades in the refresh token for an access token whenever it expires
