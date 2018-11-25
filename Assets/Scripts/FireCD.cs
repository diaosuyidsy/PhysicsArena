using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCD : MonoBehaviour
{

    public float cooldownTime;
    
    private float nextFireTime;
    private float _fire;

    private void Update()
    {
        _fire = Input.GetAxis("XboxRT");
        _fire = Mathf.Approximately(_fire, 0f) || Mathf.Approximately(_fire, -1f) ? 0f : 1f;
        
        if (Time.time > nextFireTime)
        {
            
            if (StartToFire())
            {
                print("Cooldown started");
                nextFireTime = Time.time + cooldownTime;
            }
        }   
    }

    private bool StartToFire()
    {
        return Mathf.Approximately(_fire, 1f);
    }
}
