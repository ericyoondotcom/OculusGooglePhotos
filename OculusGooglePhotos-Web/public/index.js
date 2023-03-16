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

let code;
function onCodeSubmit() {
    code = document.getElementById("code-entry").value.replace("-", "").toLowerCase().trim();
    if(code.length !== 9) {
        displayError("That doesn't seem like a valid link code.");
        return;
    }
    displayStep(1);
}

let client;

function oauthCallback(response) {
    displayStep(2);
    console.log(response)
}

function initializeOAuth() {
    client = google.accounts.oauth2.initCodeClient({
        client_id: OAUTH_CLIENT_ID,
        scope: "https://www.googleapis.com/auth/photoslibrary.readonly",
        ux_mode: "popup",
        callback: oauthCallback
    });
}

function onSignInButtonClick() {
    client.requestCode();
}

async function exchangeCodeForToken(code) {
    const result = await fetch(serviceData.tokenUrl, {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: new URLSearchParams({
            grant_type: "authorization_code",
            code,
            redirect_uri: WEB_REDIRECT_URL,
            client_id: serviceConstantsData.clientId,
            client_secret: serviceConstantsData.clientSecret
        })
    });
    const json = await result.json();
}

window.onload = () => {
    displayStep(0);
    initializeOAuth();
}
