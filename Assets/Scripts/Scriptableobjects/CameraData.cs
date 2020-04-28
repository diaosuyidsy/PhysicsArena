using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraData", menuName = "ScriptableObjects/CameraData", order = 1)]
public class CameraData : ScriptableObject
{
    public float SmoothSpeed = 0.04f;

    public float FOVSizeMin = 8f;
    public float FOVSizeMax = 35f;
    public float CameraDistance = 20f;
    public float WonFOVSize = 6f;
}
