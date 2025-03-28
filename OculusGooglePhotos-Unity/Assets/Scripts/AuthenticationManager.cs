using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SimpleJSON;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    public bool useFunctionsEmulators;

    [NonSerialized]
    public string refreshToken;
    [NonSerialized]
    public string accessToken;
    [NonSerialized]
    public string sessionId;
    [NonSerialized]
    public DateTime accessTokenExpiry;
    readonly HttpClient client = new HttpClient();
    string loadSceneOnNextUpdate;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (useFunctionsEmulators)
        {
            Debug.Log("** USING FIREBASE FUNCTIONS EMULATORS **");
        }

        LoadSavedRefreshToken();
    }

    void OnAuthFail()
    {
        loadSceneOnNextUpdate = "Login";
    }

    void LoadSavedRefreshToken()
    {
        refreshToken = PlayerPrefs.GetString("google_refresh_token");
        sessionId = PlayerPrefs.GetString("photos_session_id");
        if (refreshToken.Length == 0 || sessionId.Length == 0)
        {
            OnAuthFail();
            return;
        }
        Debug.Log("Loaded auth data from disk");
    }

    string GetFirebaseFunctionsBaseURL() => useFunctionsEmulators ? Constants.FIREBASE_FUNCTIONS_BASE_URL_EMULATOR : Constants.FIREBASE_FUNCTIONS_BASE_URL;

    public async Task<bool> FetchPickerSession(string linkCode)
    {
        using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, GetFirebaseFunctionsBaseURL() + "pollForPickerSession"))
        {
            req.Content = new StringContent(linkCode);
            HttpResponseMessage res = null;
            try
            {
                res = await client.SendAsync(req);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This link code has no result yet
                return false;
            }

            if (!res.IsSuccessStatusCode)
            {
                string content = res.Content == null ? "" : await res.Content.ReadAsStringAsync();
                Debug.LogError("Fetch token function returned status " + res.StatusCode + ": " + content);
                return false;
            }

            if (res.Content == null)
            {
                Debug.LogError("Fetch token function returned no content!");
                return false;
            }

            string respStr = await res.Content.ReadAsStringAsync();

            JSONNode json = JSON.Parse(respStr);
            refreshToken = json["refreshToken"];
            sessionId = json["sessionId"];
            Debug.Log("Found data on server. Refresh token: " + refreshToken + ", session ID: " + sessionId);

            PlayerPrefs.SetString("google_refresh_token", refreshToken);
            PlayerPrefs.SetString("photos_session_id", sessionId);
            PlayerPrefs.Save();

            return true;
        }
    }

    public async Task<bool> RefreshToken()
    {
        FormUrlEncodedContent body = new FormUrlEncodedContent(new Dictionary<string, string>() {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", Constants.OAUTH_CLIENT_ID },
            { "client_secret", Constants.OAUTH_CLIENT_SECRET }
        });
        using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token"))
        {
            req.Content = body;
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            HttpResponseMessage res = await client.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                string content = res.Content == null ? "" : await res.Content.ReadAsStringAsync();
                Debug.LogError("Refresh token returned status " + res.StatusCode + ": " + content);
                OnAuthFail();
                return false;
            }

            if (res.Content == null)
            {
                Debug.LogError("Refresh token returned null content.");
                return false;
            }

            string returnContent = await res.Content.ReadAsStringAsync();
            SimpleJSON.JSONNode ret = SimpleJSON.JSON.Parse(returnContent);
            accessToken = ret["access_token"];
            accessTokenExpiry = DateTime.Now.AddSeconds(ret["expires_in"].AsInt);
            Debug.Log("Refreshed access token. Expires on " + accessTokenExpiry.ToLongDateString() + " @ " + accessTokenExpiry.ToLongTimeString() + ", access token: " + accessToken);
            return true;
        }
    }

    public async Task<string> GetAccessToken()
    {
        if (refreshToken == null || refreshToken.Length == 0)
        {
            return null;
        }
        if (accessToken == null || accessToken.Length == 0 || accessTokenExpiry < DateTime.Now)
        {
            if (!await RefreshToken())
            {
                return null;
            }
        }
        return accessToken;
    }

    public void SignOut()
    {
        PlayerPrefs.DeleteAll();
        OnAuthFail();
    }

    private void Update()
    {
        if (loadSceneOnNextUpdate != null)
        {
            SceneManager.LoadSceneAsync(loadSceneOnNextUpdate);
            loadSceneOnNextUpdate = null;
        }
    }
}
