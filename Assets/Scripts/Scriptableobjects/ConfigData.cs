using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConfigData", menuName = "ScriptableObjects/ConfigData", order = 1)]
public class ConfigData : ScriptableObject
{
	public LayerMask AllPlayerLayer;
	public string[] IndexToName;
	public Color[] IndexToColor;
	[Tooltip("Team Index 0 is chicken, 1 is duck")]
	public Color[] TeamColor;
	[Header("Statistics Setting")]
	public StatisticsInformation[] StatsInfo;
	public float StatisticsTitleAnimationDuration = 1f;
	public float StatisticsNomineeAnimationDuration = 0.5f;
	public float StatisticsRecordAnimationDuration = 1f;
	public float StatisStayTime = 3f;
}
