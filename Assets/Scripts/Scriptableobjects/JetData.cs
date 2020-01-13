using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JetpackData", menuName = "ScriptableObjects/EquipmentData/JetpackData", order = 1)]
public class JetData : EquipmentDataBase
{
    public float JumpForce = 400f;
    public float JumpStaminaDrain = 0.3f;
    public float InAirStaminaDrain = 0.05f;
    public Vector3 InAirForce = new Vector3(0f, 50f, 20f);
    public float InAirJumpCD = 0.2f;
    public float FloatAuxilaryForce = 9.8f;
    public float ButtStaminaDrain = 0.1f;
    public float ButtAnticipationDuration = 0.4f;
    public float ButtStrikeRaidus;
    public float ButtStrikeStrength;
    public float ButtStrikeStopDistance;
    public LayerMask ButtHitStopLayer;
    public float ButtStrikeDuration;
    public float ButtStrikeForwardPush;
    public float ButtStrikeDistance;
    public float ButtRecoveryDuration;
}
