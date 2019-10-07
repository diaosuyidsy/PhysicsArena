using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SushiModeObjectiveManager : ObjectiveManager
{
    private SushiModeData _sushiModeData;

    public SushiModeObjectiveManager(SushiModeData _smd) : base()
    {
        _sushiModeData = _smd;
    }

    public void ChangeRespawnPoint(int coloindex, Vector3 Pos, string tag)
    {
        if (tag.Equals("Team1"))
        {
            Services.Config.Team1RespawnPoints[coloindex - 3] = Pos;
        }
        else
        {
            Services.Config.Team2RespawnPoints[coloindex] = Pos;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="winner">0 is draw, 1 is team1, 2 is team2</param>
    public void OnWin(int winner, Transform winnedTransform)
    {
        EventManager.Instance.TriggerEvent(new GameEnd(winner, winnedTransform, GameWinType.RaceWin));
    }

}
