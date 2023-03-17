using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;

public class PhotosDataManager : MonoBehaviour
{
    public PhotosDataStore data { get; private set; } = new PhotosDataStore();

    readonly HttpClient client = new HttpClient();

    public async Task<bool> FetchNextPageOfAlbumData()
    {
        if (!data.hasMoreAlbumPagesToLoad) return false;
        string accessToken = await AuthenticationManager.Instance.GetAccessToken();
        if (accessToken == null || accessToken.Length == 0) return false;

        UriBuilder uriBuilder = new UriBuilder("https://photoslibrary.googleapis.com/v1/albums");
        uriBuilder.Port = -1;
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["pageSize"] = "50";
        if (data.nextAlbumPageToken.Length > 0) query["pageToken"] = data.nextAlbumPageToken;
        uriBuilder.Query = query.ToString();

        using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString()))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage res = await client.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                string content = res.Content == null ? "" : await res.Content.ReadAsStringAsync();
                Debug.Log("Response " + res.StatusCode + " from Google Photos album LIST: " + content);
                return false;
            }

            if (res.Content == null)
            {
                Debug.Log("Null content from Google Photos album LIST.");
                return false;
            }

            string returnContent = await res.Content.ReadAsStringAsync();
            SimpleJSON.JSONNode ret = SimpleJSON.JSON.Parse(returnContent);
            if (ret.HasKey("nextPageToken"))
            {
                data.nextAlbumPageToken = ret["nextPageToken"];
                data.hasMoreAlbumPagesToLoad = true;
            }
            else
            {
                data.nextAlbumPageToken = "";
                data.hasMoreAlbumPagesToLoad = false;
            }

            SimpleJSON.JSONArray albums = ret["albums"].AsArray;
            for (int i = 0; i < albums.Count; i++)
            {
                SimpleJSON.JSONObject albumData = albums[i].AsObject;
                string id = albumData["id"];
                string title = albumData["title"];
                int mediaItemsCount = int.Parse(albumData["mediaItemsCount"]);
                string coverPhotoBaseUrl = albumData["coverPhotoBaseUrl"];
                Debug.Log("id: " + id + ", title: " + title);
                data.albums[id] = new Album(id, title, mediaItemsCount, coverPhotoBaseUrl);
            }
            return true;
        }
    }
}
