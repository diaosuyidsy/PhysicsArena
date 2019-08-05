using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;
using System.Linq;

public class StatisticsManager
{
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

    public StatisticsRecord MostSelfDestruct
    {
        get
        {
            int maxTimes = SuicideRecord.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[0].ShowLimit) return null;
            return new StatisticsRecord(0, Array.IndexOf(SuicideRecord, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[0].ExtraLimit);
        }
    }
    public StatisticsRecord MostKill
    {
        get
        {
            int maxTimes = KillRecord.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[1].ShowLimit) return null;
            return new StatisticsRecord(1, Array.IndexOf(KillRecord, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[1].ExtraLimit);
        }
    }
    public StatisticsRecord MostTeammateKill
    {
        get
        {
            int maxTimes = TeammateMurderRecord.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[2].ShowLimit) return null;
            return new StatisticsRecord(2, Array.IndexOf(TeammateMurderRecord, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[2].ExtraLimit);
        }
    }
    public StatisticsRecord MostPayloadTime
    {
        get
        {
            float maxTimes = CartTime.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[3].ShowLimit) return null;
            return new StatisticsRecord(3, Array.IndexOf(CartTime, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[3].ExtraLimit);
        }
    }
    public StatisticsRecord MostBlockMaster
    {
        get
        {
            int maxTimes = BlockTimes.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[4].ShowLimit) return null;
            return new StatisticsRecord(4, Array.IndexOf(BlockTimes, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[4].ExtraLimit);
        }
    }
    public StatisticsRecord MostBlockedDumbass
    {
        get
        {
            int maxTimes = BlockedTimes.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[5].ShowLimit) return null;
            return new StatisticsRecord(5, Array.IndexOf(BlockedTimes, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[5].ExtraLimit);
        }
    }
    public StatisticsRecord MostFoodDelivery
    {
        get
        {
            int maxTimes = FoodScoreTimes.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[6].ShowLimit) return null;
            return new StatisticsRecord(6, Array.IndexOf(FoodScoreTimes, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[6].ExtraLimit);
        }
    }
    public StatisticsRecord MostWaterGunMaster
    {
        get
        {
            int maxTimes = WaterGunUseTime.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[7].ShowLimit) return null;
            return new StatisticsRecord(7, Array.IndexOf(WaterGunUseTime, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[7].ExtraLimit);
        }
    }
    public StatisticsRecord MostHookGunAccuracy
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
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[8].ShowLimit) return null;
            return new StatisticsRecord(8, Array.IndexOf(HookGunAccuracy, maxTimes), maxTimes * 100f, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[8].ExtraLimit);
        }
    }
    public StatisticsRecord MostPlayerSucked
    {
        get
        {
            int maxTimes = SuckedPlayersTimes.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[9].ShowLimit) return null;
            return new StatisticsRecord(9, Array.IndexOf(SuckedPlayersTimes, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[9].ExtraLimit);
        }
    }
    public StatisticsRecord MostWeaponPickedUp
    {
        get
        {
            int maxTimes = WeaponPickedTimes.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[10].ShowLimit) return null;
            return new StatisticsRecord(10, Array.IndexOf(WeaponPickedTimes, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[10].ExtraLimit);
        }
    }
    public StatisticsRecord MostDeadTimes
    {
        get
        {
            int maxTimes = DeadTimes.Max();
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[11].ShowLimit) return null;
            return new StatisticsRecord(11, Array.IndexOf(DeadTimes, maxTimes), maxTimes, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[11].ExtraLimit);
        }
    }
    public StatisticsRecord MostMeleeHitAccuracy
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
            if (1f * maxTimes <= Services.Config.ConfigData.StatsInfo[12].ShowLimit) return null;
            return new StatisticsRecord(12, Array.IndexOf(MeleeAccuracy, maxTimes), maxTimes * 100f, 1f * maxTimes > Services.Config.ConfigData.StatsInfo[12].ExtraLimit);
        }
    }

    public StatisticsRecord[] FoodCartRecords
    {
        get
        {
            StatisticsRecord[] result = new StatisticsRecord[13];
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
