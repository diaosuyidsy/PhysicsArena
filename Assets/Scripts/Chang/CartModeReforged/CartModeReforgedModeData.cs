using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CartModeReforgedModeData", menuName = "ScriptableObjects/CartModeReforgedModeData", order = 1)]
public class CartModeReforgedModeData : ModeSepcificData
{
    public int KillScore;
    public int WinScore;

    public float CheckpointOccupySpeed;
    public float RecoverSpeed;

    public float BaseCartSpeed;
    public float CartSpeedBonusPerCheckpoint;

}
