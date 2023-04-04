using System.Collections;
using System.Collections.Generic;
using OVR.OpenVR;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PhotoDisplayer : MonoBehaviour
{
    public Material defaultSkyboxMaterial;
    public RawImage rectangularDisplayRaw;
    public RectTransform photoDisplayCanvas;
    public RenderTexture renderTexture;
    public VideoPlayer videoPlayer;

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
                    rectangularDisplayRaw.texture = currentMediaItem.downloadedTexture;
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

    public void DisplayVideo()
    {
        if (currentMediaItem == null) return;

        float aspectRatio = (float)currentMediaItem.width / currentMediaItem.height;

        renderTexture.width = currentMediaItem.width;
        renderTexture.height = currentMediaItem.height;

        videoPlayer.url = currentMediaItem.videoDownloadCanonURL;
        // TODO: Unfortunately, Unity video player has a bug where it only plays
        // files that end in .mp4 etc, even if the URL leads to valid bytes.
        // See https://forum.unity.com/threads/big-bug-in-5-6-videoplayer.469153/
        // So, we will have to download the bytes of the video and supply
        // them to the video player :(
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.Play();

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
                    rectangularDisplayRaw.texture = renderTexture;
                    break;
                }
            case Utility.PhotoTypes.RectangularStereo:
                {
                    break;
                }
            case Utility.PhotoTypes.SphericalMono:
                {
                    
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
