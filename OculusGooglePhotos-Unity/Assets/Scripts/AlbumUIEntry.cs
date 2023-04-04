using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class AlbumUIEntry : MonoBehaviour
{
    public Button button;
    public Image image;
    public TextMeshProUGUI text;

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

    public void SetText(string content)
    {
        text.text = content;
    }
}
