using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class rtEmit : MonoBehaviour
{
    public GameObject WaterBall;
    public float Speed;

    public void Shoot (float TriggerVal)
    {
        WaterBall.GetComponent<ObiEmitter> ().speed = TriggerVal * Speed;
    }

}
