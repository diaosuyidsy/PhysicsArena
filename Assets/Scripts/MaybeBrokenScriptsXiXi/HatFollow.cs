using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatFollow : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset;
    public float SmoothSpeed = 0.125f;

    void LateUpdate()
    {
        Vector3 desiredPos = Target.position + Offset;
        Vector3 smoothedPos = Vector3.Lerp (transform.position, desiredPos, SmoothSpeed);
        transform.position = smoothedPos;
    }

}
