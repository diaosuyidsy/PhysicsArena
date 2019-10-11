using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class rtBoomerang : WeaponBase
{
    private GameObject _firer;
    private HashSet<PlayerController> _hitSet;
    private DOTweenAnimation _meshRotate;
    private Vector3 _initialPos;
    private float _fireTime;
    private Rigidbody _rb;
    private bool _canHit;
    private LineRenderer _lr;
    private List<Vector3> _axuilaryLinePoints;

    private FSM<rtBoomerang> _boomerangFSM;
    private bool _UIShow;

    protected override void Awake()
    {
        base.Awake();
        _ammo = WeaponDataStore.BoomerangDataStore.MaxAmmo;
        _hitSet = new HashSet<PlayerController>();
        _meshRotate = GetComponentInChildren<DOTweenAnimation>();
        _rb = GetComponent<Rigidbody>();
        _lr = GetComponent<LineRenderer>();
        _axuilaryLinePoints = new List<Vector3>();
        for (int i = 0; i < WeaponDataStore.BoomerangDataStore.Time / WeaponDataStore.BoomerangDataStore.Step; i++)
        {
            _axuilaryLinePoints.Add(Vector3.zero);
        }
        _lr.positionCount = _axuilaryLinePoints.Count;
        _boomerangFSM = new FSM<rtBoomerang>(this);
        _boomerangFSM.TransitionTo<BoomerangInState>();
    }

    protected override void Update()
    {
        base.Update();
        _boomerangFSM.Update();
        if (_UIShow)
            _updateUI();
    }

    private void FixedUpdate()
    {
        _boomerangFSM.FixedUpdate();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (_boomerangFSM.CurrentState.GetType().Equals(typeof(BoomerangInState))) return;
        if (WeaponDataStore.BoomerangDataStore.ObstacleLayer == (WeaponDataStore.BoomerangDataStore.ObstacleLayer | (1 << other.gameObject.layer)))
        {
            print("Hit Obstacles");
            _rb.velocity = Vector3.zero;
            _rb.AddForce(WeaponDataStore.BoomerangDataStore.BoomerangReflectionForce * Vector3.up, ForceMode.VelocityChange);
            _boomerangFSM.TransitionTo<BoomerangInState>();
        }
    }

    private void _updateUI()
    {
        RaycastHit hit;
        BoomerangData bd = WeaponDataStore.BoomerangDataStore;
        float yy = transform.position.y;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f, WeaponDataStore.BoomerangDataStore.GroundLayer))
        {
            yy = hit.point.y;
        }

        for (int i = 0; i < _axuilaryLinePoints.Count; i++)
        {
            float t = i * bd.Step;
            float ax0 = bd.BoomerangAngleVelocity;
            float az0 = 0f;
            float t0 = bd.Time / 2f;
            float vx0 = bd.BoomerangInitialSpeed * Mathf.Sin(bd.BoomerangInitialLeftwardAngle * Mathf.Deg2Rad);
            float vz0 = bd.BoomerangInitialSpeed * Mathf.Cos(bd.BoomerangInitialLeftwardAngle * Mathf.Deg2Rad);
            float sx = (-ax0 / (12f * t0 * t0)) * Mathf.Pow(t, 4) + (ax0 / (3f * t0)) * Mathf.Pow(t, 3) + vx0 * t;
            float sz = (az0 / (12f * t0 * t0)) * Mathf.Pow(t, 4) - (az0 / (3f * t0)) * Mathf.Pow(t, 3) + (az0 / 2f) * t * t + vz0 * t;
            _axuilaryLinePoints[i] = transform.position + transform.forward * sz + transform.right * sx;
            Vector3 temp = _axuilaryLinePoints[i];
            temp.y = yy;
            _axuilaryLinePoints[i] = temp;
        }
        _lr.SetPositions(_axuilaryLinePoints.ToArray());
    }

    public override void Fire(bool buttondown)
    {
        if (!buttondown)
        {
            _firer = Owner;
            _boomerangFSM.TransitionTo<BoomerangOutState>();
        }
        else
        {
            _UIShow = true;
            _lr.enabled = true;
        }
    }

    protected override void _onWeaponDespawn()
    {
        _meshRotate.DOPause();
        _boomerangFSM.TransitionTo<BoomerangInState>();
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
        Vector3 centerPoint = WeaponDataStore.BoomerangDataStore.CircleCenter;
        Gizmos.DrawSphere(transform.position + transform.forward * centerPoint.z + transform.right * centerPoint.x, 0.2f);

    }

    private abstract class BoomerangStates : FSM<rtBoomerang>.State
    {
        protected BoomerangData BoomerangData;
        public override void Init()
        {
            base.Init();
            BoomerangData = Context.WeaponDataStore.BoomerangDataStore;
        }
    }

    private class BoomerangInState : BoomerangStates
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._UIShow = false;
            Context._lr.enabled = false;
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._lr.enabled = false;
        }
    }

    private class BoomerangOutState : BoomerangStates
    {
        private Vector3 _centerPoint;
        private float _timer;
        private bool _firstFrame;
        public override void OnEnter()
        {
            base.OnEnter();
            Context._meshRotate.DORestart();
            Vector3 _initDir = Quaternion.Euler(0f, BoomerangData.BoomerangInitialLeftwardAngle, 0f) * Context.transform.forward;
            _initDir.Normalize();
            // Add Initial Force based on initila leftward angles
            Context._rb.AddForce(_initDir * BoomerangData.BoomerangInitialSpeed, ForceMode.VelocityChange);
            _centerPoint = Context.transform.position + Context.transform.forward * BoomerangData.CircleCenter.z + Context.transform.right * BoomerangData.CircleCenter.x;
            Context.GetComponent<Collider>().isTrigger = true;
            Context._rb.useGravity = false;
            _timer = Time.timeSinceLevelLoad + BoomerangData.BoomerangMaxOutTime;
            Context._onWeaponUsedOnce();
            Context._hitSet.Clear();
            _firstFrame = true;
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                TransitionTo<BoomerangInState>();
                return;
            }
            RaycastHit hit;
            if (!_firstFrame && Physics.SphereCast(Context.transform.position + Context._rb.velocity.normalized * BoomerangData.ForwardCastAmount + Vector3.up * BoomerangData.UpCastAmount,
                BoomerangData.HitRadius,
                Vector3.down,
                out hit, BoomerangData.HitMaxDistance,
                BoomerangData.CanHitLayer))
            {
                if (hit.collider.GetComponent<WeaponBase>() != null) return;
                PlayerController pc = hit.collider.GetComponentInParent<PlayerController>();
                if (pc != null && pc.CanBlock(Context.transform.forward))
                {
                    Context._rb.velocity = Vector3.zero;
                    Context._rb.AddForce(BoomerangData.BoomerangReflectionForce * Vector3.up, ForceMode.VelocityChange);
                    TransitionTo<BoomerangInState>();
                    return;
                }
                if (pc != null && !Context._hitSet.Contains(pc))
                {
                    pc.OnImpact((pc.transform.position - Context.transform.position).normalized * BoomerangData.OnHitForce, ForceMode.Impulse, Context._firer, ImpactType.Boomerang);
                    Context._hitSet.Add(pc);
                }
            }
            _firstFrame = false;
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            // First Set Angular Velocity
            Context._rb.AddForce(
                (_centerPoint - Context.transform.position).normalized * BoomerangData.BoomerangAngleVelocity, ForceMode.Acceleration);
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._rb.useGravity = true;
            Context.GetComponent<Collider>().isTrigger = false;
            Context._meshRotate.DOPause();
            Context._meshRotate.transform.localEulerAngles = Vector3.zero;
        }
    }
}
