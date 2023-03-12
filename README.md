# OculusGooglePhotos
Google Photos viewer for Oculus Quest.

## Install the app
WIP

## Get started with development
WIP

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
