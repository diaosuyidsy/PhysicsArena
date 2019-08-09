using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BrawlModeData", menuName = "ScriptableObjects/BrawlModeData", order = 1)]
public class BrawlModeData : ScriptableObject
{
    public float TotalTime = 180f;
    public float TimeSpeed = 1f;
    public float FoodReduceTime = 30f;
}
