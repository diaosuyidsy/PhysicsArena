using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtSuck : MonoBehaviour
{
    public float MaxBallTravelTime = 1f;
    public float BallTravelSpeed = 3f;

    private float _ballTraveledTime = 0f;
    private GameObject _suckBall;
    private bool _charged = false;
    private enum State
    {
        In,
        Out,
        Suck
    }

    private State _ballState;
    private SuckBallController _sbc;

    private void Start()
    {
        _ballState = State.In;
        _suckBall = transform.GetChild(0).gameObject;
        _sbc = _suckBall.GetComponent<SuckBallController>();
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
}
