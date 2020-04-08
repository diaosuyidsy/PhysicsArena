using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaterGunData", menuName = "ScriptableObjects/Weapon/WaterGun", order = 1)]
public class WaterGunData : WeaponDataBase
{
    public float Speed;
    public float BackFireThrust;
    public float WaterForce = 100f;
    public int MaxAmmo = 168;
    public float ShootMaxCD = 0.3f;
    public float HelpAimAngle = 30f;
    public float HelpAimDistance = 30f;
    public float WaterCastRadius = 0.4f;
    public float WaterCastDistance = 5f;
    public float WaterBackCastDistance = 0.3f;
}
