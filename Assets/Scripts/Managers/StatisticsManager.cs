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
	public int[] CartTime;
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
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(SuicideRecord, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostKill
	{
		get
		{
			int maxTimes = KillRecord.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(KillRecord, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostTeammateKill
	{
		get
		{
			int maxTimes = TeammateMurderRecord.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(TeammateMurderRecord, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostPayloadTime
	{
		get
		{
			int maxTimes = CartTime.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(CartTime, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostBlockMaster
	{
		get
		{
			int maxTimes = BlockTimes.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(BlockTimes, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostBlockedDumbass
	{
		get
		{
			int maxTimes = BlockedTimes.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(BlockedTimes, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostFoodDelivery
	{
		get
		{
			int maxTimes = FoodScoreTimes.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(FoodScoreTimes, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostWaterGunMaster
	{
		get
		{
			int maxTimes = WaterGunUseTime.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(WaterGunUseTime, maxTimes), maxTimes);
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
			if (maxTimes == -1f) return null;
			return new StatisticsRecord(Array.IndexOf(HookGunAccuracy, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostPlayerSucked
	{
		get
		{
			int maxTimes = SuckedPlayersTimes.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(SuckedPlayersTimes, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostWeaponPickedUp
	{
		get
		{
			int maxTimes = WeaponPickedTimes.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(WeaponPickedTimes, maxTimes), maxTimes);
		}
	}
	public StatisticsRecord MostDeadTimes
	{
		get
		{
			int maxTimes = DeadTimes.Max();
			if (maxTimes == 0) return null;
			return new StatisticsRecord(Array.IndexOf(DeadTimes, maxTimes), maxTimes);
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
			if (maxTimes == -1f) return null;
			return new StatisticsRecord(Array.IndexOf(MeleeAccuracy, maxTimes), maxTimes);
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
		CartTime = new int[maxPlayers];
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
		HookGunSuccessTimes[hh.HookerPlayerNumber]++;
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
