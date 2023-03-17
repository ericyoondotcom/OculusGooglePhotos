using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlbumUI : MonoBehaviour
{
    float entryHeight;
    public float entryGap;
    public float contentHeightAddition;

    public RectTransform scrollViewContent;
    public RectTransform loadMoreButton;
    public GameObject albumEntryPrefab;
    [System.NonSerialized]
    public PlayerUIController playerUIController;

    private void Start()
    {
        entryHeight = albumEntryPrefab.GetComponent<RectTransform>().rect.height;
    }

    public void DisplayAlbums(PhotosDataStore data)
    {
        foreach(Transform child in scrollViewContent) {
            Destroy(child);
        }

        scrollViewContent.sizeDelta = new Vector2(0, (entryHeight + entryGap) * data.albums.Count + contentHeightAddition);
        loadMoreButton.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (entryHeight + entryGap) * data.albums.Count, loadMoreButton.rect.height);
        loadMoreButton.gameObject.SetActive(data.hasMoreAlbumPagesToLoad);

        for (int i = 0; i < data.albums.Count; i++) {
            Album album = data.albums.ElementAt(i).Value;
            GameObject newEntry = Instantiate(albumEntryPrefab, scrollViewContent);
            RectTransform rt = newEntry.GetComponent<RectTransform>();
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (entryHeight + entryGap) * i, entryHeight);
            AlbumUIEntry albumUIEntry = newEntry.GetComponent<AlbumUIEntry>();

            string imageUrl = album.coverPhotoBaseUrl + "=w500-h500";
            StartCoroutine(albumUIEntry.SetImageSprite(imageUrl));
            albumUIEntry.SetText(album.title);
        }
    }

    public void LoadMore()
    {
        playerUIController.LoadAlbums();
    }
}
