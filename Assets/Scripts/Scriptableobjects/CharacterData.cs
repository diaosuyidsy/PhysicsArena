﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
	public CharacterMovementData CharacterMovementDataStore;
	public CharacterBlockData CharacterBlockDataStore;
	public CharacterMeleeData CharacterMeleeDataStore;
	public CharacterPickUpData CharacterPickUpDataStore;

	public float HelpAimMaxRange = 10f;
	public float HelpAimMaxDegree = 40f;
}
