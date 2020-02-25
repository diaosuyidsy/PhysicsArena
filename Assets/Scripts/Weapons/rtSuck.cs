using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class rtSuck : WeaponBase
{
    public GameObject suckBallEffect;
    public List<GameObject> suckBallExplodeEffects;
    public ParticleSystem DistortionSphere;
    public DOTweenAnimation lineParticle;
    private float _ballTraveledTime = 0f;
    private GameObject _suckBall;
    private Vector3 _suckBallInitialScale;
    private float _matOpacity = 1;
    private Quaternion _ballEffectQuaternion;


    private enum State
    {
        In,
        Out,
        Suck,
    }

    private State _ballState;
    private SuckBallController _sbc;


    protected override void Awake()
    {
        base.Awake();
        _ballState = State.In;
        _suckBall = transform.GetChild(0).gameObject;
        _sbc = _suckBall.GetComponent<SuckBallController>();
        _suckBallInitialScale = new Vector3(_suckBall.transform.localScale.x, _suckBall.transform.localScale.y, _suckBall.transform.localScale.z);
        _ammo = WeaponDataStore.SuckGunDataStore.SuckGunMaxUseTimes;
        _ballEffectQuaternion = suckBallEffect.transform.rotation;

    }

    protected override void Update()
    {
        base.Update();
        if (_ballState == State.Out)
        {
            _suckBall.transform.position += Time.deltaTime * -1f * _suckBall.transform.right * WeaponDataStore.SuckGunDataStore.BallTravelSpeed;
            _ballTraveledTime += Time.deltaTime;
            if (_ballTraveledTime >= WeaponDataStore.SuckGunDataStore.MaxBallTravelTime)
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
                _ballState = State.In;
                // StartCoroutine(sucking(0.5f));
            }
        }

        /*if (_ballState == State.Suck)
        {
            _suckBall.GetComponent<SuckBallController>().MakeLineSicker();
        }
*/


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
                    _suckBall.SetActive(true);
                    suckBallEffect.transform.rotation = _ballEffectQuaternion;
                    suckBallEffect.GetComponent<DOTweenAnimation>().DOPauseAllById("Explode");
                    suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Create");
                    _matOpacity = 1;
                    _suckBall.transform.parent = null;
                    EventManager.Instance.TriggerEvent(new SuckGunFired(gameObject, Owner, Owner.GetComponent<PlayerController>().PlayerNumber));
                    break;
            }
        }
        else
        {
            // If player released RT
            if (_ballState == State.Out)
            {

                suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Suck");
                lineParticle.DORestart();
                lineParticle.GetComponent<ParticleSystem>().Play();
                DistortionSphere.Play();

                _sbc.RTText.SetActive(false);
                _ballState = State.Suck;
                StartCoroutine(sucking(0.1f));

            }
        }
    }

    IEnumerator sucking(float time)
    {
        List<GameObject> gos = _sbc.InRangePlayers;
        EventManager.Instance.TriggerEvent(new SuckGunSuck(gameObject, _suckBall, Owner, Owner.GetComponent<PlayerController>().PlayerNumber,
            gos));
        // First prototype: let's try adding a force to every object
        yield return new WaitForSeconds(time);

        foreach (GameObject go in gos)
        {
            Vector3 force = (_suckBall.transform.position + new Vector3(0, 2f, 0) - go.transform.position).normalized * WeaponDataStore.SuckGunDataStore.SuckStrength;
            go.GetComponent<IHittable>().OnImpact(force, ForceMode.Impulse, Owner, ImpactType.SuckGun);
        }
        yield return new WaitForSeconds(0.45f);
        foreach (var suckBallExplodeEffect in suckBallExplodeEffects)
        {
            suckBallExplodeEffect.GetComponent<ParticleSystem>().Play();
        }
        suckBallEffect.GetComponent<DOTweenAnimation>().DOPauseAllById("Suck");
        suckBallEffect.GetComponent<DOTweenAnimation>().DORestartById("Explode");
        yield return new WaitForSeconds(0.45f);
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
        _onWeaponUsedOnce();
    }

    public bool isSucking()
    {
        return _ballState == State.Suck;
    }

    protected override void _onWeaponDespawn()
    {
        _ballTraveledTime = 0f;
        _suckBall.transform.parent = transform;
        _suckBall.transform.localPosition = new Vector3(-0.468f, 0f);
        _suckBall.transform.localEulerAngles = Vector3.zero;
        _suckBall.transform.localScale = _suckBallInitialScale;
        _suckBall.SetActive(false);
        _ballState = State.In;
        _ammo = WeaponDataStore.SuckGunDataStore.SuckGunMaxUseTimes;
        EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
        // Need a little clean up the line renderer and stuff
        _sbc.CleanUpAll();
        gameObject.SetActive(false);
    }

    //private void _addToSuckedTimes()
    //{
    //	int playernum = GetComponent<GunPositionControl>().Owner.GetComponent<PlayerController>().PlayerNumber;
    //	GameManager.GM.SuckedPlayersTimes[playernum]++;
    //}
}
