using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

public class StatisticsManager
{
    private ConfigData _configData;
    public int[] SuicideRecord;
    public int[] KillRecord;
    public int[] TeammateMurderRecord;
    public float[] CartTime;
    public int[] BlockTimes;
    /// Times that this player is blocked, stupid
    public int[] BlockedTimes;
    public int[] FoodScoreTimes;
    public int[] WaterGunUseTime;
    public int[] HookGunUseTimes;
    public int[] HookGunSuccessTimes;
    public int[] SuckedPlayersTimes;
    public int[] WeaponPickedTimes;
    public int[] DeadTimes;
    public int[] MeleeUseTimes;
    public int[] MeleeHitTimes;

    public StatsTuple MostSelfDestruct
    {
        get
        {
            int maxTimes = SuicideRecord.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(0, Array.IndexOf(SuicideRecord, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[0].Weight);
        }
    }
    public StatsTuple MostKill
    {
        get
        {
            int maxTimes = KillRecord.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(1, Array.IndexOf(KillRecord, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[1].Weight);
        }
    }
    public StatsTuple MostTeammateKill
    {
        get
        {
            int maxTimes = TeammateMurderRecord.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(2, Array.IndexOf(TeammateMurderRecord, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[2].Weight);
        }
    }
    public StatsTuple MostPayloadTime
    {
        get
        {
            float maxTimes = CartTime.Max();
            if (Mathf.Approximately(0f, maxTimes)) return null;
            return new StatsTuple(3, Array.IndexOf(CartTime, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[3].Weight);
        }
    }
    public StatsTuple MostBlockMaster
    {
        get
        {
            int maxTimes = BlockTimes.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(4, Array.IndexOf(BlockTimes, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[4].Weight);
        }
    }
    public StatsTuple MostBlockedDumbass
    {
        get
        {
            int maxTimes = BlockedTimes.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(5, Array.IndexOf(BlockedTimes, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[5].Weight);
        }
    }
    public StatsTuple MostFoodDelivery
    {
        get
        {
            int maxTimes = FoodScoreTimes.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(6, Array.IndexOf(FoodScoreTimes, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[6].Weight);
        }
    }
    public StatsTuple MostWaterGunMaster
    {
        get
        {
            int maxTimes = WaterGunUseTime.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(7, Array.IndexOf(WaterGunUseTime, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[7].Weight);
        }
    }
    public StatsTuple MostHookGunAccuracy
    {
        get
        {
            float[] HookGunAccuracy = new float[HookGunSuccessTimes.Length];
            for (int i = 0; i < HookGunAccuracy.Length; i++)
            {
                if (HookGunUseTimes[i] == 0) HookGunAccuracy[i] = -1f;
                else HookGunAccuracy[i] = 1f * HookGunSuccessTimes[i] / HookGunUseTimes[i];
            }
            float maxTimes = HookGunAccuracy.Max();
            if (Mathf.Approximately(0f, maxTimes)) return null;
            return new StatsTuple(8, Array.IndexOf(HookGunAccuracy, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[8].Weight);
        }
    }
    public StatsTuple MostPlayerSucked
    {
        get
        {
            int maxTimes = SuckedPlayersTimes.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(9, Array.IndexOf(SuckedPlayersTimes, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[9].Weight);
        }
    }
    public StatsTuple MostWeaponPickedUp
    {
        get
        {
            int maxTimes = WeaponPickedTimes.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(10, Array.IndexOf(WeaponPickedTimes, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[10].Weight);
        }
    }
    public StatsTuple MostDeadTimes
    {
        get
        {
            int maxTimes = DeadTimes.Max();
            if (maxTimes == 0) return null;
            return new StatsTuple(11, Array.IndexOf(DeadTimes, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[11].Weight);
        }
    }
    public StatsTuple MostMeleeHitAccuracy
    {
        get
        {
            float[] MeleeAccuracy = new float[MeleeHitTimes.Length];
            for (int i = 0; i < MeleeAccuracy.Length; i++)
            {
                if (MeleeUseTimes[i] == 0) MeleeAccuracy[i] = -1f;
                else MeleeAccuracy[i] = 1f * MeleeHitTimes[i] / MeleeUseTimes[i];
            }
            float maxTimes = MeleeAccuracy.Max();
            if (Mathf.Approximately(0f, maxTimes)) return null;
            return new StatsTuple(12, Array.IndexOf(MeleeAccuracy, maxTimes), maxTimes, maxTimes * _configData.StatsInfo[12].Weight);
        }
    }
    public StatsTuple[] FoodCartRecords
    {
        get
        {
            StatsTuple[] result = new StatsTuple[13];
            result[0] = MostSelfDestruct;
            result[1] = MostKill;
            result[2] = MostTeammateKill;
            result[3] = MostPayloadTime;
            result[4] = MostBlockMaster;
            result[5] = MostBlockedDumbass;
            result[6] = MostFoodDelivery;
            result[7] = MostWaterGunMaster;
            result[8] = MostHookGunAccuracy;
            result[9] = MostPlayerSucked;
            result[10] = MostWeaponPickedUp;
            result[11] = MostDeadTimes;
            result[12] = MostMeleeHitAccuracy;
            return result;
        }
    }

    public StatisticsManager()
    {
        _configData = Services.Config.ConfigData;
        int maxPlayers = ReInput.players.playerCount;
        SuicideRecord = new int[maxPlayers];
        KillRecord = new int[maxPlayers];
        TeammateMurderRecord = new int[maxPlayers];
        BlockTimes = new int[maxPlayers];
        FoodScoreTimes = new int[maxPlayers];
        HookGunUseTimes = new int[maxPlayers];
        HookGunSuccessTimes = new int[maxPlayers];
        SuckedPlayersTimes = new int[maxPlayers];
        BlockedTimes = new int[maxPlayers];
        WeaponPickedTimes = new int[maxPlayers];
        CartTime = new float[maxPlayers];
        WaterGunUseTime = new int[maxPlayers];
        DeadTimes = new int[maxPlayers];
        MeleeUseTimes = new int[maxPlayers];
        MeleeHitTimes = new int[maxPlayers];
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelievered);
        EventManager.Instance.AddHandler<HookGunFired>(_onHookgunFired);
        EventManager.Instance.AddHandler<HookHit>(_onHookHit);
        EventManager.Instance.AddHandler<SuckGunSuck>(_onPlayerSucked);
        EventManager.Instance.AddHandler<WaterGunFired>(_onWaterGunFired);
        EventManager.Instance.AddHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.AddHandler<PunchReleased>(_onMelee);
    }

    /// <summary>
    /// Returns the Statistic Result of the Game
    /// </summary>
    /// <returns>A Dictionary that maps rewired Id to a statistic Record</returns>
    public Dictionary<int, StatisticsRecord> GetStatisticResult()
    {
        Dictionary<int, StatisticsRecord> result = new Dictionary<int, StatisticsRecord>();
        StatsTuple[] foodcartrecord = FoodCartRecords;
        Utility.SelectionSortStatisticRecord(ref foodcartrecord);
        HashSet<int> rewiredIDSet = new HashSet<int>();
        for (int i = foodcartrecord.Length - 1; i >= 0; i--)
        {
            StatsTuple record = foodcartrecord[i];
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
        return result;
    }

    private void _onPlayerDied(PlayerDied pd)
    {
        /// Suicide Record
        if (!pd.HitterIsValid || pd.PlayerHitter == null) SuicideRecord[pd.PlayerNumber]++;
        /// Kill Record
        if (pd.HitterIsValid && pd.Player.tag != pd.PlayerHitter.tag) KillRecord[pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
        /// Teammate Murder Record
        if (pd.HitterIsValid && pd.Player.tag == pd.PlayerHitter.tag) TeammateMurderRecord[pd.PlayerHitter.GetComponent<PlayerController>().PlayerNumber]++;
        DeadTimes[pd.PlayerNumber]++;
    }

    private void _onMelee(PunchReleased pr)
    {
        MeleeUseTimes[pr.PlayerNumber]++;
    }

    private void _onPlayerHit(PlayerHit ph)
    {
        if (ph.IsABlock)
        {
            BlockTimes[ph.HiterPlayerNumber]++;
            BlockedTimes[ph.HittedPlayerNumber]++;
        }
        else
        {
            MeleeHitTimes[ph.HiterPlayerNumber]++;
        }
    }

    private void _onFoodDelievered(FoodDelivered fd)
    {
        FoodScoreTimes[fd.DeliverPlayerNumber]++;
    }

    private void _onHookgunFired(HookGunFired h)
    {
        HookGunUseTimes[h.HookGunOwnerPlayerNumber]++;
    }

    private void _onHookHit(HookHit hh)
    {
        HookGunSuccessTimes[hh.HookGunOwnerPlayerNumber]++;
    }

    private void _onPlayerSucked(SuckGunSuck s)
    {
        SuckedPlayersTimes[s.SuckGunOwnerPlayerNumber] += s.SuckedPlayers.Count;
    }

    private void _onWaterGunFired(WaterGunFired w)
    {
        WaterGunUseTime[w.WaterGunOwnerPlayerNumber]++;
    }

    private void _onObjectPickedUp(ObjectPickedUp op)
    {
        if (!op.Obj.tag.Contains("Resource")) WeaponPickedTimes[op.PlayerNumber]++;
    }

    public void Destory()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelievered);
        EventManager.Instance.RemoveHandler<HookGunFired>(_onHookgunFired);
        EventManager.Instance.RemoveHandler<HookHit>(_onHookHit);
        EventManager.Instance.RemoveHandler<SuckGunSuck>(_onPlayerSucked);
        EventManager.Instance.RemoveHandler<WaterGunFired>(_onWaterGunFired);
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.RemoveHandler<PunchReleased>(_onMelee);

    }
}