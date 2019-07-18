using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public float[] WaterGunUseTime;
	public int[] HookGunUseTimes;
	public int[] HookGunSuccessTimes;
	public int[] SuckedPlayersTimes;
	public int[] WeaponPickedTimes;
	public int[] DeadTimes;
	public int[] MeleeUseTimes;
	public int[] MeleeHitTimes;

	public StatisticsManager()
	{
		SuicideRecord = new int[6];
		KillRecord = new int[6];
		TeammateMurderRecord = new int[6];
		BlockTimes = new int[6];
		FoodScoreTimes = new int[6];
		HookGunUseTimes = new int[6];
		HookGunSuccessTimes = new int[6];
		SuckedPlayersTimes = new int[6];
		BlockedTimes = new int[6];
		WeaponPickedTimes = new int[6];
		CartTime = new float[6];
		WaterGunUseTime = new float[6];
		DeadTimes = new int[6];
		MeleeUseTimes = new int[6];
		MeleeHitTimes = new int[6];
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
