using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BrawlModeReforgedModeData", menuName = "ScriptableObjects/BrawlModeReforgedModeData", order = 1)]
public class BrawlModeReforgedModeData : ModeSepcificData
{
    public float CanonFireTime;
    public float CanonFireAlertTime;
    public float CanonCoolDown;

    public float CanonRadius;
    public float CanonPower;

    public int BagelKillPoint;
    public int NormalKillPoint;
    public int TotalTime;
}
