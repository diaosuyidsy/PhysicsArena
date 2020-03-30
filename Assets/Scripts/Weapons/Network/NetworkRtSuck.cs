using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Mirror;

public class NetworkRtSuck : NetworkWeaponBase
{
    public GameObject suckBallEffect;
    public List<GameObject> suckBallExplodeEffects;
    public ParticleSystem DistortionSphere;
    public DOTweenAnimation lineParticle;
    private float _ballTraveledTime = 0f;
    private GameObject _suckBall;
    private Vector3 _suckBallInitialScale;
    private Quaternion _ballEffectQuaternion;


    private enum State
    {
        In,
        Out,
        Suck,
    }
    [SyncVar]
    private State _ballState;
    private NetworkSuckBallController _sbc;
    private SuckGunData _suckGunData;

    protected override void Awake()
    {
        base.Awake();
        _suckGunData = WeaponDataBase as SuckGunData;
        _ballState = State.In;
        _suckBall = transform.GetChild(0).gameObject;
        _sbc = _suckBall.GetComponent<NetworkSuckBallController>();
        _suckBallInitialScale = new Vector3(_suckBall.transform.localScale.x, _suckBall.transform.localScale.y, _suckBall.transform.localScale.z);
        _ammo = _suckGunData.SuckGunMaxUseTimes;
        _ballEffectQuaternion = suckBallEffect.transform.rotation;

    }

    protected override void Update()
    {
        base.Update();
        if (_ballState == State.Out)
        {
            _suckBall.transform.position += Time.deltaTime * -1f * _suckBall.transform.right * _suckGunData.BallTravelSpeed;
            _ballTraveledTime += Time.deltaTime;
            if (_ballTraveledTime >= _suckGunData.MaxBallTravelTime)
            {
                _ballTraveledTime = 0f;
                _suckBall.transform.parent = transform;
                _suckBall.transform.localPosition = new Vector3(-0.468f, 0f);
                _suckBall.transform.localEulerAngles = Vector3.zero;
                _suckBall.transform.localScale = _suckBallInitialScale;
                _suckBall.SetActive(false);
                if (isServer)
                    _ballState = State.In;
                // Need a little clean up the line renderer and stuff
                _sbc.CleanUpAll();
            }
        }
    }

    public override void Fire(bool buttondown)
    {
        // If player pressed RT
        if (buttondown)
        {
            switch (_ballState)
            {
                case State.In:
                    _ballState = State.Out;
                    RpcShootOutBall();
                    _suckBall.SetActive(true);
                    suckBallEffect.transform.rotation = _ballEffectQuaternion;
                    suckBallEffect.GetComponent<DOTweenAnimation>().DOPauseAllById("Explode");
                    suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Create");
                    _suckBall.transform.parent = null;
                    EventManager.Instance.TriggerEvent(new SuckGunFired(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber));
                    break;
            }
        }
        else
        {
            // If player released RT
            if (_ballState == State.Out)
            {
                RpcReleaseRT();
                suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Suck");
                lineParticle.DORestart();
                lineParticle.GetComponent<ParticleSystem>().Play();
                DistortionSphere.Play();

                _ballState = State.Suck;
                StartCoroutine(sucking(0.1f));

            }
        }
    }

    [ClientRpc]
    private void RpcReleaseRT()
    {
        suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Suck");
        lineParticle.DORestart();
        lineParticle.GetComponent<ParticleSystem>().Play();
        DistortionSphere.Play();
    }

    [ClientRpc]
    private void RpcShootOutBall()
    {
        _suckBall.SetActive(true);
        suckBallEffect.transform.rotation = _ballEffectQuaternion;
        suckBallEffect.GetComponent<DOTweenAnimation>().DOPauseAllById("Explode");
        suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Create");
        _suckBall.transform.parent = null;
        EventManager.Instance.TriggerEvent(new SuckGunFired(gameObject, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber));
    }

