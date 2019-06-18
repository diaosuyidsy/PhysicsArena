using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffectManager : MonoBehaviour
{
	public static VisualEffectManager VEM;

	[Header("All Kinds of Visual Effects Holder")]
	public GameObject DeliverFoodVFX;
	public GameObject DeathVFX;
	public GameObject VanishVFX;
	public GameObject HitVFX;

	private void Awake()
	{
		VEM = this;
	}

	private void _onPlayerHit(PlayerHit ph)
	{
		Vector3 hittedPos = ph.Hitted.transform.position;
		Vector3 force = ph.Force;
		GameObject par = Instantiate(HitVFX, hittedPos, Quaternion.Euler(0f, 180f + Vector3.SignedAngle(Vector3.forward, new Vector3(force.x, 0f, force.z), Vector3.up), 0f));
		ParticleSystem.MainModule psmain = par.GetComponent<ParticleSystem>().main;
		ParticleSystem.MainModule psmain2 = par.transform.GetChild(0).GetComponent<ParticleSystem>().main;
		psmain.maxParticles = (int)Mathf.Round((9f / 51005f) * force.magnitude * force.magnitude);
		psmain2.maxParticles = (int)Mathf.Round(12f / 255025f * force.magnitude * force.magnitude);
	}

	private void _onPlayerDied(PlayerDied pd)
	{
		Instantiate(DeathVFX, pd.Player.transform.position, DeathVFX.transform.rotation);
	}

	private void _onObjectDespawned(ObjectDespawned od)
	{
		Instantiate(VanishVFX, od.Obj.transform.position, VanishVFX.transform.rotation);
	}

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.AddHandler<ObjectDespawned>(_onObjectDespawned);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
		EventManager.Instance.RemoveHandler<ObjectDespawned>(_onObjectDespawned);
	}

}
