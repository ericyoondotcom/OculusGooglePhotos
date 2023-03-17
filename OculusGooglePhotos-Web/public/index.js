const STEP_ELEMENTS = [
    document.getElementById("step-0"),
    document.getElementById("step-1"),
    document.getElementById("step-2"),
    document.getElementById("step-3"),
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

let client;

function oauthCallback(response) {
    displayStep(2);
    sendCodeToServer(response.code);
}

function initializeOAuth() {
    client = google.accounts.oauth2.initCodeClient({
        client_id: OAUTH_CLIENT_ID,
        scope: "https://www.googleapis.com/auth/photoslibrary.readonly",
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

async function sendCodeToServer(authorizationCode) {
    const onAuthorizationCodeObtained = functions.httpsCallable("onAuthorizationCodeObtained");
    let res;
    try {
        res = await onAuthorizationCodeObtained({
            linkCode,
            authorizationCode
        });
    } catch(e) {
        console.error(e);
        displayError(`Something went wrong. Error: ${e.code}, ${e.message}`);
        return;
    }
    if(!res.data?.success) {
        console.log(res.data);
        displayError("Something went wrong. Error: non-success response from server");
        return;
    }
    displayStep(3);
}

window.onload = () => {
    displayStep(0);
    initializeOAuth();
    initializeFirebase();
}
