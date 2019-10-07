using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "BoomerangData", menuName = "ScriptableObjects/Weapon/BoomerangData", order = 1)]
public class BoomerangData : WeaponDataBase
{
    public float BoomerangSpeed = 9f;
    public Vector3 BoomerangVelocity = new Vector3(2f, 0f, 2f);
    public Vector3 BoomerangAmplitude = new Vector3(2f, 0f, 2f);
    public float StartAffectiveDuration = 0.1f;
    public float EndAffectiveDuration = 1f;
    public float OnHitForce = 900f;
    public float CurveMultiplier = 1f;
    public Vector3[] LocalMovePoints;
    public float BoomerangYOffset;
    public AnimationCurve BoomEase;
    public int MaxAmmo = 1;
    public LayerMask CanHitLayer;
    public LayerMask ObstacleLayer;
    [Header("Hit Scan Config")]
    public float ForwardCastAmount = 0.1f;
    public float UpCastAmount = 0.1f;
    public float HitRadius = 0.5f;
    public float HitMaxDistance = 0.1f;
    public float BoomerangReflectionForce = 100f;
}
