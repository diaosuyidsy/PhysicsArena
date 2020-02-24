using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponDataBase : ScriptableObject
{
    public float XRotation;
    public float YRotation;
    public float ZRotation;
    public float YOffset;
    public float ZOffset;
    public float XOffset;
    public float PickupSlowMultiplier = 1f;
    public float PickupCD = 0.8f;
}
