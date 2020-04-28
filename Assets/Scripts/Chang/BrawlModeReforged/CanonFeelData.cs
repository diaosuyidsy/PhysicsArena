using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonFeelData : MonoBehaviour
{
    public float MinShootDisShape;
    public float MaxShootDisShape;
    public float MaxPercentage;

    public float MinLinearLimit;
    public float MaxLinearLimit;
    public float MinPadDis;
    public float MaxPadDis;
    public float MinJoint0AngularYZ;
    public float MaxJoint0AngularYZ;
    public float MinJoint1AngularYZ;
    public float MaxJoint1AngularYZ;
    public float MinJointRotation;
    public float MaxJointRotation;
    public float MinAmmoPos;
    public float MaxAmmoPos;
    public float AmmoOffset;

    public float AimingPercentageFollowSpeed;
    public float ShootPercentageFollowSpeed;
    public float PercentageIgnoreError;

    public float RotateSpeed;

    public float CabelStartEmission;
    public float CabelEndEmission;
    public float CabelShineTime;

    public Color RedCabelColor;
    public Color BlueCabelColor;

    public Vector3 AimedMarkOffset;

    public float MarkAppearTime;
    public Color MarkDefaultColor;
    public Color MarkAlertColor;
    public Color MarkFallColor;

    public Material WrapRedMat;
    public Material WrapBlueMat;
    public Material WrapNMat;

    public float AimingCameraWeight;

    public float DeliverFallTime;
    public float DeliverMoveAcTime;
    public float DeliverMoveStableTime;
    public float DeliverMoveDcTime;
    

    public float CabelFirstSegAcTime;
    public float CabelFirstSegStableTime;
    public float CabelFirstSegDcTime;

    public float CabelSecondSegTime;

    public float CabelThirdSegAcTime;
    public float CabelThirdSegStableTime;

    public float DeliverJumpTime;
    public float DeliverJumpVerSpeed;
    public float DeliverJumpEndHoriSpeed;
    public int DeliverJumpEndExtraSpin;

    public float BombVerticalSpeedPercentage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
