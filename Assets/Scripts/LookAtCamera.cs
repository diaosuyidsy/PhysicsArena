using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform Cam;

    void Update()
    {
        if (Cam == null)
            transform.rotation = Camera.main.transform.rotation;
        else
            transform.rotation = Cam.rotation;
    }
}