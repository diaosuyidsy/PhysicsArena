using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class rtFist : WeaponBase
{
    public override float HelpAimAngle { get { return _fistGunData.HelpAimAngle; } }
    public override float HelpAimDistance { get { return _fistGunData.HelpAimDistance; } }
    private FistGunData _fistGunData;
    private Transform _fist;
    private GameObject _fistDup;
    private FistGunState _fistGunState = FistGunState.Idle;
    private GameObject _fireOwner;
    private enum FistGunState
    {
        Idle,
        Out,
        Recharging,
    }
    private Vector3 _maxDistance;
    private float _chargeTimer;
    private FSM<rtFist> _fistGunFSM;

    protected override void Awake()
    {
        base.Awake();
        _fistGunData = WeaponDataBase as FistGunData;
        _fist = transform.Find("Fist");
        Debug.Assert(_fist != null);
        _ammo = _fistGunData.MaxAmmo;
        _fistGunFSM = new FSM<rtFist>(this);
        _fistGunFSM.TransitionTo<FistReadyState>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        _fistGunFSM.Update();
    }

    public override void Fire(bool buttondown)
    {
        if (buttondown)
        {
            if (_fistGunFSM.CurrentState != null && _fistGunFSM.CurrentState.GetType().Equals(typeof(FistReadyState)))
            {

                _fistGunFSM.TransitionTo<FistOutState>();
            }
        }
    }

    protected override void _onWeaponDespawn()
    {
        _fistGunState = FistGunState.Idle;
        float rechargetimeleft = _chargeTimer + _fistGunData.ReloadTime - Time.time;
        rechargetimeleft = rechargetimeleft > 0f ? rechargetimeleft : 0f;
        if (_fistDup != null) Destroy(_fistDup, rechargetimeleft);
        _fistDup = null;
        _fist.gameObject.SetActive(true);
        _ammo = _fistGunData.MaxAmmo;
        base._onWeaponDespawn();
    }

    private abstract class RtFistState : FSM<rtFist>.State
    {
        protected FistGunData _fistGunData { get { return Context._fistGunData; } }
        protected GameObject gameObject { get { return Context.gameObject; } }
        protected Transform transform { get { return Context.transform; } }
        protected GameObject Owner { get { return Context.Owner; } }
    }

    private class FistReadyState : RtFistState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._fist.gameObject.SetActive(true);
        }
    }

    private class FistOutState : RtFistState
    {
        private GameObject _fistBullet;
        private Vector3 _maxDistance;
        private float _stateTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            Context._fireOwner = Owner;
            _maxDistance = transform.position + -transform.right * _fistGunData.MaxFlyDistance;
            _fistBullet = Instantiate(Context._fist.gameObject, transform);
            _fistBullet.GetComponent<FistControl>().Init(Context, _maxDistance, Context._fireOwner);
            _fistBullet.transform.position = transform.position;
            _fistBullet.transform.parent = null;
            _fistBullet.GetComponent<Collider>().isTrigger = false;
            Context._fist.gameObject.SetActive(false);
            if (!Owner.GetComponent<PlayerController>().IsIdle)
                Owner.GetComponent<PlayerController>().OnImpact(-Owner.transform.forward * _fistGunData.BackfireHitForce, ForceMode.VelocityChange, Owner, ImpactType.FistGun);
            else
                Context.Owner.GetComponent<PlayerController>().OnImpact(-Owner.transform.forward * _fistGunData.IdleBackfireHitForce, ForceMode.VelocityChange, Owner, ImpactType.FistGun);
            EventManager.Instance.TriggerEvent(new FistGunFired(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber, _fistBullet));
            _stateTimer = Time.timeSinceLevelLoad;
        }

        public override void Update()
        {
            base.Update();
            if (_stateTimer + _fistGunData.FistOutDuration < Time.timeSinceLevelLoad)
            {
                TransitionTo<FistChargingState>();
                return;
            }
        }
    }

    private class FistChargingState : RtFistState
    {
        private float _stateTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            _stateTimer = Time.timeSinceLevelLoad;
            Context._fist.DOScale(0f, _fistGunData.ReloadTime).SetEase(_fistGunData.ReloadEase).From().OnPlay(() => Context._fist.gameObject.SetActive(true));
            EventManager.Instance.TriggerEvent(new FistGunStartCharging(gameObject, Context._fireOwner, Context._fireOwner.GetComponent<PlayerController>().PlayerNumber));
        }

        public override void Update()
        {
            base.Update();
            if (_stateTimer + _fistGunData.ReloadTime < Time.timeSinceLevelLoad)
            {
                TransitionTo<FistReadyState>();
                return;
            }
        }
    }

}
