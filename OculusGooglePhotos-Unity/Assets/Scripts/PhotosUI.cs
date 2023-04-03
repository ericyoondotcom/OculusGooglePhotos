using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotosUI : MonoBehaviour
{
    public float entryGap;
    public float contentHeightAddition;
    public int numColumnsPerRow;
    public Sprite iconRectangularMono;
    public Sprite iconRectangularStereo;
    public Sprite iconSphericalMono;
    public Sprite iconSphericalStereo;

    public Image formatButtonIcon;
    public GameObject formatPicker;
    public RectTransform scrollViewContent;
    public RectTransform loadMoreButton;
    public TextMeshProUGUI albumTitle;
    public GameObject photoEntryPrefab;
    [System.NonSerialized]
    public PlayerUIController playerUIController;

    float entryDimension;
    bool isShowingLibrary = false;
    string displayedAlbumId = null;
    List<string> instantiatedPhotoKeys = new List<string>();
    List<GameObject> instantiatedEntries = new List<GameObject>();

    private void Start()
    {
        entryDimension = scrollViewContent.rect.width / numColumnsPerRow;
    }

    public void DisplayLibrary(Dictionary<string, MediaItem> mediaItems)
    {
        if(!isShowingLibrary || displayedAlbumId != null)
        {
            DestroyAllEntries();
        }
        isShowingLibrary = true;
        albumTitle.text = PlayerUIController.ALL_PHOTOS_TEXT;
        displayedAlbumId = null;
        DisplayPhotos(mediaItems);
    }

    public void DisplayAlbum(Album album)
    {
        if(isShowingLibrary || displayedAlbumId != album.id)
        {
            DestroyAllEntries();
        }
        isShowingLibrary = false;
        albumTitle.text = album.title;
        displayedAlbumId = album.id;
        DisplayPhotos(album.mediaItems);
    }

    void DisplayPhotos(Dictionary<string, MediaItem> mediaItems)
    {

    }

    public void DestroyAllEntries()
    {
        foreach (GameObject go in instantiatedEntries)
        {
            Destroy(go);
        }
        instantiatedPhotoKeys.Clear();
        instantiatedEntries.Clear();
    }

    public void LoadMore()
    {
        playerUIController.LoadAlbums();
    }

    void OnSelectPhoto(string photoId)
    {
        // TODO
        // Check aspect ratio.
            // If 2:1, display format picker modal with options "rectangular" and "spherical mono"
            // If 1:1, display format picker modal with options "rectangular" and "spherical stereo"
    }

    public void OnFormatSelect(string type)
    {
        switch (type)
        {
            case "rectangular mono":
                OnFormatSelect(Utility.PhotoTypes.RectangularMono);
                break;
            case "rectangular stereo":
                OnFormatSelect(Utility.PhotoTypes.RectangularStereo);
                break;
            case "spherical mono":
                OnFormatSelect(Utility.PhotoTypes.SphericalMono);
                break;
            case "spherical stereo":
                OnFormatSelect(Utility.PhotoTypes.SphericalStereo);
                break;
            default:
                OnFormatSelect(Utility.PhotoTypes.Unspecified);
                break;
        }
    }
    public void OnFormatSelect(Utility.PhotoTypes type)
    {

    }
}
