using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponDataBase : ScriptableObject
{
    public float YFloatDistance = 0.5f;
    public float RotationSpeed = 90f;
    public Vector3 RotatingRotation;
    public float XRotation;
    public float YRotation;
    public float ZRotation;
    public float YOffset;
    public float ZOffset;
    public float XOffset;
    public float PickupSlowMultiplier = 1f;
    public LayerMask Ground;
    public LayerMask OnNoAmmoDropDisappear;
    public LayerMask OnHitDisappear;
    public Vector3 DropForce = new Vector3(0f, 2f, -2f);
}
