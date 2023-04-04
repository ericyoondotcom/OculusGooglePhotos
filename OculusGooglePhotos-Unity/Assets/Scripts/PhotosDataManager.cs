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

    public int numAlbumsPerPage = 10;
    public int numMediaItemsPerPage = 10;

    readonly HttpClient client = new HttpClient();

    public async Task<bool> FetchNextPageOfAlbumData()
    {
        if (!data.hasMoreAlbumPagesToLoad) return false;
        string accessToken = await AuthenticationManager.Instance.GetAccessToken();
        if (accessToken == null || accessToken.Length == 0) return false;

        UriBuilder uriBuilder = new UriBuilder("https://photoslibrary.googleapis.com/v1/albums");
        uriBuilder.Port = -1;
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["pageSize"] = numAlbumsPerPage.ToString();
        if (data.nextAlbumPageToken.Length > 0) query["pageToken"] = data.nextAlbumPageToken;
        uriBuilder.Query = query.ToString();

        using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString()))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage res = await client.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                string content = res.Content == null ? "" : await res.Content.ReadAsStringAsync();
                Debug.LogError("Response " + res.StatusCode + " from Google Photos album LIST: " + content);
                return false;
            }

            if (res.Content == null)
            {
                Debug.LogError("Null content from Google Photos album LIST.");
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
                data.albums[id] = new Album(id, title, mediaItemsCount, coverPhotoBaseUrl);
            }
            return true;
        }
    }

    public async Task<bool> FetchNextPageOfLibraryMediaItems()
    {
        if (!data.library.hasMoreMediaItemsToLoad) return false;
        string accessToken = await AuthenticationManager.Instance.GetAccessToken();
        if (accessToken == null || accessToken.Length == 0) return false;

        UriBuilder uriBuilder = new UriBuilder("https://photoslibrary.googleapis.com/v1/mediaItems");
        uriBuilder.Port = -1;
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["pageSize"] = numMediaItemsPerPage.ToString();
        if (data.library.nextMediaItemsPageToken.Length > 0) query["pageToken"] = data.library.nextMediaItemsPageToken;
        uriBuilder.Query = query.ToString();

        using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString()))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage res = await client.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                string content = res.Content == null ? "" : await res.Content.ReadAsStringAsync();
                Debug.LogError("Response " + res.StatusCode + " from Google Photos media item LIST: " + content);
                return false;
            }

            if (res.Content == null)
            {
                Debug.LogError("Null content from Google Photos media item LIST.");
                return false;
            }

            string returnContent = await res.Content.ReadAsStringAsync();
            SimpleJSON.JSONNode ret = SimpleJSON.JSON.Parse(returnContent);
            if (ret.HasKey("nextPageToken"))
            {
                data.library.nextMediaItemsPageToken = ret["nextPageToken"];
                data.library.hasMoreMediaItemsToLoad = true;
            }
            else
            {
                data.library.nextMediaItemsPageToken = "";
                data.library.hasMoreMediaItemsToLoad = false;
            }

            SimpleJSON.JSONArray mediaItems = ret["mediaItems"].AsArray;
            for (int i = 0; i < mediaItems.Count; i++)
            {
                SimpleJSON.JSONObject itemData = mediaItems[i].AsObject;
                string id = itemData["id"];
                string description = itemData["title"];
                string mimeType = itemData["mimeType"];
                string baseUrl = itemData["baseUrl"];
                SimpleJSON.JSONObject mediaMetadata = itemData["mediaMetadata"].AsObject;
                DateTime timestamp = DateTime.Parse(mediaMetadata["creationTime"]);
                int width = int.Parse(mediaMetadata["width"]);
                int height = int.Parse(mediaMetadata["height"]);
                data.library.mediaItems[id] = new MediaItem(id, description, mimeType, baseUrl, timestamp, width, height);
            }
            return true;
        }
    }

    public async Task<bool> FetchNextPageOfMediaItemsInAlbum()
    {
        // TODO
        // https://developers.google.com/photos/library/reference/rest/v1/mediaItems/search
        return false;
    }
}
