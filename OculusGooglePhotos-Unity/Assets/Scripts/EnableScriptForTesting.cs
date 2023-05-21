using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableScriptForTesting : MonoBehaviour
{
    public MonoBehaviour script;
#if UNITY_EDITOR
    private void Start()
    {
        script.enabled = true;
    }
#endif
}
