using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;

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

    public async Task<bool> FetchNextPageOfMediaItemsInAlbum(string albumKey)
    {
        Album album = data.albums[albumKey];
        if (!album.hasMoreMediaItemsToLoad) return false;
        string accessToken = await AuthenticationManager.Instance.GetAccessToken();
        if (accessToken == null || accessToken.Length == 0) return false;

        UriBuilder uriBuilder = new UriBuilder("https://photoslibrary.googleapis.com/v1/mediaItems:search");
        uriBuilder.Port = -1;
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["pageSize"] = numMediaItemsPerPage.ToString();
        if (album.nextMediaItemsPageToken.Length > 0) query["pageToken"] = album.nextMediaItemsPageToken;
        query["albumId"] = albumKey;
        uriBuilder.Query = query.ToString();

        using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage res = await client.SendAsync(req);

            if (!res.IsSuccessStatusCode)
            {
                string content = res.Content == null ? "" : await res.Content.ReadAsStringAsync();
                Debug.LogError("Response " + res.StatusCode + " from Google Photos media item SEARCH: " + content);
                return false;
            }

            if (res.Content == null)
            {
                Debug.LogError("Null content from Google Photos media item SEARCH.");
                return false;
            }

            string returnContent = await res.Content.ReadAsStringAsync();
            SimpleJSON.JSONNode ret = SimpleJSON.JSON.Parse(returnContent);
            if (ret.HasKey("nextPageToken"))
            {
                album.nextMediaItemsPageToken = ret["nextPageToken"];
                album.hasMoreMediaItemsToLoad = true;
            }
            else
            {
                album.nextMediaItemsPageToken = "";
                album.hasMoreMediaItemsToLoad = false;
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
                album.mediaItems[id] = new MediaItem(id, description, mimeType, baseUrl, timestamp, width, height);
            }
            return true;
        }
    }

    public IEnumerator DownloadMediaContent(MediaItem mediaItem, Action<MediaItem> callback)
    {
        if (mediaItem.IsPhoto)
        {
            string url = mediaItem.baseUrl + "=d";

            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Download media returned error: " + req.result);
                    yield break;
                }
                var texture = DownloadHandlerTexture.GetContent(req);
                mediaItem.SetDownloadedProperties(texture);
            }

            // TODO
            // Check the XMP data for a projection field, and set a defaultProjection
            // field in the MediaItem
            callback(mediaItem);
        }
        else if (mediaItem.IsVideo)
        {
            Debug.LogError("Do not call DownloadMediaContent on videos. Use Unity's video player.");
            yield break;
        }
        else
        {
            Debug.LogError("Media is neither photo nor video.");
            yield break;
        };
    }
}
