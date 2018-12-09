﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [HideInInspector]
    public Vector3 FollowTarget;

    public float SmoothSpeed = 0.04f;

    public float CameraScaleSpeed = 1f;
    public float FOVSizeMin = 8f;
    public float FOVSizeMax = 15f;

    private float _maxDistanceOrigin;
    private float _xDiffOrigin;
    private float _zDiffOrigin;
    private Vector3 _desiredPosition;
    private Vector3 _smoothedPosition;
    private float _desiredFOV;
    private float _smoothedFOV;

    // Use this for initialization
    void Start ()
    {
        SetTarget ();
        // Set the max Distance originally
        float maxDist = 0f;
        foreach (GameObject go in GameManager.GM.Players)
        {
            float temp = Vector3.Distance (go.transform.position, FollowTarget);
            if (temp > maxDist)
            {
                maxDist = temp;
            }
        }
        _maxDistanceOrigin = maxDist;
        // Set X and Z Original Difference
        _xDiffOrigin = transform.position.x - FollowTarget.x;
        _zDiffOrigin = transform.position.z - FollowTarget.z;
    }

    // Update is called once per frame
    void Update ()
    {
        SetTarget ();
        _desiredPosition = new Vector3 (FollowTarget.x + _xDiffOrigin, transform.position.y, FollowTarget.z + _zDiffOrigin);
        _smoothedPosition = Vector3.Lerp (transform.position, _desiredPosition, SmoothSpeed);
        transform.position = _smoothedPosition;
        _desiredFOV = GetComponent<Camera> ().fieldOfView + (MaxDistance () - _maxDistanceOrigin) * CameraScaleSpeed;
        _smoothedFOV = Mathf.Lerp (GetComponent<Camera> ().fieldOfView, _desiredFOV, SmoothSpeed);
        GetComponent<Camera> ().fieldOfView = _smoothedFOV;
        _maxDistanceOrigin = MaxDistance ();
        GetComponent<Camera> ().fieldOfView = Mathf.Clamp (GetComponent<Camera> ().fieldOfView, FOVSizeMin, FOVSizeMax);
    }

    void SetTarget ()
    {
        // Need to set Follow Target to be average of all players
        Vector3 total = Vector3.zero;
        foreach (GameObject go in GameManager.GM.Players)
        {
            if (go != null)
                total += go.transform.position;
        }
        total /= GameManager.GM.Players.Length;
        FollowTarget = total;
    }

    float MaxDistance ()
    {
        float maxDist = 0f;
        foreach (GameObject go in GameManager.GM.Players)
        {
            float temp = Vector3.Distance (go.transform.position, FollowTarget);
            if (temp > maxDist)
            {
                maxDist = temp;
            }
        }
        return maxDist;
    }
}
