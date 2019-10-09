﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SushiModeData", menuName = "ScriptableObjects/SushiModeData", order = 1)]
public class SushiModeData : ModeSepcificData
{
    public int PlayerMaxHoldingEggs = 3;
    public int PlayerMaxNeedEggs = 9;
}
