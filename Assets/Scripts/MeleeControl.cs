using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeControl : MonoBehaviour
{
	//public float Thrust = 100f;
	//public PlayerController pc;

	//private void OnCollisionEnter(Collision collision)
	//{
	//    if (DesignPanelManager.DPM.MeleeAlternateSchemaToggle.isOn)
	//    {
	//        return;
	//    }
	//    if (GameManager.GM.AllPlayers != (GameManager.GM.AllPlayers | (1 << collision.collider.gameObject.layer)))
	//        return;

	//    if (pc != null && pc.IsPunching)
	//    {
	//        pc.IsPunching = false;
	//        float velocityAddon = transform.GetComponent<Rigidbody>().velocity.magnitude * 1.5f;
	//        velocityAddon = velocityAddon > 1.4f ? 1.4f : velocityAddon;
	//        collision.collider.GetComponentInParent<PlayerController>().OnMeleeHit(transform.forward * Thrust * pc.MeleeCharge * velocityAddon, pc.gameObject);
	//        collision.collider.GetComponentInParent<PlayerController>().Mark(pc.gameObject);
	//    }
	//}
}
