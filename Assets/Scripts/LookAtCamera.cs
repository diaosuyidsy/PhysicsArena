using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform Cam;

    void Update()
    {
		if (Cam == null)
			transform.LookAt(Camera.main.transform.position);
		else
			transform.rotation = Cam.rotation;
    }
}