using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "BoomerangData", menuName = "ScriptableObjects/Weapon/BoomerangData", order = 1)]
public class BoomerangData : WeaponDataBase
{
    [Header("Basic Config")]
    public float BoomerangInitialSpeed = 10f;
    public float BoomerangInitialLeftwardAngle = 45f;
    public Vector3 CircleCenter = Vector3.zero;
    public float BoomerangAngleVelocity = 5f;
    public float BoomerangMaxOutTime = 0.5f;
    public float OnHitForce = 900f;
    public int MaxAmmo = 3;
    public LayerMask CanHitLayer;
    public LayerMask ObstacleLayer;

    [Header("Hit Scan Config")]
    public float ForwardCastAmount = 0.1f;
    public float UpCastAmount = 0.1f;
    public float HitRadius = 0.5f;
    public float HitMaxDistance = 0.1f;
    public float BoomerangReflectionForce = 1f;

    [Header("Auxilary Line Config")]
    public float Step = 0.02f;
    [Range(0.01f, 2f)]
    public float Time = 1f;
    public LayerMask GroundLayer;

}
