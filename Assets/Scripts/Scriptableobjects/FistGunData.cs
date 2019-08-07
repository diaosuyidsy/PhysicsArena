using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FistGunData", menuName = "ScriptableObjects/Weapon/FistGun", order = 1)]
public class FistGunData : ScriptableObject
{
    public float FistSpeed = 12f;
    public int MaxAmmo = 15;
    public float FistHitForce = 900f;
    public float BackfireHitForce = 300f;
    public float MaxFlyDistance = 10f;
    public float ReloadTime = 2f;
    public float FistReboundY = 1f;
    public float FistReboundForce = 100f;
    public float FistHitScanRadius = 0.3f;
    public float FistHitScanDist = 0.1f;
    public LayerMask AllThingFistCanCollideLayer;
}
