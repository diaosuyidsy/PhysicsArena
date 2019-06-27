using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarModeData", menuName = "ScriptableObjects/CarModeData", order = 1)]
public class CarModeData : ScriptableObject
{
	public float CarInitalSpeed = 0.5f;
	public float CarMaxSpeed = 1.5f;
	[Tooltip("How much speed increased per second")]
	public float CarSpeedIncreaseRate = 0.01f;
	public float RotationSpeed = 2f;
	public float MaxAnglePerSecond = 10f;
}
