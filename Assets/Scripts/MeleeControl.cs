using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeControl : MonoBehaviour
{
    public float Thrust = 100f;
    public PlayerController pc;

    private void OnCollisionEnter (Collision collision)
    {
        if (GameManager.GM.AllPlayers != (GameManager.GM.AllPlayers | (1 << collision.collider.gameObject.layer)))
            return;

        if (pc != null && pc.IsPunching)
        {
            Debug.Log ("Hit : " + pc.MeleeCharge);
            pc.IsPunching = false;
            collision.collider.SendMessageUpwards ("OnMeleeHit", transform.forward * Thrust * pc.MeleeCharge);
        }
    }
}
