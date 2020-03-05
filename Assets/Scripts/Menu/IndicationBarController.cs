using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IndicationBarController : MonoBehaviour
{
    public ComicMenu ComicMenu;
    public Image IndicationImage;
    public int MaxPlayers;
    public float ChargeTime = 1f;
    public float DeChargeTime = 0.5f;
    private int _playerInCircleCount
    {
        get
        {
            return _duckCount + _chickenCount;
        }
    }
    private int _duckCount
    {
        get
        {
            int result = 0;
            for (int i = 0; i < 3; i++)
            {
                if (_inTheCircle[i])
                {
                    result++;
                }
            }
            return result;
        }
    }
    private int _chickenCount
    {
        get
        {
            int result = 0;
            for (int i = 3; i < 6; i++)
            {
                if (_inTheCircle[i])
                {
                    result++;
                }
            }
            return result;
        }
    }
    private float _charge;
    private bool[] _inTheCircle;
    private bool _chargedUp;

    private void Awake()
    {
        _inTheCircle = new bool[6];
    }

    private void Update()
    {
        if (MaxPlayers == 0) return;
        if (_playerInCircleCount == MaxPlayers && _chickenCount != 0 && _duckCount != 0)
        {
            _addCharge(Time.deltaTime * (1f / ChargeTime));
            if (_charge >= 1f && !_chargedUp)
            {
                _chargedUp = true;
                ComicMenu.ChargedUp();
            }
        }
        else
        {
            _chargedUp = false;
            _addCharge(-Time.deltaTime * (1f / DeChargeTime));
        }
    }

    private void _addCharge(float charge)
    {
        _charge += charge;
        _charge = Mathf.Clamp(_charge, 0f, 1f);
        IndicationImage.fillAmount = _charge;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() && !_inTheCircle[other.transform.GetSiblingIndex()])
        {
            _inTheCircle[other.transform.GetSiblingIndex()] = true;
            transform.GetChild(other.transform.GetSiblingIndex()).GetComponent<DOTweenAnimation>().DOPlayForward();
            PlayerDoCircle();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() && _inTheCircle[other.transform.GetSiblingIndex()])
        {
            _inTheCircle[other.transform.GetSiblingIndex()] = false;
            transform.GetChild(other.transform.GetSiblingIndex()).GetComponent<DOTweenAnimation>().DOPlayBackwards();
            PlayerDoCircle();
        }
    }

    public void PlayerDoCircle()
    {
        ComicMenu.PlayerInCircleChange(_chickenCount, _duckCount);
    }
}
