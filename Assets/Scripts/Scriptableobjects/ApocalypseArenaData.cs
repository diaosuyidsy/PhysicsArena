using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ApocalypseArenaData", menuName = "ScriptableObjects/ApocalypseArenaData", order = 1)]
public class ApocalypseArenaData : ModeSepcificData
{
    public float ApocalypsePrepareTime;
    public float ApocalypseTime;
    public float ApocalypseAlertTime;

    public float ApocalypseFollowSpeed;

    public int ApoTrapDeathScore = 3;
    public int NormalDeathScore = 1;
    public int TotalTime = 180;
}
