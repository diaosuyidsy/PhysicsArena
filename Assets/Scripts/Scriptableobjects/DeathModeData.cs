using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeathModeData", menuName = "ScriptableObjects/DeathModeData", order = 1)]
public class DeathModeData : ModeSepcificData
{
    public int CircleDeathScore = 4;
    public int NormalDeathScore = 1;
    public int WinningScore = 20;
}
