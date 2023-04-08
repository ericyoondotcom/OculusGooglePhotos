using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class PhotoUIEntry : MonoBehaviour
{
    public Image image;
    public Button button;
    public GameObject selectedUI;

    public void SetSelected(bool isSelected)
    {
        selectedUI.SetActive(isSelected);
    }

    public IEnumerator DownloadThumbnail(MediaItem mediaItem, PhotosDataManager dataManager)
    {
        if(mediaItem.downloadedThumbnailSprite != null)
        {
            image.sprite = mediaItem.downloadedThumbnailSprite;
            yield break;
        }
        yield return dataManager.DownloadThumbnail(mediaItem, AfterThumbnailDownloaded);
    }

    public void AfterThumbnailDownloaded(MediaItem mediaItem)
    {
        image.sprite = mediaItem.downloadedThumbnailSprite;
    }
}
