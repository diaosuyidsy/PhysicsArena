using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "ConfigData", menuName = "ScriptableObjects/ConfigData", order = 1)]
public class ConfigData : ScriptableObject
{
    public LayerMask AllPlayerLayer;
    public string[] IndexToName;
    public Color[] IndexToColor;
    [Tooltip("Team Index 0 is chicken, 1 is duck")]
    public Color[] TeamColor;
    [Header("Statistics Setting")]
    public float[] FrameYPosition;
    public StatisticsInformation[] StatsInfo;
    public StatisticsInformation UselessInfo;
    public float StatisticsTitleAnimationDuration = 1f;
    public float StatisticsNomineeAnimationDuration = 0.5f;
    public float StatisticsRecordAnimationDuration = 1f;
    public float StatisStayTime = 3f;
    public float MVPPodiumMoveDuration = 1f;
    public Ease MVPPodiumMoveEase = Ease.OutQuad;
    public float MVPSpotLightDuration = 0.2f;
    public float MVPSpotLightIntensity = 5f;
    public Ease MVPSpotLightEase = Ease.OutQuad;
    public float MVPSpotLightToLandDuration = 0.5f;
    public float MVPLandToWordShowDuration = 0.2f;

    public float MVPToUIMoveInDuration = 0.5f;
    public float MVPScaleDownDuration = 0.5f;
    public float FrameMoveInDuration = 0.2f;
    public GameObject MVPBadgePrefab;
    [Header("Misc")]
    public string FightString = "FIGHT!";
    public string ReadyString = "READY?";
}
