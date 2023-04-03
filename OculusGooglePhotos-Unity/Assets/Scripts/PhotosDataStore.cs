using System;
using System.Collections.Generic;

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

}
