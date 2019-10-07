using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class rtBoomerang : WeaponBase
{
    private enum BoomerangState
    {
        In,
        Out,
    }
    private BoomerangState _boomerangState;
    private Tweener _pathMoveTweener;
    private GameObject _firer;
    private HashSet<PlayerController> _hitSet;
    private DOTweenAnimation _meshRotate;
    private Vector3 _initialPos;
    private float _fireTime;
    private Rigidbody _rb;
    private bool _canHit;

    protected override void Awake()
    {
        base.Awake();
        _ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
        _boomerangState = BoomerangState.In;
        _hitSet = new HashSet<PlayerController>();
        _meshRotate = GetComponentInChildren<DOTweenAnimation>();
        _rb = GetComponent<Rigidbody>();
    }

    protected override void Update()
    {
        base.Update();
        if (_boomerangState == BoomerangState.Out)
        {
            _movement();
            RaycastHit hit;
            if (Physics.SphereCast(transform.position + _rb.velocity.normalized * WeaponDataStore.BoomerangDataStore.ForwardCastAmount + Vector3.up * WeaponDataStore.BoomerangDataStore.UpCastAmount,
                WeaponDataStore.BoomerangDataStore.HitRadius,
                Vector3.down,
                out hit, WeaponDataStore.BoomerangDataStore.HitMaxDistance,
                WeaponDataStore.BoomerangDataStore.CanHitLayer))
            {
                if (hit.collider.GetComponent<WeaponBase>() != null) return;
                PlayerController pc = hit.collider.GetComponentInParent<PlayerController>();
                if (pc != null && pc.CanBlock(transform.forward))
                {
                    _boomerangState = BoomerangState.In;
                    _meshRotate.DOPause();
                    _meshRotate.transform.localEulerAngles = Vector3.zero;
                    _pathMoveTweener.Kill();
                    _hitSet.Clear();
                    _rb.velocity = Vector3.zero;
                    _rb.AddForce(WeaponDataStore.BoomerangDataStore.BoomerangReflectionForce * Vector3.up, ForceMode.Impulse);
                    _onWeaponUsedOnce();
                    return;
                }
                if (pc != null && !_hitSet.Contains(pc) && _canHit)
                {
                    pc.OnImpact((pc.transform.position - transform.position).normalized * WeaponDataStore.BoomerangDataStore.OnHitForce, ForceMode.Impulse, _firer, ImpactType.Boomerang);
                    _hitSet.Add(pc);
                }
            }
        }
    }

    protected override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);
        if (_boomerangState == BoomerangState.In) return;
        if (WeaponDataStore.BoomerangDataStore.ObstacleLayer == (WeaponDataStore.BoomerangDataStore.ObstacleLayer | (1 << other.gameObject.layer)))
        {
            print("Hit Obstacles");
            _boomerangState = BoomerangState.In;
            _meshRotate.DOPause();
            _meshRotate.transform.localEulerAngles = Vector3.zero;
            _pathMoveTweener.Kill();
            _canHit = false;
            _hitSet.Clear();
            _rb.velocity = Vector3.zero;
            _rb.AddForce(WeaponDataStore.BoomerangDataStore.BoomerangReflectionForce * Vector3.up, ForceMode.Impulse);
            _onWeaponUsedOnce();
        }
    }

    private void _movement()
    {
        float xSpeed = (WeaponDataStore.BoomerangDataStore.BoomerangVelocity.x * (_fireTime - Time.timeSinceLevelLoad));
        float zSpeed = (WeaponDataStore.BoomerangDataStore.BoomerangVelocity.z * (_fireTime - Time.timeSinceLevelLoad));
        float deltaX = 0f;
        float deltaZ = 0f;
        deltaX = WeaponDataStore.BoomerangDataStore.BoomerangAmplitude.x * Mathf.Cos(xSpeed);
        deltaZ = WeaponDataStore.BoomerangDataStore.BoomerangAmplitude.z * Mathf.Sin(zSpeed);
        Vector3 delatPos = new Vector3(deltaX, 0f, deltaZ);
        _rb.velocity = transform.forward * deltaZ + transform.right * deltaX;
        _rb.useGravity = false;
        // transform.position = _initialPos + delatPos;
    }

    public override void Fire(bool buttondown)
    {
        if (!buttondown)
        {
            _firer = Owner;
            _boomerangState = BoomerangState.Out;
            _meshRotate.DORestart();
            _initialPos = transform.position;
            _fireTime = Time.timeSinceLevelLoad;
            Vector3[] localPath = new Vector3[WeaponDataStore.BoomerangDataStore.LocalMovePoints.Length];
            for (int i = 0; i < WeaponDataStore.BoomerangDataStore.LocalMovePoints.Length; i++)
            {
                localPath[i] = transform.position + transform.forward * WeaponDataStore.BoomerangDataStore.LocalMovePoints[i].z + transform.right * WeaponDataStore.BoomerangDataStore.LocalMovePoints[i].x;
            }
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(WeaponDataStore.BoomerangDataStore.StartAffectiveDuration);
            sequence.AppendCallback(() => _canHit = true);
            sequence.AppendInterval(WeaponDataStore.BoomerangDataStore.EndAffectiveDuration);
            sequence.AppendCallback(() =>
            {
                _rb.velocity = _rb.velocity.normalized * WeaponDataStore.BoomerangDataStore.BoomerangSpeed;
                _rb.useGravity = true;
                _meshRotate.DOPause();
                _meshRotate.transform.localEulerAngles = Vector3.zero;
                _boomerangState = BoomerangState.In;
                _hitSet.Clear();
                _canHit = false;
            });
            _onWeaponUsedOnce();
        }
    }

    protected override void _onWeaponDespawn()
    {
        _boomerangState = BoomerangState.In;
        _meshRotate.DOPause();
        _meshRotate.transform.localEulerAngles = Vector3.zero;
        _ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
        _hitSet.Clear();
        _canHit = false;
        _rb.useGravity = true;
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3[] localMovePoints = WeaponDataStore.BoomerangDataStore.LocalMovePoints;
        for (int i = 0; i < localMovePoints.Length; i++)
        {
            // Gizmos.DrawSphere(transform.position + localMovePoints[i], 0.2f);
            Gizmos.DrawSphere(transform.position + transform.forward * localMovePoints[i].z + transform.right * localMovePoints[i].x, 0.2f);
        }
    }
}
