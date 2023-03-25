using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    public const string ALL_PHOTOS_TEXT = "All photos";
    
    public PhotosDataManager photosDataManager;
    public GameObject loader;
    public AlbumUI albumUI;
    public PhotosUI photosUI;

    bool loadAlbumsOnNextCycle;

    void Start()
    {
        albumUI.playerUIController = this;
        photosUI.playerUIController = this;
        LoadAlbums();
        loader.SetActive(true);
        albumUI.gameObject.SetActive(false);
        photosUI.gameObject.SetActive(false);
    }

    public void LoadAlbums()
    {
        Task task = Task.Run(async () =>
        {
            await photosDataManager.FetchNextPageOfAlbumData();
        }).ContinueWith((t) =>
        {
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception);
            }
            if (t.IsCompleted) loadAlbumsOnNextCycle = true;
        });
    }

    void Update()
    {
        if (loadAlbumsOnNextCycle)
        {
            loader.SetActive(false);
            albumUI.gameObject.SetActive(true);
            albumUI.DisplayAlbums(photosDataManager.data);
            loadAlbumsOnNextCycle = false;
        }
        if (Input.GetKeyDown(KeyCode.X)) LoadAlbums();
    }
}
