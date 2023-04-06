using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Networking;
using XmpCore;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using System.Linq;

public class PhotosDataManager : MonoBehaviour
{
    public PhotosDataStore data { get; private set; } = new PhotosDataStore();

    public int numAlbumsPerPage = 10;
    public int numMediaItemsPerPage = 10;

    readonly HttpClient client = new HttpClient();
    readonly HttpClient clientWithoutRedirects = new HttpClient(new HttpClientHandler()
    {
        AllowAutoRedirect = false
    });

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
                string filename = itemData["filename"];
                string mimeType = itemData["mimeType"];
                string baseUrl = itemData["baseUrl"];
                SimpleJSON.JSONObject mediaMetadata = itemData["mediaMetadata"].AsObject;
                DateTime timestamp = DateTime.Parse(mediaMetadata["creationTime"]);
                int width = int.Parse(mediaMetadata["width"]);
                int height = int.Parse(mediaMetadata["height"]);
                data.library.mediaItems[id] = new MediaItem(id, description, filename, mimeType, baseUrl, timestamp, width, height);
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
                string filename = itemData["filename"];
                string mimeType = itemData["mimeType"];
                string baseUrl = itemData["baseUrl"];
                SimpleJSON.JSONObject mediaMetadata = itemData["mediaMetadata"].AsObject;
                DateTime timestamp = DateTime.Parse(mediaMetadata["creationTime"]);
                int width = int.Parse(mediaMetadata["width"]);
                int height = int.Parse(mediaMetadata["height"]);
                album.mediaItems[id] = new MediaItem(id, description, filename, mimeType, baseUrl, timestamp, width, height);
            }
            return true;
        }
    }

    public IEnumerator DownloadPhotoContent(MediaItem mediaItem, Action<MediaItem> callback, Action<float> onProgressChange)
    {
        if (!mediaItem.IsPhoto)
        {
            Debug.LogError("DownloadPhotoContent called on media that does not have a photo MIME type");
            yield break;
        }

        string url = mediaItem.baseUrl + "=d";

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            var operation = req.SendWebRequest();

            while (!operation.isDone)
            {
                onProgressChange(operation.progress);
                yield return new WaitForEndOfFrame();
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download image returned error: " + req.result);
                yield break;
            }
            var texture = DownloadHandlerTexture.GetContent(req);
            byte[] bytes = req.downloadHandler.data;
            mediaItem.OnPhotoDownloaded(texture, bytes);
        }
        RetrieveXMPData(mediaItem);
        callback(mediaItem);
    }

    public IEnumerator DownloadVideoContent(MediaItem mediaItem, Action<MediaItem> callback, Action<float> onProgressChange)
    {
        if (!mediaItem.IsVideo)
        {
            Debug.LogError("DownloadVideoContent called on media that does not have a video MIME type");
            yield break;
        }

        string url = mediaItem.baseUrl + "=dv";
        string uuid = Utility.GenerateUUID();

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            var operation = req.SendWebRequest();

            while (!operation.isDone)
            {
                onProgressChange(operation.progress);
                yield return new WaitForEndOfFrame();
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download video returned error: " + req.result);
                yield break;
            }
            string savePath = string.Format(
                "{0}/{1}.{2}",
                Application.temporaryCachePath,
                uuid,
                mediaItem.OriginalFilenameExtension
            );
            var writeTask = System.IO.File.WriteAllBytesAsync(savePath, req.downloadHandler.data);
            yield return new WaitUntil(() => writeTask.IsCompleted);
            mediaItem.OnVideoDownloaded(savePath);
        }
        RetrieveXMPData(mediaItem);
        callback(mediaItem);
    }

    public void RetrieveXMPData(MediaItem mediaItem)
    {
        Stream stream = null;
        if (mediaItem.IsPhoto && mediaItem.imageBytes != null)
        {
            stream = new MemoryStream(mediaItem.imageBytes);
        }
        else if(mediaItem.IsVideo && mediaItem.downloadedVideoFilePath != null)
        {
            stream = File.OpenRead(mediaItem.downloadedVideoFilePath);
        }
        if (stream == null)
        {
            Debug.LogError("Stream was null, could not read XMP");
            return;
        }

        var dirs = ImageMetadataReader.ReadMetadata(stream);
        var xmpDir = dirs.OfType<XmpDirectory>().FirstOrDefault();

        if (xmpDir?.XmpMeta == null)
        {
            Debug.LogWarning("XMP metadata is null, skipping");
            return;
        }
        var properties = xmpDir.XmpMeta.Properties;

        string projection = null;

        foreach (var property in properties)
        {
            Debug.Log($"Path={property.Path} Namespace={property.Namespace} Value={property.Value}");
            if (property.Path == null || property.Value == null) continue;
            if (property.Path.Contains("ProjectionType"))
            {
                projection = property.Value;
            }
        }

        mediaItem.SetMetadata(projection);
    }
}
