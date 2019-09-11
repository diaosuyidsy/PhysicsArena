﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "BoomerangData", menuName = "ScriptableObjects/Weapon/BoomerangData", order = 1)]
public class BoomerangData : WeaponDataBase
{
    public float BoomerangSpeed = 9f;
    public float OnHitForce = 900f;
    public float CurveMultiplier = 1f;
    public Vector3[] LocalMovePoints;
    public AnimationCurve BoomEase;
    public int MaxAmmo = 1;
    public LayerMask CanHitLayer;
}
