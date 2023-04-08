using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlbumUI : MonoBehaviour
{
    public float entryGap;

    public GameObject albumUI;
    public RectTransform scrollViewContent;
    public RectTransform loadMoreButton;
    public GameObject albumEntryPrefab;
    [System.NonSerialized]
    public PlayerUIController playerUIController;

    float entryHeight;
    List<string> instantiatedAlbumKeys = new List<string>();
    List<GameObject> instantiatedEntries = new List<GameObject>();

    const int NUM_PERSISTENT_ENTRIES = 1;

    public void DisplayAlbums(PhotosDataStore data)
    {
        entryHeight = albumEntryPrefab.GetComponent<RectTransform>().rect.height;
        scrollViewContent.sizeDelta = new Vector2(
            0,
            (entryHeight + entryGap) * (data.albums.Count + NUM_PERSISTENT_ENTRIES) +
            loadMoreButton.rect.height +
            entryGap
        );
        loadMoreButton.SetInsetAndSizeFromParentEdge(
            RectTransform.Edge.Top,
            (entryHeight + entryGap) * (data.albums.Count + NUM_PERSISTENT_ENTRIES),
            loadMoreButton.rect.height
        );
        loadMoreButton.gameObject.SetActive(data.hasMoreAlbumPagesToLoad);

        for (int i = 0; i < data.albums.Count; i++) {
            
            var kvp = data.albums.ElementAt(i);
            Album album = kvp.Value;
            if (instantiatedAlbumKeys.Contains(kvp.Key)) continue;
            instantiatedAlbumKeys.Add(kvp.Key);
            GameObject newEntry = Instantiate(albumEntryPrefab, scrollViewContent);
            instantiatedEntries.Add(newEntry);
            RectTransform rt = newEntry.GetComponent<RectTransform>();
            rt.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top,
                (entryHeight + entryGap) * (i + NUM_PERSISTENT_ENTRIES),
                entryHeight
            );
            
            AlbumUIEntry albumUIEntry = newEntry.GetComponent<AlbumUIEntry>();
            string imageUrl = album.coverPhotoBaseUrl + "=w500-h500-c";
            StartCoroutine(albumUIEntry.SetImageSprite(imageUrl));
            albumUIEntry.SetText(album.title);
            albumUIEntry.button.onClick.AddListener(() => OnSelectAlbum(kvp.Key));
        }
    }

    public void DestroyAllEntries()
    {
        foreach (GameObject go in instantiatedEntries)
        {
            Destroy(go);
        }
        instantiatedAlbumKeys.Clear();
        instantiatedEntries.Clear();
    }

    public void LoadMore()
    {
        playerUIController.LoadAlbums();
    }

    public void OnSelectLibrary()
    {
        playerUIController.LoadLibraryMediaItems();
    }

    void OnSelectAlbum(string albumKey)
    {
        playerUIController.LoadAlbumMediaItems(albumKey);
    }
}
