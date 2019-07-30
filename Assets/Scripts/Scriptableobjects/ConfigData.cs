using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConfigData", menuName = "ScriptableObjects/ConfigData", order = 1)]
public class ConfigData : ScriptableObject
{
	public LayerMask AllPlayerLayer;
	public string[] IndexToName;
	public Color[] IndexToColor;
}
