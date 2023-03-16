const STEP_ELEMENTS = [
    document.getElementById("step-0"),
    document.getElementById("step-1"),
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

function oauthCallback() {

}

function renderGoogleSignInButton() {
    google.accounts.id.initialize({
        client_id: OAUTH_CLIENT_ID,
        callback: oauthCallback
    });
    google.accounts.id.renderButton(
        document.getElementById("google-button"),
        {
            theme: "filled_blue",
            size: "large",
            shape: "pill",
            text: "continue_with"
        }
    );
}

window.onload = () => {
    displayStep(0);
    renderGoogleSignInButton();
}
