using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BrawlModeReforgedUIData", menuName = "ScriptableObjects/BrawlModeReforgedUIData", order = 1)]
public class BrawlModeReforgedUIData : UIData
{
    public float ScoreTextHopStartAlpha;
    public float ScoreTextHopEndAlpha;
    public float ScoreTextHopStartScale;
    public float ScoreTextHopNormalScale;
    public float ScoreTextHopSmallScale;
    public float ScoreBoardHopNormalScale;
    public float ScoreBoardHopBigScale;

    public float ScoreHopFirstPhaseTime;
    public float ScoreHopSecondPhaseTime;
    public float ScoreHopBoardHopBeginTime;

    public float ScorePlusTextHopStartScale;
    public float ScorePlusTextHopBigScale;
    public float ScorePlusTextHopEndScale;
    public float ScorePlusTextHopScaleUpTime;
    public float ScorePlusTextHopScaleDownTime;
    public float ScorePlusTextHopStayTime;

    public Vector3 ScorePlusTextCharacterOffset;
    public float ScorePlusTextCharacterScale;
    public Vector3 ScorePlusTextBasketOffset;
    public float ScorePlusTextBasketScale;
    public GameObject ScorePlusTextPrefab;

    public float ShakeRadius;
    public float ShakeSpeed;
}
