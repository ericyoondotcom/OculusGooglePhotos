const config = require("./config");
const functions = require("firebase-functions");
const admin = require("firebase-admin");

admin.initializeApp();
const db = admin.database();

exports.onAuthorizationCodeObtained = functions.https.onCall(async (data, context) => {
    const {linkCode, authorizationCode} = data;
    if((typeof linkCode) !== "string" || linkCode.length !== 9 || linkCode.includes("/")) {
        throw new functions.https.HttpsError("invalid-argument", "Malformed link code.");
    }
    if((typeof authorizationCode) !== "string" || authorizationCode.length === 0) {
        throw new functions.https.HttpsError("invalid-argument", "Malformed authorization code.");
    }

    let json;
    try {
        const res = await fetch("https://oauth2.googleapis.com/token", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: new URLSearchParams({
                grant_type: "authorization_code",
                code: authorizationCode,
                redirect_uri: "postmessage", // for popup url. https://stackoverflow.com/a/72365385/4699945
                client_id: config.OAUTH_CLIENT_ID,
                client_secret: config.OAUTH_CLIENT_SECRET
            })
        });
        json = await res.json();
    } catch(e) {
        console.error(e);
        throw new functions.https.HttpsError("internal", "Error while fetching access token.");
    }
    
    await db.ref("authResults/" + linkCode).set({
        refreshToken: json.refresh_token
    });
    return {success: true};
});

exports.pollForRefreshToken = functions.https.onRequest((req, res) => {

});
