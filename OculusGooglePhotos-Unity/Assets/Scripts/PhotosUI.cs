using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotosUI : MonoBehaviour
{
    public enum FilterMode
    {
        Unfiltered,
        SphericalPhotosOnly,
        StereoPhotosOnly,
        VideosOnly
    };

    public float entryGap;
    public float contentHeightAddition;
    public int numColumnsPerRow;
    public Sprite iconRectangularMono;
    public Sprite iconRectangularStereo;
    public Sprite iconSphericalMono;
    public Sprite iconSphericalStereo;

    public Image formatButtonIcon;
    public GameObject formatModal;
    public GameObject filterModal;
    public RectTransform scrollViewContent;
    public RectTransform loadMoreButton;
    public TextMeshProUGUI albumTitle;
    public GameObject photoEntryPrefab;
    public PhotoDisplayer photoDisplayer;
    [System.NonSerialized]
    public PlayerUIController playerUIController;
    [System.NonSerialized]
    FilterMode filterMode = FilterMode.Unfiltered;

    float entryDimension;
    bool isShowingLibrary = false;
    string displayedAlbumId = null;
    List<string> instantiatedPhotoKeys = new List<string>();
    List<GameObject> instantiatedEntries = new List<GameObject>();

    private void Start()
    {
        entryDimension = scrollViewContent.rect.width / numColumnsPerRow;
        formatModal.SetActive(false);
        filterModal.SetActive(false);
        OnFormatSelect(Utility.PhotoTypes.RectangularMono);
    }

    public void DisplayLibrary(PhotosDataStore data)
    {
        if(!isShowingLibrary || displayedAlbumId != null)
        {
            DestroyAllEntries();
        }
        isShowingLibrary = true;
        albumTitle.text = PlayerUIController.ALL_PHOTOS_TEXT;
        displayedAlbumId = null;
        DisplayPhotos(data.library.mediaItems);
    }

    public void DisplayAlbum(PhotosDataStore data, string albumKey)
    {
        Album album = data.albums[albumKey];
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

    void Refresh()
    {
        if (isShowingLibrary)
        {
            playerUIController.DisplayPhotosFromLibrary();
        }
        else
        {
            playerUIController.DisplayPhotosFromAlbum(displayedAlbumId);
        }
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
        if (isShowingLibrary)
        {
            playerUIController.LoadLibraryMediaItems();
        }
        else
        {
            playerUIController.LoadAlbumMediaItems(displayedAlbumId);
        }
    }

    void OnSelectPhoto(string photoId)
    {
        // TODO
        // Make a web request to get the photo with the =d flag
        // Check the XMP data for a projection field, and auto-set the photo type
        // Set currentMediaItem of photoDisplayer and call its DisplayPhoto()
    }

    public void OnFilterButtonClick()
    {
        formatModal.SetActive(false);
        filterModal.SetActive(!filterModal.activeSelf);
    }

    public void OnFilterModeSelect(string filterMode)
    {
        switch (filterMode)
        {
            case "spherical":
                OnFilterModeSelect(FilterMode.SphericalPhotosOnly);
                break;
            case "stereo":
                OnFilterModeSelect(FilterMode.StereoPhotosOnly);
                break;
            case "video":
                OnFilterModeSelect(FilterMode.VideosOnly);
                break;
            case "unfiltered":
            default:
                OnFilterModeSelect(FilterMode.Unfiltered);
                break;
        }
    }

    public void OnFilterModeSelect(FilterMode filterMode)
    {
        filterModal.SetActive(false);
        this.filterMode = filterMode;
        Refresh();
    }

    public void OnFormatButtonClick()
    {
        formatModal.SetActive(!formatModal.activeSelf);
        filterModal.SetActive(false);
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
        formatModal.SetActive(false);
        photoDisplayer.photoType = type;
        switch (type)
        {
            case Utility.PhotoTypes.RectangularMono:
                formatButtonIcon.sprite = iconRectangularMono;
                break;
            case Utility.PhotoTypes.RectangularStereo:
                formatButtonIcon.sprite = iconRectangularStereo;
                break;
            case Utility.PhotoTypes.SphericalMono:
                formatButtonIcon.sprite = iconSphericalMono;
                break;
            case Utility.PhotoTypes.SphericalStereo:
                formatButtonIcon.sprite = iconSphericalStereo;
                break;
            case Utility.PhotoTypes.Unspecified:
            default:
                formatButtonIcon.sprite = null;
                break;
        }
    }
}