    // This is only called on server
    IEnumerator sucking(float time)
    {
        List<GameObject> gos = _sbc.InRangePlayers;
        EventManager.Instance.TriggerEvent(new SuckGunSuck(gameObject, _suckBall, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber,
            gos));
        RpcSucking1();
        // First prototype: let's try adding a force to every object
        yield return new WaitForSeconds(time);

        foreach (GameObject go in gos)
        {
            Vector3 force = (_suckBall.transform.position + new Vector3(0, 2f, 0) - go.transform.position).normalized * _suckGunData.SuckStrength;
            // go.GetComponent<IHittable>().OnImpact(force, ForceMode.Impulse, Owner, ImpactType.SuckGun);
            TargetSuckPlayer(go.GetComponent<NetworkIdentity>().connectionToClient, go, force, Owner);
        }
        yield return new WaitForSeconds(0.45f);
        RpcSucking2();
        foreach (var suckBallExplodeEffect in suckBallExplodeEffects)
        {
            suckBallExplodeEffect.GetComponent<ParticleSystem>().Play();
        }
        suckBallEffect.GetComponent<DOTweenAnimation>().DOPauseAllById("Suck");
        suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Explode");
        yield return new WaitForSeconds(0.45f);
        RpcSucking3();
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
        _onWeaponUsedOnce();
    }
    [TargetRpc]
    private void TargetSuckPlayer(NetworkConnection connection, GameObject target, Vector3 force, GameObject owner)
    {
        target.GetComponent<IHittable>().OnImpact(force, ForceMode.Impulse, owner, ImpactType.SuckGun);
    }

    [ClientRpc]
    private void RpcSucking1()
    {
        List<GameObject> gos = _sbc.InRangePlayers;
        EventManager.Instance.TriggerEvent(new SuckGunSuck(gameObject, _suckBall, Owner, Owner.GetComponent<PlayerControllerMirror>().PlayerNumber,
            gos));
    }

    [ClientRpc]
    private void RpcSucking2()
    {
        foreach (var suckBallExplodeEffect in suckBallExplodeEffects)
        {
            suckBallExplodeEffect.GetComponent<ParticleSystem>().Play();
        }
        suckBallEffect.GetComponent<DOTweenAnimation>().DOPauseAllById("Suck");
        suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Explode");

    }

    [ClientRpc]
    private void RpcSucking3()
    {
        // After time, disable the suckball and return it to the original position,
        // reset ballstate;
        _ballTraveledTime = 0f;
        _suckBall.transform.parent = transform;
        _suckBall.transform.localPosition = new Vector3(-0.468f, 0f);
        _suckBall.transform.localEulerAngles = Vector3.zero;
        _suckBall.transform.localScale = _suckBallInitialScale;
        _suckBall.SetActive(false);
        // Need a little clean up the line renderer and stuff
        _sbc.CleanUpAll();
    }

    public bool isSucking()
    {
        return _ballState == State.Suck;
    }

    protected override void _onWeaponDespawn()
    {
        RpcSuckGunDespawn();
        _ballTraveledTime = 0f;
        _suckBall.transform.parent = transform;
        _suckBall.transform.localPosition = new Vector3(-0.468f, 0f);
        _suckBall.transform.localEulerAngles = Vector3.zero;
        _suckBall.transform.localScale = _suckBallInitialScale;
        _suckBall.SetActive(false);
        _ballState = State.In;
        _ammo = _suckGunData.SuckGunMaxUseTimes;
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        // Need a little clean up the line renderer and stuff
        _sbc.CleanUpAll();
        base._onWeaponDespawn();
    }

    [ClientRpc]
    private void RpcSuckGunDespawn()
    {
        _ballTraveledTime = 0f;
        _suckBall.transform.parent = transform;
        _suckBall.transform.localPosition = new Vector3(-0.468f, 0f);
        _suckBall.transform.localEulerAngles = Vector3.zero;
        _suckBall.transform.localScale = _suckBallInitialScale;
        _suckBall.SetActive(false);
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        _sbc.CleanUpAll();

    }
}
