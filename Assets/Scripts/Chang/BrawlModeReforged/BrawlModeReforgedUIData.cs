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
}
