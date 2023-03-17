using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotate : MonoBehaviour
{
    public Vector3 rotationPerSecond;

    void Start()
    {

    }

    void FixedUpdate()
    {
        transform.Rotate(rotationPerSecond * Time.fixedDeltaTime);
    }
}
