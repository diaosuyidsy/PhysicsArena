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
    public int MaxAmmo = 1000;
    public int currentAmmo;

    private void Start()
    {
        currentAmmo = MaxAmmo;
    }

    public void Shoot(float TriggerVal)
    {
        if (Mathf.Approximately(TriggerVal, 0f) || currentAmmo <= 0)
        {
            WaterBall.GetComponent<ObiEmitter>().speed = 0f;
            return;
        }
        WaterBall.GetComponent<ObiEmitter>().speed = Speed;
        GunPositionControl gpc = GetComponent<GunPositionControl>();
        if (gpc != null)
        {
            Debug.Log("BackFire");
            gpc.Owner.GetComponent<Rigidbody>().AddForce(-gpc.Owner.transform.forward * BackFireThrust, ForceMode.Impulse);
            gpc.Owner.GetComponent<Rigidbody>().AddForce(gpc.Owner.transform.up * BackFireThrust * UpThrust, ForceMode.Impulse);
        }
        currentAmmo--;

    }

}
