using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CartModeReforgedModeData", menuName = "ScriptableObjects/CartModeReforgedModeData", order = 1)]
public class CartModeReforgedModeData : ModeSepcificData
{
    public float RecoverSpeed;

    public int MaxLevel;
    public float SpeedUpTime;


    public List<float> CartSpeedWithCheckpoint;
    public List<float> OccupySpeedWithCheckpoint;

}
