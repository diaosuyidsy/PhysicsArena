using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BazookaData", menuName = "ScriptableObjects/Weapon/Bazooka", order = 1)]
public class BazookaData : ScriptableObject
{
	public float MarkMoveSpeed = 10f;
	public float MarkThrowThurst = 10f;
	public float MarkGravityScale = 1f;
	public LayerMask LineCastLayer;
}
