using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;
using Mirror;

public class NetworkStatisticManager
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
    /// Dictionary key: netID
    /// Dictionary value: Records Value
    /// </summary>
    public Dictionary<int, float>[] AllRecords;
    public NetworkStatisticManager()
    {
        _configData = NetworkServices.Config.ConfigData;
        AllRecords = new Dictionary<int, float>[_configData.StatsInfo.Length];
        for (int i = 0; i < AllRecords.Length; i++)
        {
            AllRecords[i] = new Dictionary<int, float>();
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
    /// <returns>A Dictionary that maps netID to a statistic Record</returns>
    public Dictionary<int, StatisticsRecord> GetStatisticResult()
    {
        Dictionary<int, StatisticsRecord> result = new Dictionary<int, StatisticsRecord>();
        StatsTuple[] mostRecords = new StatsTuple[_configData.StatsInfo.Length];
        for (int i = 0; i < mostRecords.Length; i++)
        {
            if (AllRecords[i].Count == 0)
            {
                mostRecords[i] = null;
                continue;
            }
            int maxtimesNetID = AllRecords[i].Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            float maxtimes = AllRecords[i][maxtimesNetID];
            mostRecords[i] = null;
            if (!Mathf.Approximately(maxtimes, 0f)) mostRecords[i] = new StatsTuple(i
            , maxtimesNetID
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
                        record.RawData.ToString("F0") +
                            _configData.StatsInfo[record.Index].StatisticsIntro2,
                            _configData.StatsInfo[record.Index].StatisticIcon
                ));
            }
        }
        GameObject Players = GameObject.Find("Players");
        for (int i = 0; i < Players.transform.childCount; i++)
        {
            int rewiredID = Players.transform.GetChild(i).GetComponent<PlayerControllerMirror>().PlayerNumber;
            if (!result.ContainsKey(rewiredID)) result.Add(rewiredID, new StatisticsRecord(
                 _configData.UselessInfo.StatisticsTitle,
                _configData.UselessInfo.StatisticsIntro1 + _configData.UselessInfo.StatisticsIntro2,
                 _configData.UselessInfo.StatisticIcon
             ));
        }
        return result;
    }

    // Return MVP netID in int
    public uint GetMVPRewiredID()
    {
        /// The Player Rewired ID Weighted Score
        Dictionary<int, float> MVPWeightedScore = new Dictionary<int, float>();
        for (int i = 0; i < _configData.StatsInfo.Length; i++)
        {
            if (!_configData.StatsInfo[i].ExcludeFromMVPCalculation)
            {
                var record = AllRecords[i];
                foreach (var entry in record)
                {
                    if (!MVPWeightedScore.ContainsKey(entry.Key)) MVPWeightedScore.Add(entry.Key, entry.Value);
                    else MVPWeightedScore[entry.Key] += entry.Value * _configData.StatsInfo[i].Weight;
                }
            }
        }
        int MVPNetID = (int)NetworkIdentity.spawned.Keys.First();
        foreach (var kp in NetworkIdentity.spawned)
        {
            if (kp.Value != null && kp.Value.gameObject.GetComponent<PlayerControllerMirror>() != null)
                MVPNetID = (int)kp.Key;
        }
        if (MVPWeightedScore.Count > 0)
            MVPNetID = MVPWeightedScore.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        return (uint)MVPNetID;
    }

    private void _onFistGunBlocked(FistGunBlocked ev)
    {
        if (!AllRecords[4].ContainsKey(ev.BlockerPlayerNumber)) AllRecords[4].Add(ev.BlockerPlayerNumber, 0f);
        if (!AllRecords[5].ContainsKey(ev.FistGunOwnerPlayerNumber)) AllRecords[5].Add(ev.FistGunOwnerPlayerNumber, 0f);
        AllRecords[4][ev.BlockerPlayerNumber]++;
        AllRecords[5][ev.FistGunOwnerPlayerNumber]++;
    }

    private void _onHookGUnBlocked(HookBlocked ev)
    {
        if (!AllRecords[4].ContainsKey(ev.HookBlockerPlayerNumber)) AllRecords[4].Add(ev.HookBlockerPlayerNumber, 0f);
        if (!AllRecords[5].ContainsKey(ev.HookGunOwnerPlayerNumber)) AllRecords[5].Add(ev.HookGunOwnerPlayerNumber, 0f);
        AllRecords[4][ev.HookBlockerPlayerNumber]++;
        AllRecords[5][ev.HookGunOwnerPlayerNumber]++;
    }

    private void _onPlayerDied(PlayerDied pd)
    {
        /// Suicide Record
        if (!pd.HitterIsValid || pd.PlayerHitter == null || pd.PlayerHitter.GetComponent<PlayerControllerMirror>() == null)
        {
            if (!AllRecords[0].ContainsKey(pd.PlayerNumber)) AllRecords[0].Add(pd.PlayerNumber, 0f);
            AllRecords[0][pd.PlayerNumber]++;
            return;
        }

        /// Teammate Murder Record
        if (pd.HitterIsValid && pd.Player.tag == pd.PlayerHitter.tag)
        {
            if (!AllRecords[2].ContainsKey(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber)) AllRecords[2].Add(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber, 0f);
            AllRecords[2][pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber]++;
        }
        /// Kill Record
        if (pd.HitterIsValid && pd.Player.tag != pd.PlayerHitter.tag)
        {
            if (!AllRecords[1].ContainsKey(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber)) AllRecords[1].Add(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber, 0f);
            AllRecords[1][pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber]++;
            /// Kill Type Record
            switch (pd.ImpactType)
            {
                case ImpactType.WaterGun:
                    if (!AllRecords[7].ContainsKey(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber)) AllRecords[7].Add(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber, 0f);
                    AllRecords[7][pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber]++;
                    break;
                case ImpactType.BazookaGun:
                    if (!AllRecords[11].ContainsKey(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber)) AllRecords[11].Add(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber, 0f);
                    AllRecords[11][pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber]++;
                    break;
                case ImpactType.HookGun:
                    if (!AllRecords[8].ContainsKey(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber)) AllRecords[8].Add(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber, 0f);
                    AllRecords[8][pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber]++;
                    break;
                case ImpactType.FistGun:
                    if (!AllRecords[10].ContainsKey(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber)) AllRecords[10].Add(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber, 0f);
                    AllRecords[10][pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber]++;
                    break;
                case ImpactType.Melee:
                    if (!AllRecords[14].ContainsKey(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber)) AllRecords[14].Add(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber, 0f);
                    AllRecords[14][pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber]++;
                    break;
                case ImpactType.SuckGun:
                    if (!AllRecords[9].ContainsKey(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber)) AllRecords[9].Add(pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber, 0f);
                    AllRecords[9][pd.PlayerHitter.GetComponent<PlayerControllerMirror>().PlayerNumber]++;
                    break;
            }
        }
        /// Dead Times Record
        if (!AllRecords[13].ContainsKey(pd.PlayerNumber)) AllRecords[13].Add(pd.PlayerNumber, 0f);
        AllRecords[13][pd.PlayerNumber]++;
    }

    private void _onPlayerHit(PlayerHit ph)
    {
        if (ph.IsABlock)
        {
            if (!AllRecords[4].ContainsKey(ph.HiterPlayerNumber)) AllRecords[4].Add(ph.HiterPlayerNumber, 0f);
            AllRecords[4][ph.HiterPlayerNumber]++;

            if (!AllRecords[5].ContainsKey(ph.HittedPlayerNumber)) AllRecords[5].Add(ph.HittedPlayerNumber, 0f);
            AllRecords[5][ph.HittedPlayerNumber]++;
        }
    }

    private void _onFoodDelievered(FoodDelivered ev)
    {
        if (!AllRecords[6].ContainsKey(ev.DeliverPlayerNumber)) AllRecords[6].Add(ev.DeliverPlayerNumber, 0f);
        AllRecords[6][ev.DeliverPlayerNumber]++;
    }

    private void _onObjectPickedUp(ObjectPickedUp ev)
    {
        if (!ev.Obj.tag.Contains("Resource"))
        {
            if (!AllRecords[12].ContainsKey(ev.PlayerNumber)) AllRecords[12].Add(ev.PlayerNumber, 0f);
            AllRecords[12][ev.PlayerNumber]++;
        }
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