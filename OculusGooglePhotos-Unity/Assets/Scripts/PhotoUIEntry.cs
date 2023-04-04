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

    public IEnumerator SetImageSprite(string url)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image fetch returned error: " + req.result);
                yield break;
            }
            var texture = DownloadHandlerTexture.GetContent(req);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
    }
}
