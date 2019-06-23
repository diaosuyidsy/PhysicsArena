using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SuckGunData", menuName = "ScriptableObjects/Weapon/SuckGun", order = 1)]
public class SuckGunData : ScriptableObject
{
	public float MaxBallTravelTime = 4f;
	public float BallTravelSpeed = 3f;
	public float SuckStrength = 350f;
	public int SuckGunMaxUseTimes = 15;
}
