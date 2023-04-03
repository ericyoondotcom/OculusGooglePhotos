using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlbumUIEntry : MonoBehaviour
{
    public Button button;
    public Image image;
    public TextMeshProUGUI text;

    public IEnumerator SetImageSprite(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        image.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.zero);
    }

    public void SetText(string content)
    {
        text.text = content;
    }
}
