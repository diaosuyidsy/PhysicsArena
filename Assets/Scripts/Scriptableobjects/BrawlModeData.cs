using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BrawlModeData", menuName = "ScriptableObjects/BrawlModeData", order = 1)]
public class BrawlModeData : ScriptableObject
{
    public int TotalTime = 180;
    public int FoodReduceTime = 30;
}
