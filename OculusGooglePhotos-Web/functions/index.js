const config = require("./config");
const functions = require("firebase-functions");
const admin = require("firebase-admin");
const fetch = require("node-fetch-commonjs");

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

// req data: text/plain (link code string)
// res data: text/plain (refresh token string)
exports.pollForRefreshToken = functions.https.onRequest(async (req, res) => {
    const linkCode = req.body;
    if((typeof linkCode) !== "string" || linkCode.length !== 9 || linkCode.includes("/")) {
        return res.status(400).end("Malformed link code.");
    }

    let snap;
    try {
        snap = await db.ref("authResults/" + linkCode).get();
    } catch(e) {
        return res.status(500).end("Error while fetching auth result from database.");
    }
    const val = snap.val();
    if(!val) {
        return res.status(404).end("No auth result found");
    }
    return res.status(200).end(snap.val().refreshToken);
});
