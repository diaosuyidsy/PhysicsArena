using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffectManager : MonoBehaviour
{
	public VFXData VFXDataStore;

	#region Event Handlers
	private void _onPlayerHit(PlayerHit ph)
	{
		Vector3 hittedPos = ph.Hitted.transform.position;
		Vector3 force = ph.Force;
		GameObject par = _instantiateVFX(VFXDataStore.HitVFX, hittedPos, Quaternion.Euler(0f, 180f + Vector3.SignedAngle(Vector3.forward, new Vector3(force.x, 0f, force.z), Vector3.up), 0f));
		//ParticleSystem.MainModule psmain = par.GetComponent<ParticleSystem>().main;
		//ParticleSystem.MainModule psmain2 = par.transform.GetChild(0).GetComponent<ParticleSystem>().main;
		//psmain.maxParticles = (int)Mathf.Round((9f / 51005f) * force.magnitude * force.magnitude);
		//psmain2.maxParticles = (int)Mathf.Round(12f / 255025f * force.magnitude * force.magnitude);
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

	private void _onPunchStart(PunchStart ps)
	{
		GameObject VFX = ps.Player.CompareTag("Team1") ? VFXDataStore.ChickenMeleeChargingVFX : VFXDataStore.DuckMeleeChargingVFX;
		GameObject MeleeVFXHolder = ps.Player.GetComponent<PlayerController>().MeleeVFXHolder;
		if (MeleeVFXHolder != null) Destroy(MeleeVFXHolder);
		ps.Player.GetComponent<PlayerController>().MeleeVFXHolder = Instantiate(VFX, ps.PlayerRightHand, false);

	}

	private void _onPunchHolding(PunchHolding ph)
	{
		GameObject VFX = ph.Player.CompareTag("Team1") ? VFXDataStore.ChickenUltimateVFX : VFXDataStore.DuckUltimateVFX;
		GameObject MeleeVFXHolder = ph.Player.GetComponent<PlayerController>().MeleeVFXHolder;
		if (MeleeVFXHolder != null) Destroy(MeleeVFXHolder);
		ph.Player.GetComponent<PlayerController>().MeleeVFXHolder = Instantiate(VFX, ph.PlayerRightHand, false);
	}

	private void _onPunchReleased(PunchReleased pr)
	{

	}

	private void _onPunchDone(PunchDone pd)
	{
		GameObject MeleeVFXHolder = pd.Player.GetComponent<PlayerController>().MeleeVFXHolder;
		if (MeleeVFXHolder != null) Destroy(MeleeVFXHolder);
	}

	private void _onBazookaBombed(BazookaBombed bb)
	{
		_instantiateVFX(VFXDataStore.BazookaExplosionVFX, bb.BazookaGun.transform.position, VFXDataStore.BazookaExplosionVFX.transform.rotation);
	}
	#endregion

	private GameObject _instantiateVFX(GameObject _vfx, Vector3 _pos, Quaternion _rot)
	{
		if (_vfx == null) return null;
		return Instantiate(_vfx, _pos, _rot);
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
	}

}
