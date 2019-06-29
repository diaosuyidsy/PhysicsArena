using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPickUpData", menuName = "ScriptableObjects/Character/PickUpData", order = 1)]
public class CharacterPickUpData : ScriptableObject
{
	public float Radius = 1f;
	public LayerMask PickUpLayer;
}
