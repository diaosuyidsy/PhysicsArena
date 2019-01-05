﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtSuck : MonoBehaviour
{
    public float MaxBallTravelTime = 1f;
    public float BallTravelSpeed = 3f;

    private float _ballTraveledTime = 0f;
    private GameObject _suckBall;
    private bool _charged = false;
    private Vector3 _suckBallInitialScale;

    private enum State
    {
        In,
        Out,
        Suck,
    }

    private State _ballState;
    private SuckBallController _sbc;

    private void Start()
    {
        _ballState = State.In;
        _suckBall = transform.GetChild(0).gameObject;
        _sbc = _suckBall.GetComponent<SuckBallController>();
        _suckBallInitialScale = new Vector3(_suckBall.transform.localScale.x, _suckBall.transform.localScale.y, _suckBall.transform.localScale.z);
    }

    private void Update()
    {
        if (_ballState == State.Out)
        {
            _suckBall.transform.position += Time.deltaTime * _suckBall.transform.right * BallTravelSpeed;
        }
    }

    public void Suck(bool rtHolding)
    {
        // If player is holding RT (or pressed RT)
        if (rtHolding)
        {
            switch (_ballState)
            {
                case State.In:
                    _ballState = State.Out;
                    _suckBall.SetActive(true);
                    _suckBall.transform.parent = null;
                    _charged = false;
                    break;
                case State.Out:
                    if (_charged)
                    {
                        _charged = false;
                        _ballState = State.Suck;
                        StartCoroutine(sucking(0.5f));
                    }
                    break;
            }
        }
        else
        {
            // If player released RT
            if (_ballState == State.Out)
            {
                _charged = true;
            }
        }
    }

    IEnumerator sucking(float time)
    {
        List<GameObject> gos = _sbc.InRangePlayers;
        // First prototype: let's try adding a force to every object
        foreach (GameObject go in gos)
        {
            go.GetComponent<Rigidbody>().AddForce((_suckBall.transform.position - go.transform.position).normalized * 15000f);
        }
        yield return new WaitForSeconds(time);
        // After time, disable the suckball and return it to the original position,
        // reset ballstate;
        _suckBall.transform.parent = transform;
        _suckBall.transform.localPosition = Vector3.zero;
        _suckBall.transform.localEulerAngles = Vector3.zero;
        _suckBall.transform.localScale = _suckBallInitialScale;
        _suckBall.SetActive(false);
        _ballState = State.In;
        // Need a little clean up the line renderer and stuff
        _sbc.CleanUpAll();
    }

    public bool isSucking()
    {
        return _ballState == State.Suck;
    }
}
