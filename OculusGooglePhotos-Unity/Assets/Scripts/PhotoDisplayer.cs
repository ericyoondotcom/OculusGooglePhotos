using System.Collections;
using System.Collections.Generic;
using OVR.OpenVR;
using UnityEngine;
using UnityEngine.UI;

public class PhotoDisplayer : MonoBehaviour
{
    public Material defaultSkyboxMaterial;
    public Image rectangularDisplay;
    public RectTransform photoDisplayCanvas;

    [System.NonSerialized]
    public MediaItem currentMediaItem;

    Shader skyboxShader;

    Utility.PhotoTypes _photoType;
    public Utility.PhotoTypes PhotoType
    {
        get
        {
            return _photoType;
        }
        set
        {
            _photoType = value;
            DisplayPhoto();
        }
    }

    void Start()
    {
        skyboxShader = Shader.Find("Skybox/Panoramic");
        photoDisplayCanvas.gameObject.SetActive(false);
    }

    public void DisplayPhoto()
    {
        if (currentMediaItem == null) return;
        if (currentMediaItem.downloadedTexture == null) return;

        float aspectRatio = (float)currentMediaItem.width / currentMediaItem.height;

        switch (PhotoType)
        {
            case Utility.PhotoTypes.RectangularMono:
                {
                    RenderSettings.skybox = defaultSkyboxMaterial;
                    photoDisplayCanvas.gameObject.SetActive(true);
                    photoDisplayCanvas.sizeDelta = new Vector2(
                        photoDisplayCanvas.sizeDelta.y * aspectRatio,
                        photoDisplayCanvas.sizeDelta.y
                    );
                    rectangularDisplay.sprite = Sprite.Create(
                        currentMediaItem.downloadedTexture,
                        new Rect(0, 0, currentMediaItem.downloadedTexture.width, currentMediaItem.downloadedTexture.height),
                        Vector2.zero
                    ); 
                    break;
                }
            case Utility.PhotoTypes.RectangularStereo:
                {
                    break;
                }
            case Utility.PhotoTypes.SphericalMono:
                {
                    photoDisplayCanvas.gameObject.SetActive(false);
                    Material mat = new Material(skyboxShader);
                    mat.mainTexture = currentMediaItem.downloadedTexture;
                    mat.SetFloat("Mapping", 1);
                    RenderSettings.skybox = mat;
                    break;
                }
            case Utility.PhotoTypes.SphericalStereo:
                {
                    break;
                }
            case Utility.PhotoTypes.Unspecified:
            default:
                {
                    RenderSettings.skybox = defaultSkyboxMaterial;
                    photoDisplayCanvas.gameObject.SetActive(false);
                    break;
                }
        }
    }
}
