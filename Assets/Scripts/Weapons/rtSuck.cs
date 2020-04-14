using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class rtSuck : WeaponBase
{
    public GameObject suckBallEffect;
    public DOTweenAnimation lineParticle;
    public DOTweenAnimation blackHoleEffect;
    public ParticleSystem disappearEffect;
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
    private SuckGunData _suckGunData;

    protected override void Awake()
    {
        base.Awake();
        _suckGunData = WeaponDataBase as SuckGunData;
        _ballState = State.In;
        _suckBall = transform.GetChild(0).gameObject;
        _sbc = _suckBall.GetComponent<SuckBallController>();
        _suckBallInitialScale = new Vector3(_suckBall.transform.localScale.x, _suckBall.transform.localScale.y, _suckBall.transform.localScale.z);
        _ammo = _suckGunData.SuckGunMaxUseTimes;
        _ballEffectQuaternion = suckBallEffect.transform.rotation;
        blackHoleEffect.gameObject.SetActive(false);

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
                blackHoleEffect.DORestart();
                blackHoleEffect.gameObject.SetActive(true);

                _ballState = State.Suck;
                StartCoroutine(sucking());

            }
        }
    }

    IEnumerator sucking()
    {
        List<GameObject> gos = _sbc.InRangePlayers;
        EventManager.Instance.TriggerEvent(new SuckGunSuck(gameObject, _suckBall, Owner, Owner.GetComponent<PlayerController>().PlayerNumber,
            gos));
        // First prototype: let's try adding a force to every object
        foreach (GameObject go in gos)
        {
            Vector3 forceRawDir = _suckBall.transform.position - go.transform.position;
            forceRawDir.y = 0f;
            forceRawDir.Normalize();
            Vector3 force = (forceRawDir + Vector3.up * _suckGunData.SuckUpStrengthMultiplier) * _suckGunData.SuckStrength;
            go.GetComponent<IHittable>().SetVelocity(Vector3.zero);
            go.GetComponent<IHittable>().OnImpact(force, ForceMode.VelocityChange, Owner, ImpactType.SuckGun);
        }
        yield return new WaitForSeconds(0.25f);
        blackHoleEffect.gameObject.SetActive(false);
        
        disappearEffect.Play();
        yield return new WaitForSeconds(0.25f);
        yield return new WaitForSeconds(_suckGunData.SuckBallStayUpTime);
        ////Second prototype
        //yield return StartCoroutine(Congregate(time, gos));
        // After time, disable the suckball and return it to the original position,
        // reset ballstate;
        suckBallEffect.GetComponent<DOTweenAnimation>().DOPauseAllById("Suck");
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
        _ammo = _suckGunData.SuckGunMaxUseTimes;
        // Need a little clean up the line renderer and stuff
        _sbc.CleanUpAll();
        base._onWeaponDespawn();
    }

    //private void _addToSuckedTimes()
    //{
    //	int playernum = GetComponent<GunPositionControl>().Owner.GetComponent<PlayerController>().PlayerNumber;
    //	GameManager.GM.SuckedPlayersTimes[playernum]++;
    //}
}
