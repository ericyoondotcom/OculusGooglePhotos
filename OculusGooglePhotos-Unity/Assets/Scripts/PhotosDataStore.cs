using System;
using System.Collections.Generic;

public class PhotosDataStore
{
    public Dictionary<string, Album> albums = new Dictionary<string, Album>();
    public Dictionary<string, MediaItem> libraryMediaItems = new Dictionary<string, MediaItem>();
    public string nextAlbumPageToken = "";
    public bool hasMoreAlbumPagesToLoad = true;
}

public class Album {
    public string id;
    public string title;
    public int mediaItemsCount;
    public string coverPhotoBaseUrl;
    public Dictionary<string, MediaItem> mediaItems;

    public Album(string id, string title, int mediaItemsCount, string coverPhotoBaseUrl)
    {
        this.id = id;
        this.title = title;
        this.mediaItemsCount = mediaItemsCount;
        this.coverPhotoBaseUrl = coverPhotoBaseUrl;
        this.mediaItems = new Dictionary<string, MediaItem>();
    }
}

public class MediaItem {

}
