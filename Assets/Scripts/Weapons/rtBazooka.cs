using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

[RequireComponent(typeof(LineRenderer))]
public class rtBazooka : WeaponBase
{
    [HideInInspector]
    public GameObject BazookaTrailVFXHolder;
    public Vector3 BazookaShadowTransformPosition { get { return _shadowThrowMark.transform.position; } }
    private BazookaData _bazookaData;
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
            return Physics.gravity * _bazookaData.MarkGravityScale;
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
            return result.normalized * _bazookaData.MarkThrowThurst;
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
            return Mathf.Pow(_bazookaData.MarkThrowThurst, 2) / -_throwMarkGravity.y - 1f;
        }
    }
    private float _HLAxis { get { return _player.GetAxis("Move Horizontal"); } }
    private float _VLAxis { get { return _player.GetAxis("Move Vertical"); } }

    protected override void Awake()
    {
        base.Awake();
        _bazookaData = WeaponDataBase as BazookaData;
        _lineRenderer = GetComponent<LineRenderer>();
        _throwMark = transform.Find("ThrowMark");
        Debug.Assert(_throwMark != null);
        Vector3 throwmarkscaleAdjust = Vector3.one;
        throwmarkscaleAdjust.x = _bazookaData.MaxAffectionRange / _throwMark.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        throwmarkscaleAdjust.y = _bazookaData.MaxAffectionRange / _throwMark.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        throwmarkscaleAdjust.z = _bazookaData.MaxAffectionRange / _throwMark.GetComponent<SpriteRenderer>().sprite.bounds.size.z;
        _throwMark.transform.localScale = throwmarkscaleAdjust * 2f;
        _shadowThrowMark = transform.Find("ShadowThrowMark");
        Debug.Assert(_shadowThrowMark != null);
        _ammo = _bazookaData.MaxAmmo;
    }

    protected override void Update()
    {
        base.Update();
        if (_bazookaState == BazookaStates.Aiming)
        {
            _aim();
        }
        else if (_bazookaState == BazookaStates.Out)
        {
            if (Owner == null) return;
            Vector3 movement = new Vector3(_HLAxis, 0f, -_VLAxis) * _bazookaData.MarkAirMoveSpeed * Time.deltaTime;
            transform.position += movement;
            Vector3 _throwMarkNewPos = _throwMark.position + movement;
            // _throwMark.position += movement;
            RaycastHit hit1;
            if (Physics.Raycast(_throwMarkNewPos + new Vector3(0, 20f), Vector3.down, out hit1, 30f, _bazookaData.LineCastLayer))
            {
                _throwMarkNewPos.y = hit1.point.y;
                _throwMark.gameObject.SetActive(true);
            }
            else _throwMark.gameObject.SetActive(false);
            _throwMark.position = _throwMarkNewPos;
            transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity);
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out hit, 0.5f, _bazookaData.HitExplodeLayer ^ (1 << Owner.layer)))
            {
                if (hit.collider.gameObject == Owner) return;
                _bazookaState = BazookaStates.Idle;
                List<GameObject> affectedPlayers = new List<GameObject>();
                Collider[] hitColliders = Physics.OverlapSphere(transform.position,
                                                                    _bazookaData.MaxAffectionRange,
                                                                    _bazookaData.CanHitLayer);
                foreach (Collider _c in hitColliders)
                {
                    IHittable ih = _c.GetComponent<IHittable>();
                    if (ih == null || Owner == _c.gameObject ||
                    Physics.Linecast(_c.transform.position, transform.position, _bazookaData.CanHideLayer)) continue;
                    affectedPlayers.Add(_c.gameObject);
                    Vector3 dir = _c.transform.position - transform.position;
                    dir.y = 0f;
                    ih.OnImpact(_bazookaData.MaxAffectionForce * dir.normalized, ForceMode.Impulse, Owner, ImpactType.BazookaGun);
                }
                EventManager.Instance.TriggerEvent(new BazookaBombed(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber, affectedPlayers));
                Owner.GetComponent<IHittable>().OnImpact(new StunEffect(_bazookaData.SelfStunTime, 0f));
                _resetThrowMark();
                _onWeaponUsedOnce();
            }
        }
    }


    private void FixedUpdate()
    {
        if (_bazookaState == BazookaStates.Out)
            GetComponent<Rigidbody>().AddForce(Vector3.down * Physics.gravity.y * (1f - _bazookaData.MarkGravityScale));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _bazookaData.MaxAffectionRange);
    }

    public override void Fire(bool buttondown)
    {
        if (buttondown && _bazookaState == BazookaStates.Idle)
        {
            _bazookaState = BazookaStates.Aiming;
            _player = ReInput.players.GetPlayer(Owner.GetComponent<PlayerController>().PlayerNumber);
            _throwMark.gameObject.SetActive(true);
            EventManager.Instance.TriggerEvent(new OnAddCameraTargets(_throwMark.gameObject, 1));
            _shadowThrowMark.gameObject.SetActive(true);
            _throwMark.parent = null;
            _shadowThrowMark.parent = null;
            _throwMark.eulerAngles = new Vector3(90f, 0f, 0f);
        }
        else if (!buttondown && _bazookaState == BazookaStates.Aiming)
        {
            _bazookaState = BazookaStates.Out;
            EventManager.Instance.TriggerEvent(new BazookaFired(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber));
            _lineRenderer.enabled = false;
            transform.GetComponent<Rigidbody>().isKinematic = false;
            transform.GetComponent<Rigidbody>().velocity = _startVelocity;
            _followHand = false;
        }
    }

    protected override void OnCollisionEnter(Collision other)
    {
        if (_bazookaState == BazookaStates.Out) return;
        base.OnCollisionEnter(other);
    }

    protected override void _onWeaponDespawn()
    {
        _followHand = true;
        if (BazookaTrailVFXHolder != null) BazookaTrailVFXHolder.SetActive(false);
        _bazookaState = BazookaStates.Idle;
        _lineRenderer.enabled = false;
        _player = null;
        _resetThrowMark();
        _ammo = _bazookaData.MaxAmmo;
        transform.GetComponent<Rigidbody>().isKinematic = false;
        _hitGroundOnce = false;
        base._onWeaponDespawn();
    }

    private void _resetThrowMark()
    {
        _throwMark.gameObject.SetActive(false);
        EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(_throwMark.gameObject));
        _shadowThrowMark.gameObject.SetActive(false);
        _throwMark.parent = transform;
        _shadowThrowMark.parent = transform;
        _throwMark.transform.localPosition = Vector3.zero;
        _throwMark.transform.localEulerAngles = Vector3.zero;
        _shadowThrowMark.transform.localPosition = Vector3.zero;
        _shadowThrowMark.transform.localEulerAngles = Vector3.zero;
    }

    private void _aim()
    {
        Vector3 newPosition = _shadowThrowMark.position + new Vector3(_HLAxis, 0f, -_VLAxis) * Time.deltaTime * _bazookaData.MarkMoveSpeed;
        RaycastHit hit;
        if (Physics.Raycast(newPosition + new Vector3(0, 20f), Vector3.down, out hit, 30f, _bazookaData.LineCastLayer))
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
        _throwAngle = 90f - Mathf.Asin(-_throwMarkGravity.y * distance / Mathf.Pow(_bazookaData.MarkThrowThurst, 2)) * Mathf.Rad2Deg / 2f;

        _drawTrajectory();

    }

    private void _drawTrajectory()
    {
        var points = _getTrajectoryPoints(transform.position, _startVelocity, _bazookaData.TrajectoryLineStep, _bazookaData.TrajectoryLineTime);
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
            if (Physics.Linecast(prev, pos, out hit, _bazookaData.LineCastLayer))
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
