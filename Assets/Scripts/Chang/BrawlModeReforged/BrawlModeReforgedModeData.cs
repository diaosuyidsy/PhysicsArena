using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BrawlModeReforgedModeData", menuName = "ScriptableObjects/BrawlModeReforgedModeData", order = 1)]
public class BrawlModeReforgedModeData : ModeSepcificData
{
    public float CanonFireTime;
    public float CanonFireAlertTime;
    public float CanonFireFinalTime;
    public float CanonCooldown;
    public float CanonSwitchTime;



    public float NormalFollowSpeed;
    public float AlertFollowSpeed;
    public float CanonRadius;
    public float CanonPower;
    public int MaxCanonFireCount;

    public int TargetScore;
    public int CloseScore;
    public int DeliveryPoint;
    public int BagelKillPoint;
    public int NormalKillPoint;
    public int TotalTime;

    public Vector3 BagelGenerationPos;
    public Vector3 BagelGenerationPosLeft;
    public Vector3 BagelGenerationPosRight;
}
