using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClass", menuName = "ScriptableObjects/PlayerClass/Wind", order = 1)]
public class PlayerClassData_Wind : ScriptableObject
{
    public float FanStrikeAnticipationDuration = 0f;
    public float FanStrikeDuration = 0.8f;
    public float FanStrikeRecoveryDuration = 0.3f;
    public float FanStrikeSlowPercent = 0.5f;
    public float FanStrikeSlowRotatePercent = 0.8f;
}
