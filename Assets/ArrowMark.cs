using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMark : MonoBehaviour
{
    public float HopInterval;
    public float HopDis;
    public float HopTime;


    private float HopTimer;
    private Vector3 StartPos;
    private Vector3 HopTarget;

    // Start is called before the first frame update
    void Start()
    {
        StartPos = transform.position;
        HopTarget = StartPos - transform.right * HopDis;
    }

    // Update is called once per frame
    void Update()
    {
        HopTimer += Time.deltaTime;

        if(HopTimer <= HopTime / 2)
        {
            transform.position = Vector3.Lerp(StartPos, HopTarget, HopTimer / (HopTime / 2));
        }
        else if(HopTimer <= HopTime)
        {
            transform.position = Vector3.Lerp(HopTarget, StartPos, (HopTimer-(HopTime/2)) / (HopTime / 2));
        }
        else if(HopTimer <= HopTime + HopInterval)
        {
            transform.position = StartPos;
        }
        else
        {
            HopTimer = 0;
        }

    }
}
