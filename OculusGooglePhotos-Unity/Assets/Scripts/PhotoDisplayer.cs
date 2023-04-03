using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoDisplayer : MonoBehaviour
{
    public MediaItem currentMediaItem;

    Utility.PhotoTypes _photoType;
    public Utility.PhotoTypes photoType
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

    public void DisplayPhoto()
    {
        if (currentMediaItem == null) return;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
