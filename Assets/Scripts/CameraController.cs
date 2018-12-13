using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [HideInInspector]
    public Vector3 FollowTarget;

    public float SmoothSpeed = 0.04f;

    public float CameraScaleSpeed = 1f;
    public float FOVSizeMin = 8f;
    public float FOVSizeMax = 35f;
    public float XOffset = 0f;
    public float ZOffset = -0.33f;
    public float WonFOVSize = 6f;

    private float _maxDistanceOrigin;
    private float _xDiffOrigin;
    private float _zDiffOrigin;
    private Vector3 _desiredPosition;
    private Vector3 _smoothedPosition;
    private float _desiredFOV;
    private float _smoothedFOV;
    private bool _winLock = false;

    // Use this for initialization
    void Start ()
    {
        SetTarget (false);
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
        //_xDiffOrigin = transform.position.x - FollowTarget.x;
        _zDiffOrigin = transform.position.z - FollowTarget.z;
        _xDiffOrigin = 0f;
        //iffOrigin = 0f;
    }

    // Update is called once per frame
    void Update ()
    {
        // If player won, then do Won Logic and lock others
        if (_winLock)
        {
            _desiredPosition = new Vector3 (FollowTarget.x + _xDiffOrigin, transform.position.y, FollowTarget.z + _zDiffOrigin);
            _smoothedPosition = Vector3.Lerp (transform.position, _desiredPosition, SmoothSpeed);
            transform.position = _smoothedPosition;
            GetComponent<Camera> ().fieldOfView = Mathf.Lerp (GetComponent<Camera> ().fieldOfView, WonFOVSize, SmoothSpeed);
            return;
        }
        SetTarget ();
        _desiredPosition = new Vector3 (FollowTarget.x + _xDiffOrigin, transform.position.y, FollowTarget.z + _zDiffOrigin);
        _smoothedPosition = Vector3.Lerp (transform.position, _desiredPosition, SmoothSpeed);
        transform.position = _smoothedPosition;
        //GetComponent<Camera> ().fieldOfView += (MaxDistance () - _maxDistanceOrigin) * CameraScaleSpeed;
        //_maxDistanceOrigin = MaxDistance ();
        _desiredFOV = 1.5f * MaxDistance () + 3.99f;
        GetComponent<Camera> ().fieldOfView = Mathf.Lerp (GetComponent<Camera> ().fieldOfView, _desiredFOV, SmoothSpeed);
        GetComponent<Camera> ().fieldOfView = Mathf.Clamp (GetComponent<Camera> ().fieldOfView, FOVSizeMin, FOVSizeMax);
    }

    public void OnWinCameraZoom (Transform tar)
    {
        _winLock = true;
        FollowTarget = tar.position;
    }

    void SetTarget (bool withOffset = true)
    {
        // Need to set Follow Target to be average of all players
        Vector3 total = Vector3.zero;
        int length = 0;
        foreach (GameObject go in GameManager.GM.Players)
        {
            if (go.GetComponent<PlayerController> ().LegSwingReference.activeSelf)
            {
                total += go.transform.position;
                length++;
            }
        }
        total /= length;
        if (withOffset)
        {
            total.x += XOffset;
            total.z += ZOffset;
        }
        FollowTarget = total;
    }

    // Should get the max distance between any two players
    float MaxDistance ()
    {
        float maxDist = 0f;
        for (int i = 0; i < GameManager.GM.Players.Length; i++)
        {
            if (!GameManager.GM.Players[i].GetComponent<PlayerController> ().LegSwingReference.activeSelf)
                continue;
            for (int j = i + 1; j < GameManager.GM.Players.Length; j++)
            {
                if (!GameManager.GM.Players[j].GetComponent<PlayerController> ().LegSwingReference.activeSelf)
                    continue;
                float temp = Vector3.Distance (GameManager.GM.Players[i].transform.position, GameManager.GM.Players[j].transform.position);
                if (temp > maxDist)
                {
                    maxDist = temp;
                }
            }
        }
        return maxDist;
    }
}
