using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CartModeReforgedModeData", menuName = "ScriptableObjects/CartModeReforgedModeData", order = 1)]
public class CartModeReforgedModeData : ModeSepcificData
{
    public int LevelUpExp;
    public int CheckPointExp;
    public int CheckPointExpTimeInterval;
    public int KillExp;
    public int KillExpBonusPerLevelDif;
    public float BaseCartSpeed;
    public float CartSpeedBonusPerLevel;

}
