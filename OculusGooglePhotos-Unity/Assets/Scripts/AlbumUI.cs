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

    List<string> instantiatedAlbumKeys = new List<string>();

    private void Start()
    {
        entryHeight = albumEntryPrefab.GetComponent<RectTransform>().rect.height;
    }

    public void DisplayAlbums(PhotosDataStore data)
    {
        scrollViewContent.sizeDelta = new Vector2(
            0,
            (entryHeight + entryGap) * data.albums.Count +
            contentHeightAddition +
            loadMoreButton.rect.height +
            entryGap
        );
        loadMoreButton.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (entryHeight + entryGap) * data.albums.Count, loadMoreButton.rect.height);
        loadMoreButton.gameObject.SetActive(data.hasMoreAlbumPagesToLoad);

        for (int i = 0; i < data.albums.Count; i++) {
            
            var kvp = data.albums.ElementAt(i);
            Album album = kvp.Value;
            if (instantiatedAlbumKeys.Contains(kvp.Key)) continue;
            instantiatedAlbumKeys.Add(kvp.Key);
            GameObject newEntry = Instantiate(albumEntryPrefab, scrollViewContent);
            RectTransform rt = newEntry.GetComponent<RectTransform>();
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (entryHeight + entryGap) * i, entryHeight);
            AlbumUIEntry albumUIEntry = newEntry.GetComponent<AlbumUIEntry>();

            string imageUrl = album.coverPhotoBaseUrl + "=w500-h500-c";
            StartCoroutine(albumUIEntry.SetImageSprite(imageUrl));
            albumUIEntry.SetText(album.title);
        }
    }

    public void DestroyAllEntries()
    {
        foreach (Transform child in scrollViewContent)
        {
            if (child.gameObject.GetComponent<AlbumUIEntry>() != null)
                Destroy(child.gameObject);
        }
        instantiatedAlbumKeys.Clear();
    }

    public void LoadMore()
    {
        Debug.Log("Load more!");
        playerUIController.LoadAlbums();
    }
}
