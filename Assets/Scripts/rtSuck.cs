using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtSuck : MonoBehaviour
{
    public float MaxBallTravelTime = 1f;
    public float BallTravelSpeed = 3f;

    private float _ballTraveledTime = 0f;
    private GameObject _suckBall;
    private enum State
    {
        In,
        Out,
        Suck
    }

    private State _ballState;

    private void Start()
    {
        _ballState = State.In;
        _suckBall = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if (_ballState == State.Out)
        {
            _suckBall.transform.position += Time.deltaTime * _suckBall.transform.right * BallTravelSpeed;
        }
    }

    public void Suck(bool holding)
    {

    }
}
