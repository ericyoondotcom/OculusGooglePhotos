using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        WWW www = new WWW(url);
        yield return www;
        image.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.zero);
    }
}
