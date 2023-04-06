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
            if (currentMediaItem != null)
            {
                if (currentMediaItem.IsPhoto) DisplayPhoto();
                else if (currentMediaItem.IsVideo) DisplayVideo();
            }
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
        if (currentMediaItem.downloadedImageTexture == null) return;

        float aspectRatio = (float)currentMediaItem.width / currentMediaItem.height;

        videoPlayer.Pause();
        videoPlayer.url = "";
        Debug.Log(PhotoType);
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
                    rectangularDisplayRaw.texture = currentMediaItem.downloadedImageTexture;
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
                    mat.mainTexture = currentMediaItem.downloadedImageTexture;
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
        int rtWidth = currentMediaItem.width;
        int rtHeight = currentMediaItem.height;

        if (currentMediaItem.width == currentMediaItem.height && PhotoType == Utility.PhotoTypes.SphericalMono)
        {
            // Google Photos API incorrectly changes the aspect ratio of
            // monoscopic spherical videos. See my issue on the issue tracker:
            // https://issuetracker.google.com/issues/277176957
            // This code fixes this.
            rtWidth = rtHeight * 2;
        }

        RenderTexture renderTexture = new RenderTexture(currentMediaItem.width, currentMediaItem.height, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt, 0);

        videoPlayer.url = "file://" + currentMediaItem.downloadedVideoFilePath;
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
                    photoDisplayCanvas.gameObject.SetActive(false);
                    Material mat = new Material(skyboxShader);
                    mat.mainTexture = renderTexture;
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

    public void StopDisplaying()
    {
        videoPlayer.Pause();
        videoPlayer.url = "";
        RenderSettings.skybox = defaultSkyboxMaterial;
        photoDisplayCanvas.gameObject.SetActive(false);
    }
}
