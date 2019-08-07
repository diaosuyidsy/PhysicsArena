using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

public class StatisticsManager
{
    private ConfigData _configData;
    /// <summary>
    /// [0Suicide][0][1][2][3][4][5]
    /// [1Kill]
    /// [2Teammate]
    /// [3Cart]
    /// [4Block]
    /// [5Blocked]
    /// [6FoodScore]
    /// [7Water Gun Kill]
    /// [8Hook Gun Kill]
    /// [9Suck Gun Kill]
    /// [10Fist Gun Kill]
    /// [11Bazooka Gun Kill]
    /// [12Weapon Picked Times]
    /// [13Dead Times]
    /// [14Melee Kill]
    /// </summary>
    public float[][] AllRecords;
    public StatisticsManager()
    {
        _configData = Services.Config.ConfigData;
        int maxPlayers = ReInput.players.playerCount;
        AllRecords = new float[_configData.StatsInfo.Length][];
        for (int i = 0; i < AllRecords.Length; i++)
        {
            AllRecords[i] = new float[maxPlayers];
        }
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelievered);
        EventManager.Instance.AddHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.AddHandler<HookBlocked>(_onHookGUnBlocked);
        EventManager.Instance.AddHandler<FistGunBlocked>(_onFistGunBlocked);
    }

    /// <summary>
    /// Returns the Statistic Result of the Game
    /// </summary>
    /// <returns>A Dictionary that maps rewired Id to a statistic Record</returns>
    public Dictionary<int, StatisticsRecord> GetStatisticResult()
    {
        Dictionary<int, StatisticsRecord> result = new Dictionary<int, StatisticsRecord>();
        StatsTuple[] mostRecords = new StatsTuple[_configData.StatsInfo.Length];
        for (int i = 0; i < mostRecords.Length; i++)
        {
            float maxtimes = AllRecords[i].Max();
            mostRecords[i] = null;
            if (!Mathf.Approximately(maxtimes, 0f)) mostRecords[i] = new StatsTuple(i
            , Array.IndexOf(AllRecords[i], maxtimes)
            , maxtimes
            , maxtimes * _configData.StatsInfo[i].Weight);
        }
        Utility.SelectionSortStatisticRecord(ref mostRecords);
        HashSet<int> rewiredIDSet = new HashSet<int>();
        for (int i = mostRecords.Length - 1; i >= 0; i--)
        {
            StatsTuple record = mostRecords[i];
            if (record == null) continue;
            if (rewiredIDSet.Contains(record.RewiredID)) continue;
            else
            {
                rewiredIDSet.Add(record.RewiredID);
                result.Add(record.RewiredID, new StatisticsRecord(
                    _configData.StatsInfo[record.Index].StatisticsTitle,
                    _configData.StatsInfo[record.Index].StatisticsIntro1 +
                        record.RawData +
                            _configData.StatsInfo[record.Index].StatisticsIntro2,
                            _configData.StatsInfo[record.Index].StatisticIcon
                ));
            }
        }
        for (int i = 0; i < Services.GameStateManager.PlayersInformation.RewiredID.Length; i++)
        {
            int rewiredID = Services.GameStateManager.PlayersInformation.RewiredID[i];
            if (!result.ContainsKey(rewiredID)) result.Add(rewiredID, new StatisticsRecord(
                 _configData.UselessInfo.StatisticsTitle,
                _configData.UselessInfo.StatisticsIntro1 + _configData.UselessInfo.StatisticsIntro2,
                 _configData.UselessInfo.StatisticIcon
             ));
        }
        return result;
    }

    public int GetMVPRewiredID()
    {
        /// The Player Rewired ID Weighted Score
        float[] MVPWeightedScore = new float[ReInput.players.playerCount];
        for (int i = 0; i < MVPWeightedScore.Length; i++)
        {
            for (int j = 0; j < _configData.StatsInfo.Length; j++)
            {
                if (!_configData.StatsInfo[j].ExcludeFromMVPCalculation)
                    MVPWeightedScore[i] += AllRecords[j][i] * _configData.StatsInfo[j].Weight;
            }
        }
        float max = MVPWeightedScore.Max();

        return Array.IndexOf(MVPWeightedScore, max);
    }

    private void _onFistGunBlocked(FistGunBlocked ev)
    {
        AllRecords[4][ev.BlockerPlayerNumber]++;
        AllRecords[5][ev.FistGunOwnerPlayerNumber]++;
    }

    private void _onHookGUnBlocked(HookBlocked ev)
    {
        AllRecords[4][ev.HookBlockerPlayerNumber]++;
        AllRecords[5][ev.HookGunOwnerPlayerNumber]++;
    }

    private void _onPlayerDied(PlayerDied pd)
    {
        /// Suicide Record
        if (!pd.HitterIsValid || pd.PlayerHitter == null)
        {
            AllRecords[0][pd.PlayerNumber]++;
            return;
        }

        /// Teammate Murder Record
        if (pd.HitterIsValid && pd.Player.tag == pd.PlayerHitter.tag) AllRecords[2][pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
        /// Kill Record
        if (pd.HitterIsValid && pd.Player.tag != pd.PlayerHitter.tag)
        {
            AllRecords[1][pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
            /// Kill Type Record
            switch (pd.ImpactType)
            {
                case ImpactType.WaterGun:
                    AllRecords[7][pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
                    break;
                case ImpactType.BazookaGun:
                    AllRecords[11][pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
                    break;
                case ImpactType.HookGun:
                    AllRecords[8][pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
                    break;
                case ImpactType.FistGun:
                    AllRecords[10][pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
                    break;
                case ImpactType.Melee:
                    AllRecords[14][pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
                    break;
                case ImpactType.SuckGun:
                    AllRecords[9][pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
                    break;
            }
        }
        /// Dead Times Record
        AllRecords[13][pd.PlayerNumber]++;
    }

    private void _onPlayerHit(PlayerHit ph)
    {
        if (ph.IsABlock)
        {
            AllRecords[4][ph.HiterPlayerNumber]++;
            AllRecords[5][ph.HittedPlayerNumber]++;
        }
    }

    private void _onFoodDelievered(FoodDelivered fd)
    {
        AllRecords[6][fd.DeliverPlayerNumber]++;
    }

    private void _onObjectPickedUp(ObjectPickedUp op)
    {
        if (!op.Obj.tag.Contains("Resource")) AllRecords[12][op.PlayerNumber]++;
    }

    public void Destory()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelievered);
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.RemoveHandler<HookBlocked>(_onHookGUnBlocked);
        EventManager.Instance.RemoveHandler<FistGunBlocked>(_onFistGunBlocked);
    }
}