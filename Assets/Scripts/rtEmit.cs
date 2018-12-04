﻿using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class rtEmit : MonoBehaviour
{
    public ObiEmitter WaterBall;
    public GameObject WaterUI;
    public float Speed;
    public float BackFireThrust;
    public float UpThrust = 1f;
    public int MaxAmmo = 1000;
    public int currentAmmo;
    public float ShootMaxCD = 0.3f;

    private float _shootCD = 0f;
    private GunPositionControl _gpc;

    private void Start ()
    {
        currentAmmo = MaxAmmo;
        _gpc = GetComponent<GunPositionControl> ();
    }

    // This function is called by PlayerController, when player is holding a gun
    // And it's holding down RT
    public void Shoot (float TriggerVal)
    {
        //For inspection purpose
        if (currentAmmo <= 0)
        {
            print ("NoAmmo");
        }

        // If player was holding down the RT button
        // CD will add up
        // If CD >= MaxCD, nothing works, only releasing the RT will replenish the CD
        if (Mathf.Approximately (TriggerVal, 1f))
        {
            _shootCD += Time.deltaTime;
            if (_shootCD >= ShootMaxCD)
            {
                WaterBall.speed = 0f;
                return;
            }
        }

        if (Mathf.Approximately (TriggerVal, 0f) || currentAmmo <= 0)
        {
            // Need to reset shoot CD if player has released RT
            _shootCD = 0f;
            WaterBall.speed = 0f;
            return;
        }
        WaterBall.speed = Speed;
        if (_gpc != null)
        {
            _gpc.Owner.GetComponent<Rigidbody> ().AddForce (-_gpc.Owner.transform.forward * BackFireThrust, ForceMode.Impulse);
            _gpc.Owner.GetComponent<Rigidbody> ().AddForce (_gpc.Owner.transform.up * BackFireThrust * UpThrust, ForceMode.Impulse);
        }
        currentAmmo--;
        // If we changed ammo, then need to change UI as well
        ChangeAmmoUI ();
    }

    private void ChangeAmmoUI ()
    {
        float scaleY = currentAmmo * 1.0f / MaxAmmo;
        WaterUI.transform.localScale = new Vector3 (1f, scaleY, 1f);
    }

    // If weapon collide to the ground, and has no ammo, then despawn it
    private void OnCollisionEnter (Collision other)
    {
        if (other.collider.CompareTag ("Ground") && currentAmmo == 0)
        {
            currentAmmo = MaxAmmo;
            //GameManager.GM.HideWeapon (gameObject);
            gameObject.SetActive (false);
        }
    }

    // If the weapon is taken down the death zone, then despawn it
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("DeathZone"))
        {
            currentAmmo = MaxAmmo;
            //GameManager.GM.HideWeapon (gameObject);
            gameObject.SetActive (false);
        }
    }

}
