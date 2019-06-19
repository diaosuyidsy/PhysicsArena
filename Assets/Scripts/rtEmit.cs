using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class rtEmit : MonoBehaviour
{
	public ObiEmitter WaterBall;
	public GameObject WaterUI;
	public GameObject GunUI;
	public float Speed;
	public float BackFireThrust;
	public float UpThrust = 1f;
	public int MaxAmmo = 1000;
	public int currentAmmo;
	public float ShootMaxCD = 0.3f;
	public LayerMask OnHitDisappear;

	private float _shootCD = 0f;
	private GunPositionControl _gpc;

	private enum State
	{
		Empty,
		Shooting,
	}

	private State _waterGunState;

	private void Awake()
	{
		currentAmmo = MaxAmmo;
		_gpc = GetComponent<GunPositionControl>();
		_waterGunState = State.Empty;
	}

	private void Update()
	{
		switch (_waterGunState)
		{
			case State.Shooting:
				_shootCD += Time.deltaTime;
				if (_shootCD >= ShootMaxCD)
				{
					WaterBall.speed = 0f;
					_waterGunState = State.Empty;
					return;
				}
				// Statistics: Here we are using raycast for players hit
				_detectPlayer();
				// As long as player are actively spraying, should add that time to the record
				//_addToSprayTime();
				// Statistics: End
				currentAmmo--;
				if (currentAmmo <= 0)
				{
					_gpc.CanBePickedUp = false;
					// If no ammo left, then drop the weapon
					_gpc.Owner.GetComponent<PlayerController>().DropHelper();
					_shootCD = 0f;
					WaterBall.speed = 0f;
				}
				// If we changed ammo, then need to change UI as well
				ChangeAmmoUI();
				break;
			case State.Empty:
				break;
		}
	}

	public void Shoot(bool down)
	{
		/// means we pressed down button here
		if (down)
		{
			_waterGunState = State.Shooting;
			GunUI.SetActive(true);
			WaterBall.speed = Speed;
			EventManager.Instance.TriggerEvent(new WaterGunFired(gameObject, _gpc.Owner, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber));
		}
		else
		{
			_waterGunState = State.Empty;
			WaterBall.speed = 0f;
			_shootCD = 0f;
		}
	}

	//when gun leaves hands, UI disappears.
	public void KillUI()
	{
		GunUI.SetActive(false);
	}

	private void _detectPlayer()
	{
		// This layermask means we are only looking for Player1Body - Player6Body
		int layermask = (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12) | (1 << 15) | (1 << 16);
		RaycastHit hit;
		if (Physics.Raycast(transform.position, -transform.right, out hit, Mathf.Infinity, layermask))
		{
			print("Hit: " + hit.transform.name);
			hit.transform.GetComponentInParent<PlayerController>().Mark(GetComponent<GunPositionControl>().Owner);
		}
	}

	private void _addToSprayTime()
	{
		int playerNumber = _gpc.Owner.GetComponent<PlayerController>().PlayerNumber;
		GameManager.GM.WaterGunUseTime[playerNumber] += Time.deltaTime;
	}

	private void ChangeAmmoUI()
	{
		float scaleY = currentAmmo * 1.0f / MaxAmmo;
		WaterUI.transform.localScale = new Vector3(1f, scaleY, 1f);
	}

	// If weapon collide to the ground, and has no ammo, then despawn it
	// And if weapon does not collide to the ground or other allowed 
	private void OnCollisionEnter(Collision other)
	{
		if (other.collider.CompareTag("Ground") && currentAmmo == 0)
		{
			currentAmmo = MaxAmmo;
			ChangeAmmoUI();
			EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
			gameObject.SetActive(false);
		}
		if (((1 << other.gameObject.layer) & OnHitDisappear) != 0)
		{
			StartCoroutine(DisappearAfterAWhile(3f));
		}
	}

	private void VanishAfterUse()
	{
		currentAmmo = MaxAmmo;
		ChangeAmmoUI();
		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
		gameObject.SetActive(false);
	}

	// If the weapon is taken down the death zone, then despawn it
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DeathZone"))
		{
			currentAmmo = MaxAmmo;
			ChangeAmmoUI();
			gameObject.SetActive(false);
		}
	}

	IEnumerator DisappearAfterAWhile(float time)
	{
		yield return new WaitForSeconds(time);
		currentAmmo = MaxAmmo;
		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
		ChangeAmmoUI();
		gameObject.SetActive(false);
	}

}
