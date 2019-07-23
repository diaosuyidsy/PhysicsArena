using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager
{
	private VFXData VFXDataStore;

	public VFXManager(VFXData _vfxdata)
	{
		VFXDataStore = _vfxdata;
		OnEnable();
	}

	#region Event Handlers
	private void _onPlayerHit(PlayerHit ph)
	{
		Vector3 hittedPos = ph.Hitted.transform.position;
		Vector3 force = ph.Force;
		GameObject par = _instantiateVFX(VFXDataStore.HitVFX, hittedPos, Quaternion.Euler(0f, 180f + Vector3.SignedAngle(Vector3.forward, new Vector3(force.x, 0f, force.z), Vector3.up), 0f));
	}

	private void _onPlayerDied(PlayerDied pd)
	{
		_instantiateVFX(VFXDataStore.DeathVFX, pd.Player.transform.position, VFXDataStore.DeathVFX.transform.rotation);
	}

	private void _onObjectDespawned(ObjectDespawned od)
	{
		_instantiateVFX(VFXDataStore.VanishVFX, od.Obj.transform.position, VFXDataStore.VanishVFX.transform.rotation);
	}

	private void _onFoodDelivered(FoodDelivered fd)
	{
		_instantiateVFX(VFXDataStore.DeliverFoodVFX, fd.Food.transform.position, VFXDataStore.DeliverFoodVFX.transform.rotation);
	}

	private void _onPlayerJump(PlayerJump pj)
	{
		var VFX = VFXDataStore.JumpGrassVFX;
		switch (pj.GroundTag)
		{
			case "Ground_Concrete":
				VFX = VFXDataStore.JumpConcreteVFX;
				break;
			case "Ground_YellowStone":
				VFX = VFXDataStore.JumpYellowStoneVFX;
				break;
		}
		_instantiateVFX(VFX, pj.PlayerFeet.transform.position, VFX.transform.rotation);
	}

	private void _onPlayerLand(PlayerLand pl)
	{
		var VFX = VFXDataStore.LandGrassVFX;
		switch (pl.GroundTag)
		{
			case "Ground_Concrete":
				VFX = VFXDataStore.LandConcreteVFX;
				break;
			case "Ground_YellowStone":
				VFX = VFXDataStore.LandYellowStoneVFX;
				break;
		}
		_instantiateVFX(VFX, pl.PlayerFeet.transform.position, VFX.transform.rotation);
	}

	private void _onBlockStart(BlockStart bs)
	{
		GameObject VFX = bs.Player.CompareTag("Team1") ? VFXDataStore.ChickenBlockVFX : VFXDataStore.DuckBlockVFX;
		GameObject BlockVFXHolder = bs.Player.GetComponent<PlayerController>().BlockVFXHolder;
		if (BlockVFXHolder == null)
		{
			bs.Player.GetComponent<PlayerController>().BlockVFXHolder = GameObject.Instantiate(VFX, bs.Player.transform);
		}
		bs.Player.GetComponent<PlayerController>().BlockVFXHolder.SetActive(true);
	}

	private void _onBlockEnd(BlockEnd be)
	{
		be.Player.GetComponent<PlayerController>().BlockVFXHolder.SetActive(false);
	}

	private void _onPunchStart(PunchStart ps)
	{
		GameObject VFX = ps.Player.CompareTag("Team1") ? VFXDataStore.ChickenMeleeChargingVFX : VFXDataStore.DuckMeleeChargingVFX;
		GameObject MeleeVFXHolder = ps.Player.GetComponent<PlayerController>().MeleeVFXHolder;
		if (MeleeVFXHolder != null) GameObject.Destroy(MeleeVFXHolder);
		ps.Player.GetComponent<PlayerController>().MeleeVFXHolder = GameObject.Instantiate(VFX, ps.PlayerRightHand, false);

	}

	private void _onPunchHolding(PunchHolding ph)
	{
		GameObject VFX = ph.Player.CompareTag("Team1") ? VFXDataStore.ChickenUltimateVFX : VFXDataStore.DuckUltimateVFX;
		GameObject MeleeVFXHolder = ph.Player.GetComponent<PlayerController>().MeleeVFXHolder;
		if (MeleeVFXHolder != null) GameObject.Destroy(MeleeVFXHolder);
		ph.Player.GetComponent<PlayerController>().MeleeVFXHolder = GameObject.Instantiate(VFX, ph.PlayerRightHand, false);
	}

	private void _onPunchReleased(PunchReleased pr)
	{

	}

	private void _onPunchDone(PunchDone pd)
	{
		GameObject MeleeVFXHolder = pd.Player.GetComponent<PlayerController>().MeleeVFXHolder;
		if (MeleeVFXHolder != null) GameObject.Destroy(MeleeVFXHolder);
	}

	private void _onBazookaBombed(BazookaBombed bb)
	{
		_instantiateVFX(VFXDataStore.BazookaExplosionVFX, bb.BazookaGun.transform.position, VFXDataStore.BazookaExplosionVFX.transform.rotation);
	}

	private void _onPlayerStunned(PlayerStunned ps)
	{
		if (ps.Player.GetComponent<PlayerController>().StunVFXHolder == null)
			ps.Player.GetComponent<PlayerController>().StunVFXHolder = GameObject.Instantiate(VFXDataStore.StunnedVFX, ps.PlayerHead, false);
		ps.Player.GetComponent<PlayerController>().StunVFXHolder.SetActive(true);
	}

	private void _onPlayerUnStunned(PlayerUnStunned ps)
	{
		ps.Player.GetComponent<PlayerController>().StunVFXHolder.SetActive(false);
	}

	private void _onPlayerSlowed(PlayerSlowed ps)
	{
		if (ps.Player.GetComponent<PlayerController>().SlowVFXHolder == null)
		{
			ps.Player.GetComponent<PlayerController>().SlowVFXHolder = GameObject.Instantiate(VFXDataStore.SlowedVFX, ps.PlayerFeet.transform, false);
			ps.Player.GetComponent<PlayerController>().SlowVFXHolder.transform.rotation = VFXDataStore.SlowedVFX.transform.rotation;
		}
		ps.Player.GetComponent<PlayerController>().SlowVFXHolder.SetActive(true);
	}

	private void _onPlayerUnslowed(PlayerUnslowed pu)
	{
		pu.Player.GetComponent<PlayerController>().SlowVFXHolder.SetActive(false);
	}
	#endregion

	private GameObject _instantiateVFX(GameObject _vfx, Vector3 _pos, Quaternion _rot)
	{
		if (_vfx == null) return null;
		return GameObject.Instantiate(_vfx, _pos, _rot);
	}

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.AddHandler<ObjectDespawned>(_onObjectDespawned);
		EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelivered);
		EventManager.Instance.AddHandler<PlayerJump>(_onPlayerJump);
		EventManager.Instance.AddHandler<PlayerLand>(_onPlayerLand);
		EventManager.Instance.AddHandler<PunchStart>(_onPunchStart);
		EventManager.Instance.AddHandler<PunchHolding>(_onPunchHolding);
		EventManager.Instance.AddHandler<PunchReleased>(_onPunchReleased);
		EventManager.Instance.AddHandler<PunchDone>(_onPunchDone);
		EventManager.Instance.AddHandler<BazookaBombed>(_onBazookaBombed);
		EventManager.Instance.AddHandler<BlockStart>(_onBlockStart);
		EventManager.Instance.AddHandler<BlockEnd>(_onBlockEnd);
		EventManager.Instance.AddHandler<PlayerStunned>(_onPlayerStunned);
		EventManager.Instance.AddHandler<PlayerUnStunned>(_onPlayerUnStunned);
		EventManager.Instance.AddHandler<PlayerSlowed>(_onPlayerSlowed);
		EventManager.Instance.AddHandler<PlayerUnslowed>(_onPlayerUnslowed);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.RemoveHandler<ObjectDespawned>(_onObjectDespawned);
		EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
		EventManager.Instance.RemoveHandler<PlayerJump>(_onPlayerJump);
		EventManager.Instance.RemoveHandler<PlayerLand>(_onPlayerLand);
		EventManager.Instance.RemoveHandler<PunchStart>(_onPunchStart);
		EventManager.Instance.RemoveHandler<PunchHolding>(_onPunchHolding);
		EventManager.Instance.RemoveHandler<PunchReleased>(_onPunchReleased);
		EventManager.Instance.RemoveHandler<PunchDone>(_onPunchDone);
		EventManager.Instance.RemoveHandler<BazookaBombed>(_onBazookaBombed);
		EventManager.Instance.RemoveHandler<BlockStart>(_onBlockStart);
		EventManager.Instance.RemoveHandler<BlockEnd>(_onBlockEnd);
		EventManager.Instance.RemoveHandler<PlayerStunned>(_onPlayerStunned);
		EventManager.Instance.RemoveHandler<PlayerUnStunned>(_onPlayerUnStunned);
		EventManager.Instance.RemoveHandler<PlayerSlowed>(_onPlayerSlowed);
		EventManager.Instance.RemoveHandler<PlayerUnslowed>(_onPlayerUnslowed);
	}

	public void Destory()
	{
		OnDisable();
	}
}
