using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaterGunData", menuName = "ScriptableObjects/Weapon/WaterGun", order = 1)]
public class WaterGunData : ScriptableObject
{
    public float Speed;
    public float BackFireThrust;
    public float UpThrust = 1f;
    public int MaxAmmo = 168;
    public float ShootMaxCD = 0.3f;
    public float HelpAimAngle = 30f;
    public float HelpAimDistance = 30f;
}
