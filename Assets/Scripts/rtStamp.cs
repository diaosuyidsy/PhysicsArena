using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtStamp : MonoBehaviour
{
    public float Thrust = 300f;

    private bool Charged = false;
    // stampedSomeone == int
    private bool stampedSomeone = true;

    public void Stamp(bool hold)
    {
        if (!hold)
        {
            Charged = true;
            stampedSomeone = true;
            return;
        }
        if (hold && !Charged) return;
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
        // Else perform similar logic as the melee
        float velocityAddon = transform.GetComponent<Rigidbody>().velocity.magnitude * 1.5f;
        velocityAddon = velocityAddon > 1.4f ? 1.4f : velocityAddon;
        other.GetComponentInParent<PlayerController>().OnMeleeHit(-transform.right * Thrust);
        //other.SendMessageUpwards("OnMeleeHit", -transform.right * Thrust);
    }

}
