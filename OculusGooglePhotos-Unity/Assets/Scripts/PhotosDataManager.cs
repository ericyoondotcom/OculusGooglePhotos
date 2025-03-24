using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using System.Linq;

public class PhotosDataManager : MonoBehaviour
{
    public PhotosDataStore data { get; private set; } = new PhotosDataStore();

    public int numMediaItemsPerPage = 10;

    readonly HttpClient client = new HttpClient();
    readonly HttpClient clientWithoutRedirects = new HttpClient(new HttpClientHandler()
    {
        AllowAutoRedirect = false
    });

    public async Task<bool> FetchNextPageOfLibraryMediaItems()
    {
        if (!data.library.hasMoreMediaItemsToLoad) return false;
        string accessToken = await AuthenticationManager.Instance.GetAccessToken();
        string sessionId = AuthenticationManager.Instance.sessionId;
        if (accessToken == null || accessToken.Length == 0) return false;
        if (sessionId == null || sessionId.Length == 0) return false;

        UriBuilder uriBuilder = new UriBuilder("https://photospicker.googleapis.com/v1/mediaItems");
        uriBuilder.Port = -1;
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["pageSize"] = numMediaItemsPerPage.ToString();
        query["sessionId"] = sessionId;
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

            // https://developers.google.com/photos/picker/reference/rest/v1/mediaItems
            SimpleJSON.JSONArray pickedMediaItems = ret["mediaItems"].AsArray;
            for (int i = 0; i < pickedMediaItems.Count; i++)
            {
                SimpleJSON.JSONObject pickedMediaItem = pickedMediaItems[i].AsObject;
                string id = pickedMediaItem["id"];
                SimpleJSON.JSONObject mediaFile = pickedMediaItem["mediaFile"].AsObject;
                string filename = mediaFile["filename"];
                string mimeType = mediaFile["mimeType"];
                string baseUrl = mediaFile["baseUrl"];
                SimpleJSON.JSONObject mediaMetadata = mediaFile["mediaFileMetadata"].AsObject;
                int width = int.Parse(mediaMetadata["width"]);
                int height = int.Parse(mediaMetadata["height"]);
                data.library.mediaItems[id] = new MediaItem(id, filename, mimeType, baseUrl, width, height);
            }
            return true;
        }
    }

    public IEnumerator DownloadThumbnail(MediaItem mediaItem, Action<MediaItem> callback)
    {
        Task<string> task = AuthenticationManager.Instance.GetAccessToken();
        yield return new WaitUntil(() => task.IsCompleted);
        string accessToken = task.Result;

        string url = mediaItem.baseUrl + "=w500-h500-c-d";

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            req.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download image thumbnail returned error: " + req.result);
                yield break;
            }
            var texture = DownloadHandlerTexture.GetContent(req);
            byte[] bytes = req.downloadHandler.data;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            mediaItem.OnPhotoThumbnailDownloaded(sprite, bytes);
            RetrieveXMPData(mediaItem); // Turns out, if you pass the -d flag along with w, h, and c, it still downloads metadata!
            callback(mediaItem);
        }
    }
    public IEnumerator DownloadPhotoContent(MediaItem mediaItem, Action<MediaItem> callback, Action<float> onProgressChange)
    {
        if (!mediaItem.IsPhoto)
        {
            Debug.LogError("DownloadPhotoContent called on media that does not have a photo MIME type");
            yield break;
        }

        Task<string> task = AuthenticationManager.Instance.GetAccessToken();
        yield return new WaitUntil(() => task.IsCompleted);
        string accessToken = task.Result;

        string url = mediaItem.baseUrl + "=d";

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            req.SetRequestHeader("Authorization", "Bearer " + accessToken);
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

        Task<string> task = AuthenticationManager.Instance.GetAccessToken();
        yield return new WaitUntil(() => task.IsCompleted);
        string accessToken = task.Result;

        string url = mediaItem.baseUrl + "=dv";
        string uuid = Utility.GenerateUUID();

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("Authorization", "Bearer " + accessToken);
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
            // Debug.LogError("Stream was null, could not read XMP");
            return;
        }

        var dirs = ImageMetadataReader.ReadMetadata(stream);
        var xmpDir = dirs.OfType<XmpDirectory>().FirstOrDefault();

        if (xmpDir?.XmpMeta == null)
        {
            // Debug.LogWarning("XMP metadata is null, skipping");
            return;
        }
        var properties = xmpDir.XmpMeta.Properties;

        string projection = null;

        foreach (var property in properties)
        {
            if (property.Path == null || property.Value == null) continue;
            if (property.Path.Contains("ProjectionType"))
            {
                projection = property.Value;
            }
        }

        mediaItem.SetMetadata(projection);
    }
}
