using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "FistGunData", menuName = "ScriptableObjects/Weapon/FistGun", order = 1)]
public class FistGunData : WeaponDataBase
{
    public float FistSpeed = 12f;
    public int MaxAmmo = 15;
    public float FistHitForce = 900f;
    public float BackfireHitForce = 300f;
    public float IdleBackfireHitForce = 20f;
    public float MaxFlyDistance = 10f;
    public float ReloadTime = 2f;
    public AnimationCurve ReloadEase;
    public float FistReboundY = 1f;
    public float FistReboundForce = 100f;
    public float FistHitScanRadius = 0.3f;
    public float FistHitScanDist = 0.1f;
    public float FistUselessDuration = 1f;
    public float FistOutDuration = 0.2f;
    public LayerMask AllThingFistCanCollideLayer;
    public float HelpAimAngle = 30f;
    public float HelpAimDistance = 30f;
}
