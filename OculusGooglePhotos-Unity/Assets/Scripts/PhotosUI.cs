using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PhotosUI : MonoBehaviour
{
    public enum FilterMode
    {
        Unfiltered,
        SphericalPhotosOnly,
        StereoPhotosOnly,
        VideosOnly
    };
    public float paddingLoadMoreButton;
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
    Dictionary<string, PhotoUIEntry> instantiatedEntries = new Dictionary<string, PhotoUIEntry>();
    PhotoUIEntry selectedEntry;

    private void Start()
    {
        formatModal.SetActive(false);
        filterModal.SetActive(false);
        OnFormatSelect(Utility.PhotoTypes.RectangularMono);
    }

    public void DisplayLibrary(PhotosDataStore data)
    {
        if (!isShowingLibrary || displayedAlbumId != null)
        {
            DestroyAllEntries();
        }
        isShowingLibrary = true;
        albumTitle.text = PlayerUIController.ALL_PHOTOS_TEXT;
        displayedAlbumId = null;
        StartCoroutine(DisplayPhotos(data.library.mediaItems, data.library.hasMoreMediaItemsToLoad));
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
        StartCoroutine(DisplayPhotos(album.mediaItems, album.hasMoreMediaItemsToLoad));
    }

    IEnumerator DisplayPhotos(Dictionary<string, MediaItem> mediaItems, bool hasMoreMediaItemsToLoad)
    {
        yield return new WaitForEndOfFrame(); // Need to wait for canvas update to get size
        entryDimension = scrollViewContent.rect.width / numColumnsPerRow;

        int numRows = (int)Mathf.Ceil((float)mediaItems.Count / numColumnsPerRow);
        scrollViewContent.sizeDelta = new Vector2(
            0,
            entryDimension * numRows +
            paddingLoadMoreButton * 2 +
            loadMoreButton.rect.height
        );
        loadMoreButton.SetInsetAndSizeFromParentEdge(
            RectTransform.Edge.Top,
            entryDimension * numRows + paddingLoadMoreButton,
            loadMoreButton.rect.height
        );
        loadMoreButton.gameObject.SetActive(hasMoreMediaItemsToLoad);

        for (int i = 0; i < mediaItems.Count; i++)
        {
            var kvp = mediaItems.ElementAt(i);
            MediaItem mediaItem = kvp.Value;
            if (instantiatedPhotoKeys.Contains(kvp.Key)) continue;
            instantiatedPhotoKeys.Add(kvp.Key);

            int row = i / numColumnsPerRow;
            int col = i % numColumnsPerRow;

            GameObject newEntry = Instantiate(photoEntryPrefab, scrollViewContent);
            RectTransform rt = newEntry.GetComponent<RectTransform>();

            rt.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top,
                entryDimension * row,
                entryDimension
            );
            rt.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Left,
                entryDimension * col,
                entryDimension
            );

            PhotoUIEntry photoUIEntry = newEntry.GetComponent<PhotoUIEntry>();
            instantiatedEntries[kvp.Key] = photoUIEntry;
            string imageUrl = null;
            if (mediaItem.IsPhoto) imageUrl = mediaItem.baseUrl + "=w500-h500-c";
            else if (mediaItem.IsVideo) imageUrl = mediaItem.baseUrl + "=w500-h500-c";
            if(imageUrl != null) StartCoroutine(photoUIEntry.SetImageSprite(imageUrl));
            photoUIEntry.button.onClick.AddListener(() => OnSelectPhoto(mediaItem));
        }
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
        foreach (var kvp in instantiatedEntries)
        {
            Destroy(kvp.Value.gameObject);
        }
        instantiatedPhotoKeys.Clear();
        instantiatedEntries.Clear();
        scrollViewContent.anchoredPosition = Vector2.zero;
        selectedEntry = null;
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

    void OnSelectPhoto(MediaItem mediaItem)
    {
        playerUIController.DisplayLoader();
        if (!instantiatedEntries.ContainsKey(mediaItem.id)) return;
        PhotoUIEntry entry = instantiatedEntries[mediaItem.id];
        if (selectedEntry != null)
        {
            selectedEntry.SetSelected(false);
        }
        if (selectedEntry == entry)
        {
            selectedEntry = null;
        }
        else
        {
            entry.SetSelected(true);
            selectedEntry = entry;
        }

        if (mediaItem.downloadedTexture == null)
        {
            StartCoroutine(playerUIController.photosDataManager.DownloadMediaContent(mediaItem, AfterPhotoDownloaded));
        }
        else
        {
            AfterPhotoDownloaded(mediaItem);
        }
    }

    void AfterPhotoDownloaded(MediaItem mediaItem)
    {
        playerUIController.HideLoader();
        photoDisplayer.currentMediaItem = mediaItem;
        photoDisplayer.DisplayPhoto();
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
        photoDisplayer.PhotoType = type;
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
