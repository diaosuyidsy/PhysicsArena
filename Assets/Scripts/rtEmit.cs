using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class rtEmit : MonoBehaviour
{
    public GameObject WaterBall;
    public float Speed;
    public float BackFireThrust;
    public float UpThrust = 1f;

    public void Shoot (float TriggerVal)
    {
        WaterBall.GetComponent<ObiEmitter> ().speed = TriggerVal * Speed;
        GunPositionControl gpc = GetComponent<GunPositionControl> ();
        if (gpc != null)
        {
            gpc.Owner.GetComponent<Rigidbody> ().AddForce (-gpc.Owner.transform.forward * BackFireThrust * TriggerVal, ForceMode.Impulse);
            gpc.Owner.GetComponent<Rigidbody> ().AddForce (gpc.Owner.transform.up * BackFireThrust * TriggerVal * UpThrust, ForceMode.Impulse);

        }
    }

}
