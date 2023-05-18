# OculusGooglePhotos
Google Photos viewer for Meta Quest.

## Install the app
WIP

## Get started with development
### Install tools
- `npm install -g serve`

### Create your own test Google Cloud project
1. Create a project in the Google Cloud Console
2. Under the API Library, enable the Google Photos library
3. Configure your OAuth Consent Screen
    - Authorize the domain `yoonicode.com`
    - Add the scope `https://www.googleapis.com/auth/photoslibrary.readonly`
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
- If you want to be able to test the UI without using your Quest, go into the "Player" scene, click on `UIHelpers > EventSystem` in the heirarchy, and enable `Input System UI Input Module`. You'll have to re-disable the component when you create a build or test on your Quest

## Technical details

### Authentication flow

1. User is prompted visit the companion website and enter a code.
2. Website redirects user to Google login page
3. Once authorized, Google redirects user back to companion website with a query parameter of the auth code
4. Companion website trades authorization code for refresh token
5. Companion website calls a Firebase Function with the refresh token
6. Firebase Function saves the refresh token in the database paired with the code the user entered
7. Unity scene polls a Firebase Function every 5 seconds given its generated code, Firebase Function returns the refresh token
8. Unity trades in the refresh token for an access token whenever it expires
