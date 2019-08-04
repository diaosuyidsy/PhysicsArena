using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

[RequireComponent(typeof(LineRenderer))]
public class rtBazooka : WeaponBase
{
	public Vector3 BazookaShadowTransformPosition { get { return _shadowThrowMark.transform.position; } }
	private enum BazookaStates
	{
		Idle,
		Aiming,
		Out,
	}
	private BazookaStates _bazookaState = BazookaStates.Idle;
	private Player _player;
	private Vector3 _throwMarkGravity
	{
		get
		{
			return Physics.gravity * WeaponDataStore.BazookaDataStore.MarkGravityScale;
		}
	}
	private Vector3 _startVelocity
	{
		get
		{
			float diffz = _shadowThrowMark.position.z - transform.position.z;
			float diffx = _shadowThrowMark.position.x - transform.position.x;
			Vector3 result = new Vector3(diffx, 0f, diffz);
			float mag = Mathf.Tan(_throwAngle * Mathf.Deg2Rad) * result.magnitude;
			result.y = mag;
			return result.normalized * WeaponDataStore.BazookaDataStore.MarkThrowThurst;
		}
	}
	[Range(10f, 89f)]
	private float _throwAngle;
	private LineRenderer _lineRenderer;
	private Transform _throwMark;
	private Transform _shadowThrowMark;
	private float _range
	{
		get
		{
			return Mathf.Pow(WeaponDataStore.BazookaDataStore.MarkThrowThurst, 2) / -_throwMarkGravity.y - 1f;
		}
	}
	private float _HLAxis { get { return _player.GetAxis("Move Horizontal"); } }
	private float _VLAxis { get { return _player.GetAxis("Move Vertical"); } }

	protected override void Awake()
	{
		base.Awake();
		_lineRenderer = GetComponent<LineRenderer>();
		_throwMark = transform.Find("ThrowMark");
		Debug.Assert(_throwMark != null);
		Vector3 throwmarkscaleAdjust = Vector3.one;
		throwmarkscaleAdjust.x = WeaponDataStore.BazookaDataStore.MaxAffectionRange / _throwMark.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
		throwmarkscaleAdjust.y = WeaponDataStore.BazookaDataStore.MaxAffectionRange / _throwMark.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
		throwmarkscaleAdjust.z = WeaponDataStore.BazookaDataStore.MaxAffectionRange / _throwMark.GetComponent<SpriteRenderer>().sprite.bounds.size.z;
		_throwMark.transform.localScale = throwmarkscaleAdjust * 2f;
		_shadowThrowMark = transform.Find("ShadowThrowMark");
		Debug.Assert(_shadowThrowMark != null);
		_ammo = WeaponDataStore.BazookaDataStore.MaxAmmo;
	}

