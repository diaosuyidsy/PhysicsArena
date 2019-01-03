using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtStamp : MonoBehaviour
{
    public float Thrust = 300f;

    [HideInInspector]
    public bool Charged = false;
    private bool stampedSomeone = false;

    public void Stamp()
    {
        if (!Charged) return;
        Charged = false;
        stampedSomeone = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        // If not in the stamping mode or did not hit any "Player body"
        // Do nothing
        if (GameManager.GM.AllPlayers != (GameManager.GM.AllPlayers | (1 << other.gameObject.layer)))
            return;
        if (stampedSomeone) return;
        stampedSomeone = true;
        print("collided");
        // Else perform similar logic as the melee
        float velocityAddon = transform.GetComponent<Rigidbody>().velocity.magnitude * 1.5f;
        velocityAddon = velocityAddon > 1.4f ? 1.4f : velocityAddon;
        other.SendMessageUpwards("OnMeleeHit", transform.forward * Thrust);
    }

}
