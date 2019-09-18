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

    protected override void Awake()
    {
        base.Awake();
        _ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
        _boomerangState = BoomerangState.In;
        _hitSet = new HashSet<PlayerController>();
    }

    protected override void Update()
    {
        base.Update();
        if (_boomerangState == BoomerangState.Out)
        {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, WeaponDataStore.BoomerangDataStore.HitRadius, transform.forward, out hit, WeaponDataStore.BoomerangDataStore.HitMaxDistance, WeaponDataStore.BoomerangDataStore.CanHitLayer))
            {
                print("hit1");

                if (hit.collider.GetComponent<WeaponBase>() != null) return;
                print("hit");
                PlayerController pc = hit.collider.GetComponentInParent<PlayerController>();
                if (pc != null && !_hitSet.Contains(pc))
                {
                    pc.OnImpact((pc.transform.position - transform.position).normalized * WeaponDataStore.BoomerangDataStore.OnHitForce, ForceMode.Impulse, _firer, ImpactType.Boomerang);
                    _hitSet.Add(pc);
                }
            }
            else if (Physics.SphereCast(transform.position, WeaponDataStore.BoomerangDataStore.HitRadius, transform.forward, out hit, WeaponDataStore.BoomerangDataStore.HitMaxDistance, WeaponDataStore.BoomerangDataStore.ObstacleLayer))
            {
                print("Hit Obstacles");
                _boomerangState = BoomerangState.In;
                _pathMoveTweener.Kill();
                GetComponent<Rigidbody>().velocity = transform.forward * WeaponDataStore.BoomerangDataStore.BoomerangSpeed;
                return;
            }
        }
    }

    public override void Fire(bool buttondown)
    {
        if (!buttondown)
        {
            _firer = Owner;
            _boomerangState = BoomerangState.Out;
            Vector3[] localPath = new Vector3[WeaponDataStore.BoomerangDataStore.LocalMovePoints.Length];
            for (int i = 0; i < WeaponDataStore.BoomerangDataStore.LocalMovePoints.Length; i++)
            {
                localPath[i] = transform.position + transform.forward * WeaponDataStore.BoomerangDataStore.LocalMovePoints[i].z + transform.right * WeaponDataStore.BoomerangDataStore.LocalMovePoints[i].x;
            }
            _pathMoveTweener = transform.DOLocalPath(localPath, WeaponDataStore.BoomerangDataStore.BoomerangSpeed, PathType.CatmullRom)
            .SetSpeedBased(true)
            .SetEase(WeaponDataStore.BoomerangDataStore.BoomEase)
            .SetLookAt(0f)
            .OnComplete(() =>
            {
                GetComponent<Rigidbody>().velocity = transform.forward * WeaponDataStore.BoomerangDataStore.BoomerangSpeed;
            });
            _onWeaponUsedOnce();
        }
    }

    protected override void _onWeaponDespawn()
    {
        _boomerangState = BoomerangState.In;
        _ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
        _hitSet.Clear();
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
