using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    public GameObject loader;
    public AlbumUI albumUI;
    public PhotosUI photosUI;

    void Start()
    {
        LoadAlbums();
    }

    async void LoadAlbums()
    {
        string accessToken = await AuthenticationManager.Instance.GetAccessToken();
        if (accessToken.Length == 0) return;


    }
}
