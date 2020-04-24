using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "ConfigData", menuName = "ScriptableObjects/ConfigData", order = 1)]
public class ConfigData : ScriptableObject
{
    public GameObject[] PlayerPrefabs;
    public Color[] IndexToColor;
    [Tooltip("Team Index 0 is chicken, 1 is duck")]
    public Color[] TeamColor;
    [Header("Statistics Setting")]
    public Sprite[] TeamNumberToMVPBackground;
    public Sprite[] ColorIndexToMVPPlayerPortrait;
    public Vector3 MVPTitleMoveAmount;
    public float MVPTitleMoveDuration = 0.5f;
    public Ease MVPTitleMoveEase = Ease.OutQuad;
    public Vector3 MVPPortraitMoveAmount;
    public float MVPPortraitMoveDuration = 0.5f;
    public Ease MVPPortraitMoveEase = Ease.OutQuad;
    public float MVPPortraitToStatisticDelay = 0.5f;
    public Sprite[] ColorIndexToStatisticPlayerIcon;
    public float[] FrameYPosition;
    public float FrameXPosition;
    public StatisticsInformation[] StatsInfo;
    public StatisticsInformation UselessInfo;

    public float FrameMoveInDuration = 0.2f;
    [Header("Misc")]
    public string FightString = "FIGHT!";
    public string ReadyString = "READY?";
}
