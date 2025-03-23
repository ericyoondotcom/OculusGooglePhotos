const STEP_ELEMENTS = [
    document.getElementById("step-0"),
    document.getElementById("step-1"),
    document.getElementById("step-2"),
    document.getElementById("step-3"),
    document.getElementById("step-4"),
    document.getElementById("step-5"),
]
function displayStep(stepNum) {
    displayError("");
    STEP_ELEMENTS.forEach((elem, i) => {
        if (i === stepNum) {
            elem.style.display = "block";
        } else {
            elem.style.display = "none";
        }
    })
}

const errorElem = document.getElementById("error");
function displayError(error) {
    errorElem.innerText = error;
}

let linkCode;
function onCodeSubmit() {
    linkCode = document.getElementById("code-entry").value.replace(/-/ig, "").toLowerCase().trim();
    if(linkCode.length !== 9 || linkCode.includes("/")) {
        displayError("That doesn't seem like a valid link code.");
        return;
    }
    displayStep(1);
}

function displayPickerUri(pickerUri) {
    const pickLinkElem = document.getElementById("pick-link");
    pickLinkElem.href = pickerUri;
    new QRCode(document.getElementById("qrcode"), pickerUri);
}

let client;

async function oauthCallback(response) {
    displayStep(2);
    const authorizationCode = response.code;
    const refreshToken = await getRefreshTokenFromAuthorizationCode(authorizationCode);
    const accessToken = await getAccessTokenFromRefreshToken(refreshToken);
    const pickerUri = await startPickerSession(accessToken, refreshToken);
    displayStep(3);
    displayPickerUri(pickerUri);
}

function initializeOAuth() {
    client = google.accounts.oauth2.initCodeClient({
        client_id: OAUTH_CLIENT_ID,
        scope: "https://www.googleapis.com/auth/photospicker.mediaitems.readonly",
        ux_mode: "popup",
        callback: oauthCallback
    });
}

let firebaseApp;
let functions;
function initializeFirebase() {
    firebaseApp = firebase.initializeApp(FIREBASE_CONFIG);
    functions = firebaseApp.functions();
    if(window.location.hostname === "localhost") {
        console.log("%c ** IN LOCALHOST ENVIRONMENT. USING FIREBASE FUNCTIONS EMULATOR **", "color: red;");
        functions.useEmulator("localhost", 5001);
    }
}

function onSignInButtonClick() {
    client.requestCode();
}

let pollInterval;
async function startPickerSession(accessToken, refreshToken) {
    // https://developers.google.com/photos/picker/reference/rest/v1/sessions/create
    let json;
    try {
        const res = await fetch("https://photospicker.googleapis.com/v1/sessions", {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${accessToken}`,
                "Content-Type": "application/x-www-form-urlencoded",
            },
        });
        json = await res.json();
    } catch(e) {
        displayError("Error while starting picker session.");
        console.error(e);
        return;
    }
    const sessionId = json.id;
    const pickerUri = json.pickerUri;
    const pollingInterval = parseFloat(json.pollingConfig.pollInterval.replace("s", "")) * 1000;
    const timeoutIn = parseFloat(json.pollingConfig.timeoutIn.replace("s", "")) * 1000;
    pollInterval = setInterval(() => {
        pollPickerSession(accessToken, refreshToken, sessionId);
    }, pollingInterval);
    setTimeout(() => {
        if(pollInterval) {
            clearInterval(pollInterval);
            pollInterval = null;
            displayError("It took too long to pick your photos. Please refresh and try again.");
        }
    }, timeoutIn);

    return pickerUri;
}

async function pollPickerSession(accessToken, refreshToken, sessionId) {
    let json;
    try {
        const res = await fetch(`https://photospicker.googleapis.com/v1/sessions/${sessionId}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${accessToken}`,
                "Content-Type": "application/x-www-form-urlencoded",
            },
        });
        json = await res.json();
    } catch(e) {
        displayError("Error while polling picker session.");
        console.error(e);
        return;
    }
    const isDone = json.mediaItemsSet;
    
    if(isDone) {
        await onDonePicking(refreshToken, sessionId);
    }
}

async function onDonePicking(refreshToken, sessionId) {
    if(pollInterval) {
        clearInterval(pollInterval);
        pollInterval = null;
    }
    displayStep(4);
    await uploadDataToServer(refreshToken, sessionId);
    displayStep(5);
}

async function getRefreshTokenFromAuthorizationCode(authorizationCode) {
    let json;
    // It is OK to do this client side: https://stackoverflow.com/questions/56412713/can-oauth-be-used-securely-from-the-client-side-for-a-back-end-service
    try {
        const res = await fetch("https://oauth2.googleapis.com/token", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: new URLSearchParams({
                grant_type: "authorization_code",
                code: authorizationCode,
                redirect_uri: "postmessage",
                client_id: OAUTH_CLIENT_ID,
                client_secret: OAUTH_CLIENT_SECRET
            }),
        });
        json = await res.json();
    } catch(e) {
        displayError("Error while exchanging authorization code for refresh token.");
        console.error(e);
        return;
    }

    const refreshToken = json.refresh_token;
    return refreshToken;
}

async function uploadDataToServer(refreshToken, sessionId) {
    const fn = functions.httpsCallable("finishedPickingPhotos");
    let res;
    try {
        res = await fn({
            linkCode,
            refreshToken,
            sessionId,
        });
    } catch(e) {
        console.error(e);
        displayError(`Error uploading data to server. Error: ${e.code}, ${e.message}`);
        return;
    }
    if(!res.data?.success) {
        console.log(res.data);
        displayError("Something went wrong. Error: non-success response from server");
        return;
    }
}

async function getAccessTokenFromRefreshToken(refreshToken) {
    let json;
    try {
        const res = await fetch("https://oauth2.googleapis.com/token", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: new URLSearchParams({
                grant_type: "refresh_token",
                refresh_token: refreshToken,
                client_id: OAUTH_CLIENT_ID,
                client_secret: OAUTH_CLIENT_SECRET
            })
        });
        json = await res.json();
    } catch(e) {
        displayError("Error while fetching access token from refresh token.");
        console.error(e);
        return;
    }
    const accessToken = json.access_token;
    return accessToken;
}

window.onload = () => {
    displayStep(0);
    initializeOAuth();
    initializeFirebase();
}