	private void Update()
	{
		if (_bazookaState == BazookaStates.Aiming)
		{
			_aim();
		}
		else if (_bazookaState == BazookaStates.Out)
		{
			Vector3 movement = new Vector3(_HLAxis, 0f, -_VLAxis) * WeaponDataStore.BazookaDataStore.MarkAirMoveSpeed * Time.deltaTime;
			transform.position += movement;
			_throwMark.position += movement;
			transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity);
			RaycastHit hit;
			if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out hit, 0.5f, WeaponDataStore.BazookaDataStore.HitExplodeLayer))
			{
				if (hit.collider.gameObject == _gpc.Owner) return;
				_bazookaState = BazookaStates.Idle;
				List<GameObject> affectedPlayers = new List<GameObject>();
				foreach (PlayerController _pc in Services.GameStateManager.PlayerControllers)
				{
					float dis = Vector3.Distance(_pc.transform.position, transform.position);
					if (_pc.gameObject.activeInHierarchy &&
						dis < WeaponDataStore.BazookaDataStore.MaxAffectionRange &&
						_pc.gameObject != _gpc.Owner &&
						!Physics.Linecast(_pc.transform.position, transform.position, WeaponDataStore.BazookaDataStore.CanHideLayer))
					{
						affectedPlayers.Add(_pc.gameObject);
						Vector3 dir = _pc.transform.position - transform.position;
						dir.y = 0f;
						_pc.gameObject.GetComponent<Rigidbody>().AddForce(WeaponDataStore.BazookaDataStore.MaxAffectionForce * dir.normalized, ForceMode.Impulse);
					}
				}
				EventManager.Instance.TriggerEvent(new BazookaBombed(gameObject, _gpc.Owner, _gpc.Owner.GetComponent<PlayerController>().PlayerNumber, affectedPlayers));
				_gpc.Owner.GetComponent<PlayerController>().OnImpact(new StunEffect(5f, 0f));
				_onWeaponUsedOnce();
			}
		}
	}

	private void FixedUpdate()
	{
		if (_bazookaState == BazookaStates.Out)
			GetComponent<Rigidbody>().AddForce(Vector3.down * Physics.gravity.y * (1f - WeaponDataStore.BazookaDataStore.MarkGravityScale));
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, WeaponDataStore.BazookaDataStore.MaxAffectionRange);
	}

	public override void Fire(bool buttondown)
	{
		if (buttondown && _bazookaState == BazookaStates.Idle)
		{
			_bazookaState = BazookaStates.Aiming;
			_player = ReInput.players.GetPlayer(_gpc.Owner.GetComponent<PlayerController>().PlayerNumber);
			_throwMark.gameObject.SetActive(true);
			Services.GameStateManager.CameraTargets.Add(_throwMark);
			_shadowThrowMark.gameObject.SetActive(true);
			_throwMark.parent = null;
			_shadowThrowMark.parent = null;
			_throwMark.eulerAngles = new Vector3(90f, 0f, 0f);
		}
		else if (!buttondown && _bazookaState == BazookaStates.Aiming)
		{
			_bazookaState = BazookaStates.Out;
			_lineRenderer.enabled = false;
			transform.GetComponent<Rigidbody>().isKinematic = false;
			transform.GetComponent<Rigidbody>().velocity = _startVelocity;
			_gpc.FollowHand = false;
		}
	}

	protected override void _onWeaponDespawn()
	{
		_gpc.FollowHand = true;
		_bazookaState = BazookaStates.Idle;
		_lineRenderer.enabled = false;
		_player = null;
		_throwMark.gameObject.SetActive(false);
		Services.GameStateManager.CameraTargets.Remove(_throwMark);
		_shadowThrowMark.gameObject.SetActive(false);
		_throwMark.parent = transform;
		_shadowThrowMark.parent = transform;
		_throwMark.transform.localPosition = Vector3.zero;
		_throwMark.transform.localEulerAngles = Vector3.zero;
		_shadowThrowMark.transform.localPosition = Vector3.zero;
		_shadowThrowMark.transform.localEulerAngles = Vector3.zero;
		_ammo = WeaponDataStore.BazookaDataStore.MaxAmmo;
		transform.GetComponent<Rigidbody>().isKinematic = false;
		_gpc.CanBePickedUp = true;
		gameObject.SetActive(false);
	}

	private void _aim()
	{
		Vector3 newPosition = _shadowThrowMark.position + new Vector3(_HLAxis, 0f, -_VLAxis) * Time.deltaTime * WeaponDataStore.BazookaDataStore.MarkMoveSpeed;
		RaycastHit hit;
		if (Physics.Raycast(newPosition + new Vector3(0, 20f), Vector3.down, out hit, 30f, WeaponDataStore.BazookaDataStore.LineCastLayer))
		{
			newPosition.y = hit.point.y;
			_throwMark.gameObject.SetActive(true);
		}
		else _throwMark.gameObject.SetActive(false);

		float distance = Vector3.Distance(new Vector3(newPosition.x, 0f, newPosition.z), new Vector3(transform.position.x, 0f, transform.position.z));
		if (distance > _range)
		{
			Vector3 fromOriginToObject = newPosition - transform.position;
			fromOriginToObject *= _range / distance;
			newPosition = transform.position + fromOriginToObject;
			_shadowThrowMark.position = newPosition;
		}
		else
			_shadowThrowMark.position = newPosition;
		_throwAngle = 90f - Mathf.Asin(-_throwMarkGravity.y * distance / Mathf.Pow(WeaponDataStore.BazookaDataStore.MarkThrowThurst, 2)) * Mathf.Rad2Deg / 2f;

		_drawTrajectory();

	}

	private void _drawTrajectory()
	{
		var points = _getTrajectoryPoints(transform.position, _startVelocity, WeaponDataStore.BazookaDataStore.TrajectoryLineStep, WeaponDataStore.BazookaDataStore.TrajectoryLineTime);
		if (_lineRenderer)
		{
			if (!_lineRenderer.enabled) _lineRenderer.enabled = true;
			_lineRenderer.positionCount = points.Count;
			_lineRenderer.SetPositions(points.ToArray());
		}
		if (_throwMark.gameObject)
		{
			//if (!_throwMark.gameObject.activeSelf) _throwMark.gameObject.SetActive(true);
			if (points.Count > 1)
				_throwMark.position = points[points.Count - 1];
		}
	}

	private List<Vector3> _getTrajectoryPoints(Vector3 start, Vector3 startVelocity, float timestep, float maxTime)
	{
		Vector3 prev = start;
		List<Vector3> points = new List<Vector3>();
		points.Add(prev);
		for (int i = 1; ; i++)
		{
			float t = timestep * i;
			if (t > maxTime) break;
			Vector3 pos = PlotTrajectoryAtTime(start, startVelocity, t);
			RaycastHit hit;
			if (Physics.Linecast(prev, pos, out hit, WeaponDataStore.BazookaDataStore.LineCastLayer))
			{
				points.Add(hit.point);
				break;
			}
			points.Add(pos);
			prev = pos;
		}
		return points;
	}

	private Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, float time)
	{
		return start + startVelocity * time + _throwMarkGravity * time * time * 0.5f;
	}
}
