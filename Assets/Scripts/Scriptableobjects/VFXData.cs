using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXData", menuName = "ScriptableObjects/VFXData", order = 1)]
public class VFXData : ScriptableObject
{
	public GameObject DeliverFoodVFX;
	public GameObject DeathVFX;
	public GameObject VanishVFX;
	public GameObject HitVFX;
	public GameObject JumpVFX;
	public GameObject LandVFX;
}
