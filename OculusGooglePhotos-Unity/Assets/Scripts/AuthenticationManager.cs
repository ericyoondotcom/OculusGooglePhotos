using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using OVRSimpleJSON;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEditor.PackageManager;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    public bool useFunctionsEmulators;

    [NonSerialized]
    public string refreshToken;
    [NonSerialized]
    public string accessToken;
    [NonSerialized]
    public DateTime accessTokenExpiry;
    readonly HttpClient client = new HttpClient();

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
        SceneManager.LoadSceneAsync("Login");
    }

    void LoadSavedRefreshToken()
    {
        refreshToken = PlayerPrefs.GetString("google_refresh_token");
        if (refreshToken.Length == 0)
        {
            OnAuthFail();
            return;
        }
        Debug.Log("Loaded refresh token from disk: " + refreshToken);
    }

    string GetFirebaseFunctionsBaseURL() => useFunctionsEmulators ? Constants.FIREBASE_FUNCTIONS_BASE_URL_EMULATOR : Constants.FIREBASE_FUNCTIONS_BASE_URL;

    public async Task<bool> FetchRefreshToken(string linkCode)
    {
        using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, GetFirebaseFunctionsBaseURL() + "pollForRefreshToken"))
        {
            req.Content = new StringContent(linkCode);
            HttpResponseMessage resp = await client.SendAsync(req);

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This link code has no result yet
                //Debug.Log("No refresh token yet...");
                return false;
            }

            if (!resp.IsSuccessStatusCode)
            {
                string content = resp.Content == null ? "" : await resp.Content.ReadAsStringAsync();
                Debug.Log("Fetch token function returned status " + resp.StatusCode + ": " + content);
                return false;
            }

            if (resp.Content == null)
            {
                Debug.Log("Fetch token function returned no content!");
                return false;
            }

            string respStr = await resp.Content.ReadAsStringAsync();
            refreshToken = respStr;
            Debug.Log("Found refresh token from server: " + refreshToken);

            PlayerPrefs.SetString("google_refresh_token", refreshToken);
            PlayerPrefs.Save();

            return true;
        }
    }

    public async Task<bool> RefreshToken()
    {
        FormUrlEncodedContent body = new FormUrlEncodedContent(new Dictionary<string, string>() {
            {"grant_type", "refresh_token" },
            {"refresh_token", Uri.EscapeDataString(authInfo.refreshToken) }
        });
    }
}
