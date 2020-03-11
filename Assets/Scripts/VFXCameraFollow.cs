using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXCameraFollow : MonoBehaviour
{
    private Camera _mainCam;

    private Camera _thisCam;
    // Start is called before the first frame update
    private void OnEnable()
    {
        _mainCam = transform.parent.GetComponent<Camera>();
        _thisCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        _thisCam.fieldOfView = _mainCam.fieldOfView;
    }
}
