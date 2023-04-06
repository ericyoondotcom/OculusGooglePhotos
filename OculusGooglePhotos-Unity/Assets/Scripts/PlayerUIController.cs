using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    public const string ALL_PHOTOS_TEXT = "All photos";
    
    public PhotosDataManager photosDataManager;
    public GameObject loader;
    public ProgressBar loaderProgressBar;
    public AlbumUI albumUI;
    public PhotosUI photosUI;

    bool displayAlbumsOnNextFrame;
    bool displayLibraryOnNextFrame;
    string albumToDisplayOnNextFrame;
    
    void Start()
    {
        albumUI.playerUIController = this;
        photosUI.playerUIController = this;
        albumUI.gameObject.SetActive(false);
        photosUI.gameObject.SetActive(false);
        LoadAlbums();
    }

    public void DisplayLoader(float progress = 0)
    {
        loader.SetActive(true);
        if (loaderProgressBar != null)
        {
            loaderProgressBar.gameObject.SetActive(progress > 0);
            loaderProgressBar.SetProgress(progress);
        }
    }
    public void HideLoader()
    {
        loader.SetActive(false);
    }

    public void DisplayAlbumUI()
    {
        loader.SetActive(false);
        albumUI.gameObject.SetActive(true);
        photosUI.gameObject.SetActive(false);
        albumUI.DisplayAlbums(photosDataManager.data);
        photosUI.photoDisplayer.StopDisplaying();
        photosUI.ClearSelection();
    }

    public void DisplayPhotosFromLibrary()
    {
        loader.SetActive(false);
        albumUI.gameObject.SetActive(false);
        photosUI.gameObject.SetActive(true);
        photosUI.DisplayLibrary(photosDataManager.data);
    }

    public void DisplayPhotosFromAlbum(string albumKey)
    {
        loader.SetActive(false);
        albumUI.gameObject.SetActive(false);
        photosUI.gameObject.SetActive(true);
        photosUI.DisplayAlbum(photosDataManager.data, albumKey);
    }

    public void LoadAlbums()
    {
        DisplayLoader();

        Task task = Task.Run(async () =>
        {
            await photosDataManager.FetchNextPageOfAlbumData();
        }).ContinueWith((t) =>
        {
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception);
            }
            if (t.IsCompleted) displayAlbumsOnNextFrame = true;
        });
    }

    public void LoadLibraryMediaItems()
    {
        DisplayLoader();

        Task task = Task.Run(async () =>
        {
            await photosDataManager.FetchNextPageOfLibraryMediaItems();
        }).ContinueWith((t) =>
        {
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception);
            }
            if (t.IsCompleted) displayLibraryOnNextFrame = true;
        });
    }

    public void LoadAlbumMediaItems(string albumKey)
    {
        DisplayLoader();

        Task task = Task.Run(async () =>
        {
            await photosDataManager.FetchNextPageOfMediaItemsInAlbum(albumKey);
        }).ContinueWith((t) =>
        {
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception);
            }
            if (t.IsCompleted) albumToDisplayOnNextFrame = albumKey;
        });
    }

    void Update()
    {
        if (displayAlbumsOnNextFrame)
        {
            DisplayAlbumUI();
            displayAlbumsOnNextFrame = false;
        }
        else if (displayLibraryOnNextFrame)
        {
            DisplayPhotosFromLibrary();
            displayLibraryOnNextFrame = false;
        }
        else if (albumToDisplayOnNextFrame != null)
        {
            DisplayPhotosFromAlbum(albumToDisplayOnNextFrame);
            albumToDisplayOnNextFrame = null;
        }
    }
}
