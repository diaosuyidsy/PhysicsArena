using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class rtHammer : WeaponBase
{
    private enum HammerState
    {
        Idle,
        Out,
    }

    private HammerState _hammerState = HammerState.Idle;
    private Player _player;
    private float _curTravelTime;
    private float _distToGround;
    private float _HLAxis { get { return _player.GetAxis("Move Horizontal"); } }
    private float _VLAxis { get { return _player.GetAxis("Move Vertical"); } }

    protected override void Awake()
    {
        base.Awake();
        _ammo = WeaponDataStore.HammerDataStore.MaxAmmo;
    }

    protected override void Update()
    {
        base.Update();
        if (_hammerState == HammerState.Out)
        {
            _curTravelTime += Time.deltaTime;
            if (_curTravelTime >= WeaponDataStore.HammerDataStore.MaxTravelTime)
            {
                _onWeaponUsedOnce();
                _hammerState = HammerState.Idle;
                return;
            }
            transform.position += -transform.forward * WeaponDataStore.HammerDataStore.Speed * Time.deltaTime;
            if (!Mathf.Approximately(_HLAxis, 0f) || !Mathf.Approximately(0f, _VLAxis))
            {
                Vector3 transeuler = transform.eulerAngles;
                transeuler.y = Mathf.LerpAngle(transeuler.y, Mathf.Atan2(_HLAxis, _VLAxis * -1f) * Mathf.Rad2Deg, Time.deltaTime * WeaponDataStore.HammerDataStore.RotationSpeed);
                transform.eulerAngles = transeuler;
            }
            RaycastHit hit;
            if (Physics.SphereCast(Owner.transform.position, 0.3f, -transform.forward, out hit, 0.3f, WeaponDataStore.HammerDataStore.CanCollideLayer))
            {
                _onWeaponUsedOnce();
                _hammerState = HammerState.Idle;
                Owner.GetComponent<PlayerController>().OnImpact(new StunEffect(0.5f, 0f));
                return;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_hammerState == HammerState.Out && !Physics.Raycast(transform.position, Vector3.down, Owner.GetComponent<CapsuleCollider>().bounds.extents.y, WeaponDataStore.HammerDataStore.CanCollideLayer))
        {
            transform.position += new Vector3(0f, 0.5f * Physics.gravity.y * Time.fixedDeltaTime * Time.fixedDeltaTime * 10f);
        }
    }

    public override void Fire(bool buttondown)
    {
        if (buttondown)
        {
            _player = ReInput.players.GetPlayer(Owner.GetComponent<PlayerController>().PlayerNumber);
            _followHand = false;
            GetComponent<SphereCollider>().radius = WeaponDataStore.HammerDataStore.CollisionRange;
            _hammerState = HammerState.Out;
        }
    }

    protected override void _onWeaponDespawn()
    {
        _hammerState = HammerState.Idle;
        _ammo = WeaponDataStore.HammerDataStore.MaxAmmo;
        _curTravelTime = 0f;
        _player = null;
        _followHand = true;
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (_hammerState == HammerState.Out &&
            other.tag.Contains("Team") &&
            other.gameObject != Owner &&
            other.gameObject.GetComponent<PlayerController>() != null)
        {
            Vector3 dir = other.transform.position - transform.position;
            if (Vector3.Angle(-transform.forward, dir) < WeaponDataStore.HammerDataStore.CollisionAngle)
            {
                dir = dir.normalized;
                dir.y = WeaponDataStore.HammerDataStore.UpwardMultiplier;
                other.gameObject.GetComponent<IHittable>().OnImpact(dir * WeaponDataStore.HammerDataStore.CollideForce, ForceMode.Impulse, Owner, ImpactType.HammerGun);
            }
        }
    }
}
