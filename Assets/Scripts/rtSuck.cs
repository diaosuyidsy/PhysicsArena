using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtSuck : MonoBehaviour
{
	public WeaponData WeaponDataStore;

	private float _ballTraveledTime = 0f;
	private GameObject _suckBall;
	private bool _charged = false;
	private Vector3 _suckBallInitialScale;
	private int _suckGunLeftTimes;

	private enum State
	{
		In,
		Out,
		Suck,
	}

	private State _ballState;
	private SuckBallController _sbc;
	private GunPositionControl _gpc;

	private void Start()
	{
		_ballState = State.In;
		_suckBall = transform.GetChild(0).gameObject;
		_sbc = _suckBall.GetComponent<SuckBallController>();
		_gpc = GetComponent<GunPositionControl>();
		_suckBallInitialScale = new Vector3(_suckBall.transform.localScale.x, _suckBall.transform.localScale.y, _suckBall.transform.localScale.z);
		_suckGunLeftTimes = WeaponDataStore.SuckGunDataStore.SuckGunMaxUseTimes;
	}

	private void Update()
	{
		if (_ballState == State.Out)
		{
			_suckBall.transform.position += Time.deltaTime * -1f * _suckBall.transform.right * WeaponDataStore.SuckGunDataStore.BallTravelSpeed;
			_ballTraveledTime += Time.deltaTime;
			if (_ballTraveledTime >= WeaponDataStore.SuckGunDataStore.MaxBallTravelTime)
			{
				_ballTraveledTime = 0f;
				_charged = false;
				_ballState = State.Suck;
				StartCoroutine(sucking(0.5f));
			}
		}
	}

	public void Suck(bool rtHolding)
	{
		// If player is holding RT (or pressed RT)
		if (rtHolding)
		{
			switch (_ballState)
			{
				case State.In:
					_ballState = State.Out;
					_suckBall.SetActive(true);
					_suckBall.transform.parent = null;
					_charged = false;
					EventManager.Instance.TriggerEvent(new SuckGunFired(gameObject, _gpc.Owner, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber));
					break;
				case State.Out:
					if (_charged)
					{
						_charged = false;
						_sbc.RTText.SetActive(false);
						_ballState = State.Suck;
						StartCoroutine(sucking(0.1f));
					}
					break;
			}
		}
		else
		{
			// If player released RT
			if (_ballState == State.Out)
			{
				_charged = true;
			}
		}
	}

	IEnumerator sucking(float time)
	{
		List<GameObject> gos = _sbc.InRangePlayers;
		EventManager.Instance.TriggerEvent(new SuckGunSuck(gameObject, _suckBall, _gpc.Owner, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber,
			gos));
		// First prototype: let's try adding a force to every object
		yield return new WaitForSeconds(time);

		foreach (GameObject go in gos)
		{
			go.GetComponent<Rigidbody>().AddForce((_suckBall.transform.position + new Vector3(0, 2f, 0) - go.transform.position).normalized * WeaponDataStore.SuckGunDataStore.SuckStrength, ForceMode.Impulse);
			// Statistics: Add kill marker
			go.GetComponent<PlayerController>().Mark(_gpc.Owner);
			// Statistics: Add every player he sucked into statistics
			_addToSuckedTimes();
			// End Statistics
		}
		yield return new WaitForSeconds(0.3f);
		////Second prototype
		//yield return StartCoroutine(Congregate(time, gos));
		// After time, disable the suckball and return it to the original position,
		// reset ballstate;
		_ballTraveledTime = 0f;
		_suckBall.transform.parent = transform;
		_suckBall.transform.localPosition = new Vector3(-0.468f, 0f);
		_suckBall.transform.localEulerAngles = Vector3.zero;
		_suckBall.transform.localScale = _suckBallInitialScale;
		_suckBall.SetActive(false);
		_ballState = State.In;
		// Need a little clean up the line renderer and stuff
		_sbc.CleanUpAll();
		_suckGunUsedOnce();
	}

	public bool isSucking()
	{
		return _ballState == State.Suck;
	}

	private void _suckGunUsedOnce()
	{
		_suckGunLeftTimes--;
		if (_suckGunLeftTimes <= 0)
		{
			_gpc.CanBePickedUp = false;
			_gpc.Owner.GetComponent<PlayerController>().DropHelper();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DeathZone"))
		{
			_ballTraveledTime = 0f;
			_suckBall.transform.parent = transform;
			_suckBall.transform.localPosition = new Vector3(-0.468f, 0f);
			_suckBall.transform.localEulerAngles = Vector3.zero;
			_suckBall.transform.localScale = _suckBallInitialScale;
			_suckBall.SetActive(false);
			_ballState = State.In;
			// Need a little clean up the line renderer and stuff
			_sbc.CleanUpAll();
			gameObject.SetActive(false);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if ((WeaponDataStore.Ground == (WeaponDataStore.Ground | (1 << other.gameObject.layer))) && _suckGunLeftTimes <= 0)
		{
			_suckGunLeftTimes = WeaponDataStore.SuckGunDataStore.SuckGunMaxUseTimes;
			EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
			gameObject.SetActive(false);
		}
		if (WeaponDataStore.OnHitDisappear == (WeaponDataStore.OnHitDisappear | (1 << other.gameObject.layer)))
		{
			StartCoroutine(DisappearAfterAWhile(0f));
		}
	}

	private void VanishAfterUsed()
	{
		_suckGunLeftTimes = WeaponDataStore.SuckGunDataStore.SuckGunMaxUseTimes;
		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
		gameObject.SetActive(false);
	}

	private void _addToSuckedTimes()
	{
		int playernum = GetComponent<GunPositionControl>().Owner.GetComponent<PlayerController>().PlayerNumber;
		GameManager.GM.SuckedPlayersTimes[playernum]++;
	}

	IEnumerator DisappearAfterAWhile(float time)
	{
		yield return new WaitForSeconds(time);
		_suckGunLeftTimes = WeaponDataStore.SuckGunDataStore.SuckGunMaxUseTimes;
		EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
		gameObject.SetActive(false);
	}
}
