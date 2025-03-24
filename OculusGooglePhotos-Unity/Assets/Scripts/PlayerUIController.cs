using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUIController : MonoBehaviour
{
    public InputAction hideUIbutton;
    public GameObject content;
    public PhotosDataManager photosDataManager;
    public GameObject loader;
    public ProgressBar loaderProgressBar;
    public GameObject hideUIHintText;
    public PhotosUI photosUI;

    bool displayLibraryOnNextFrame;
    bool isDisplayingPhotos;
    
    void Start()
    {
        hideUIbutton.performed += ctx => { OnHidePressed(); };
        photosUI.playerUIController = this;
        photosUI.photosUI.SetActive(false);
        hideUIHintText.SetActive(false);
        photosUI.photoDisplayer.onCurrentMediaItemChange.AddListener(OnCurrentMediaItemChanged);
        LoadLibraryMediaItems();
    }

    public void DisplayLoader(float progress = 0)
    {
        loader.SetActive(true);
        if (loaderProgressBar != null)
        {
            loaderProgressBar.gameObject.SetActive(progress > 0);
            loaderProgressBar.SetProgress(progress);
        }
    }
    public void HideLoader()
    {
        loader.SetActive(false);
    }

    public void DisplayPhotosFromLibrary()
    {
        loader.SetActive(false);
        photosUI.photosUI.SetActive(true);
        isDisplayingPhotos = true;
        photosUI.DisplayLibrary(photosDataManager.data);
    }

    public void LoadLibraryMediaItems()
    {
        DisplayLoader();

        Task task = Task.Run(async () =>
        {
            await photosDataManager.FetchNextPageOfLibraryMediaItems();
        }).ContinueWith((t) =>
        {
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception);
            }
            if (t.IsCompleted) displayLibraryOnNextFrame = true;
        });
    }

    public void OnCurrentMediaItemChanged()
    {
        hideUIHintText.SetActive(isDisplayingPhotos && photosUI.photoDisplayer.CurrentMediaItem != null);
    }

    public void SignOut()
    {
        AuthenticationManager.Instance.SignOut();
    }

    void Update()
    {
        if (displayLibraryOnNextFrame)
        {
            DisplayPhotosFromLibrary();
            displayLibraryOnNextFrame = false;
        }
    }

    void OnHidePressed() {
        if(content.activeSelf && isDisplayingPhotos && photosUI.photoDisplayer.CurrentMediaItem != null)
        {
            content.SetActive(false);
        } else
        {
            content.SetActive(true);
        }
    }

    void OnEnable()
    {
        hideUIbutton.Enable();
    }
    void OnDisable()
    {
        hideUIbutton.Disable();
    }
}
