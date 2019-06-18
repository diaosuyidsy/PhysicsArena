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

	private void Start()
	{
		currentAmmo = MaxAmmo;
		_gpc = GetComponent<GunPositionControl>();
	}
	// This function is called by PlayerController, when player is holding a gun
	// And it's holding down RT
	public void Shoot(float TriggerVal)
	{
		GunUI.SetActive(true);
		// If player was holding down the RT button
		// CD will add up
		// If CD >= MaxCD, nothing works, only releasing the RT will replenish the CD
		if (Mathf.Approximately(TriggerVal, 1f))
		{
			_shootCD += Time.deltaTime;
			if (_shootCD >= ShootMaxCD)
			{
				WaterBall.speed = 0f;
				return;
			}
		}

		if (Mathf.Approximately(TriggerVal, 0f))
		{
			// Need to reset shoot CD if player has released RT
			_shootCD = 0f;
			WaterBall.speed = 0f;
			return;
		}
		// This means we are actually shooting water
		WaterBall.speed = Speed;
		// Statistics: Here we are using raycast for players hit
		_detectPlayer();
		// As long as player are actively spraying, should add that time to the record
		_addToSprayTime();
		// Statistics: End
		if (_gpc != null)
		{
			_gpc.Owner.GetComponent<Rigidbody>().AddForce(-_gpc.Owner.transform.forward * BackFireThrust, ForceMode.Impulse);
			_gpc.Owner.GetComponent<Rigidbody>().AddForce(_gpc.Owner.transform.up * BackFireThrust * UpThrust, ForceMode.Impulse);
		}
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
