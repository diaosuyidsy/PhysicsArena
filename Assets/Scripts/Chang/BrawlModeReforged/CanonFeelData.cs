using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonFeelData : MonoBehaviour
{
    public float MinShootDisShape;
    public float MaxShootDisShape;
    public float MinPercentage;
    public float MaxPercentage;

    public float MinLinearLimit;
    public float MaxLinearLimit;
    public float MinPadDis;
    public float MaxPadDis;

    public float PercentageFollowSpeed;
    public float PercentageIgnoreError;

    public float RotateSpeed;


    public float MaxPipeAngle;
    public float PipeRotateSpeed;

    public float CabelStartEmission;
    public float CabelEndEmission;
    public float CabelShineTime;

    public Color RedCabelColor;
    public Color BlueCabelColor;

    public Vector3 PipeEndStartLocalPos;
    public float PipeEndFireTime;
    public float PipeEndRecoverTime;
    public float PipeEndShakeDis;

    public float MarkAppearTime;
    public Color MarkDefaultColor;
    public Color MarkAlertColor;

    public GameObject BombPrefab;
    public float BombInvisibleHeight;
    public float BombRiseTime;
    public float BombFallTime;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
