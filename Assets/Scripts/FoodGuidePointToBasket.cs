using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGuidePointToBasket : MonoBehaviour
{
    public TeamNum Team;
    private Transform _targetBasket;
    private void Awake()
    {
        if (Team == TeamNum.Team1)
        {
            _targetBasket = GameObject.Find("LForceField").transform;
        }
        else
        {
            _targetBasket = GameObject.Find("RForceField").transform;
        }

        /*if (Team == TeamNum.Team1)
        {
            _targetBasket = GameObject.Find("FoodBasketTEAM1").transform;
        }
        else
        {
            _targetBasket = GameObject.Find("FoodBasketTEAM2").transform;
        }*/
    }
    void Update()
    {
        Vector3 lookPos = _targetBasket.position - transform.position;
        lookPos.y = 0f;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = rotation;
    }
}
