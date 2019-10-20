using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RuleSetData", menuName = "Birfia/RuleSetData", order = 0)]
public class RuleSetData : ScriptableObject
{
    public RuleType MyRule;
    public float Gravity;
    public float PunchForce;
}
