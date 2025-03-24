const config = require("./config");
const functions = require("firebase-functions");
const admin = require("firebase-admin");
const fetch = require("node-fetch-commonjs");
const { refreshToken } = require("firebase-admin/app");

admin.initializeApp();
const db = admin.database();

exports.finishedPickingPhotos = functions.https.onCall(async (data, context) => {
    const {linkCode, refreshToken, sessionId} = data;
    if((typeof linkCode) !== "string" || linkCode.length !== 9 || linkCode.includes("/")) {
        throw new functions.https.HttpsError("invalid-argument", "Malformed link code.");
    }
    if((typeof refreshToken) !== "string" || refreshToken.length === 0) {
        throw new functions.https.HttpsError("invalid-argument", "Malformed refresh token.");
    }
    if((typeof sessionId) !== "string" || sessionId.length === 0) {
        throw new functions.https.HttpsError("invalid-argument", "Malformed session ID.");
    }
    
    await db.ref("pickerSessions/" + linkCode).set({
        refreshToken,
        sessionId,
        createdAt: admin.database.ServerValue.TIMESTAMP
    });
    return { success: true };
});

// req data: plaintext string (link code string)
// res data: json (refresh token, session id)
exports.pollForPickerSession = functions.https.onRequest(async (req, res) => {
    const linkCode = req.body;
    if((typeof linkCode) !== "string" || linkCode.length !== 9 || linkCode.includes("/")) {
        return res.status(400).end("Malformed link code.");
    }

    let snap;
    try {
        snap = await db.ref("pickerSessions/" + linkCode).get();
    } catch(e) {
        return res.status(500).end("Error while fetching auth result from database.");
    }
    const val = snap.val();
    if(!val) {
        return res.status(404).end("No auth result found");
    }
    if(!val.refreshToken || !val.sessionId || !val.createdAt) {
        return res.status(500).end("Malformed data saved in database");
    }
    // Delete codes older than 5 minutes
    if(val.createdAt < Date.now() - 5 * 60 * 1000) {
        await snap.ref.remove();
        return res.status(410).end("Auth result expired.");
    }

    await snap.ref.remove(); // Delete the code once it's used
    return res.status(200).json(snap.val());
});
