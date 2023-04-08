using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    public const string ALL_PHOTOS_TEXT = "All photos";

    public GameObject content;
    public PhotosDataManager photosDataManager;
    public GameObject loader;
    public ProgressBar loaderProgressBar;
    public AlbumUI albumUI;
    public PhotosUI photosUI;

    bool displayAlbumsOnNextFrame;
    bool displayLibraryOnNextFrame;
    string albumToDisplayOnNextFrame;
    bool isDisplayingAlbums;
    bool isDisplayingPhotos;
    
    void Start()
    {
        albumUI.playerUIController = this;
        photosUI.playerUIController = this;
        albumUI.albumUI.SetActive(false);
        photosUI.photosUI.SetActive(false);
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
        albumUI.albumUI.SetActive(true);
        photosUI.photosUI.SetActive(false);
        isDisplayingAlbums = true;
        isDisplayingPhotos = false;
        albumUI.DisplayAlbums(photosDataManager.data);
        photosUI.photoDisplayer.StopDisplaying();
        photosUI.ClearSelection();
    }

    public void DisplayPhotosFromLibrary()
    {
        loader.SetActive(false);
        albumUI.albumUI.SetActive(false);
        photosUI.photosUI.SetActive(true);
        isDisplayingAlbums = false;
        isDisplayingPhotos = true;
        photosUI.DisplayLibrary(photosDataManager.data);
    }

    public void DisplayPhotosFromAlbum(string albumKey)
    {
        loader.SetActive(false);
        albumUI.albumUI.SetActive(false);
        photosUI.photosUI.SetActive(true);
        isDisplayingAlbums = false;
        isDisplayingPhotos = true;
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

    public void SignOut()
    {
        AuthenticationManager.Instance.SignOut();
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
        if (OVRInput.GetDown(OVRInput.Button.Start) || Input.GetKeyDown(KeyCode.Escape))
        {
            if(content.activeSelf && isDisplayingPhotos && photosUI.photoDisplayer.currentMediaItem != null)
            {
                content.SetActive(false);
            } else
            {
                content.SetActive(true);
            }
        }
    }
}
