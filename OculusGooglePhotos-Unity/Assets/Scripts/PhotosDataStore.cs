using System;
using System.Collections.Generic;
using UnityEngine;

public class PhotosDataStore
{
    public Dictionary<string, Album> albums = new Dictionary<string, Album>();
    public Library library = new Library();
    public string nextAlbumPageToken = "";
    public bool hasMoreAlbumPagesToLoad = true;
}

public class Library
{
    public Dictionary<string, MediaItem> mediaItems = new Dictionary<string, MediaItem>();
    public bool hasMoreMediaItemsToLoad = true;
    public string nextMediaItemsPageToken = "";
}

public class Album {
    public string id;
    public string title;
    public string coverPhotoBaseUrl;
    public int mediaItemsCount;
    public Dictionary<string, MediaItem> mediaItems;
    public bool hasMoreMediaItemsToLoad = true;
    public string nextMediaItemsPageToken = "";

    public Album(string id, string title, int mediaItemsCount, string coverPhotoBaseUrl)
    {
        this.id = id;
        this.title = title;
        this.coverPhotoBaseUrl = coverPhotoBaseUrl;
        this.mediaItemsCount = mediaItemsCount;
        this.mediaItems = new Dictionary<string, MediaItem>();
    }
}

public class MediaItem {
    public string id;
    public string description;
    public string originalFilename;
    public string mimeType;
    public string baseUrl;
    public DateTime timestamp;
    public int width;
    public int height;

    public string projection;

    public Texture2D downloadedImageTexture;
    public Sprite downloadedThumbnailSprite;
    public byte[] imageBytes;
    public string downloadedVideoFilePath;

    public MediaItem(string id, string description, string originalFilename, string mimeType, string baseUrl, DateTime timestamp, int width, int height)
    {
        this.id = id;
        this.description = description;
        this.originalFilename = originalFilename;
        this.mimeType = mimeType;
        this.baseUrl = baseUrl;
        this.timestamp = timestamp;
        this.width = width;
        this.height = height;
    }

    public void OnPhotoDownloaded(Texture2D downloadedImageTexture, byte[] imageBytes)
    {
        this.downloadedImageTexture = downloadedImageTexture;
        this.imageBytes = imageBytes;
    }

    public void OnPhotoThumbnailDownloaded(Sprite downloadedThumbnailSprite, byte[] imageBytes)
    {
        this.downloadedThumbnailSprite = downloadedThumbnailSprite;
        this.imageBytes = imageBytes;
    }

    public void OnVideoDownloaded(string downloadedVideoFilePath)
    {
        this.downloadedVideoFilePath = downloadedVideoFilePath;
    }

    public void SetMetadata(string projection)
    {
        this.projection = projection;
    }

    public bool IsPhoto
    {
        get
        {
            return mimeType.Split('/')[0] == "image";
        }
    }
    public bool IsVideo
    {
        get
        {
            return mimeType.Split('/')[0] == "video";
        }
    }

    public string OriginalFilenameExtension
    {
        get
        {
            var split = originalFilename.Split(".");
            if (split.Length <= 1) return "";
            return split[split.Length - 1];
        }
    }
}
