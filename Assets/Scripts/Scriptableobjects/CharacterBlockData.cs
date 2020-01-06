using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBlockData", menuName = "ScriptableObjects/Character/BlockData", order = 1)]
public class CharacterBlockData : ScriptableObject
{
    public float MaxBlockCD = 1.5f;
    public float BlockRegenInterval = 3f;
    public float BlockRegenRate = 1f;

    public float ArmTargetPosition = 100f;
    public float HandTargetPosition = 120f;

    public float BlockMultiplier = 2f;
    public float BlockAngle = 90f;
    public bool IsSideStepping = false;
    public float SideSteppingInitForce = 300f;
    public float SideSteppingDuration = 0.2f;
    public float SideSteppingCD = 0.5f;
}
