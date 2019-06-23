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
		_instantiateVFX(VFXDataStore.JumpVFX, pj.PlayerFeet.transform.position, VFXDataStore.JumpVFX.transform.rotation);
	}

	private void _onPlayerLand(PlayerLand pl)
	{
		_instantiateVFX(VFXDataStore.LandVFX, pl.PlayerFeet.transform.position, VFXDataStore.LandVFX.transform.rotation);
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
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.RemoveHandler<ObjectDespawned>(_onObjectDespawned);
		EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
		EventManager.Instance.RemoveHandler<PlayerJump>(_onPlayerJump);
		EventManager.Instance.RemoveHandler<PlayerLand>(_onPlayerLand);
	}

}
