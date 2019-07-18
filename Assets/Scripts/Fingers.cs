using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Fingers : MonoBehaviour
{
	public float Force = 4000;
	public GameObject OtherHand;
	public GameObject Hip;
	public float GunVerticalOffset = 0.087f;
	Rigidbody rb;

	[HideInInspector]
	public bool taken = true;
	private PlayerController _pc;

	// Use this for initialization
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		_pc = Hip.GetComponent<PlayerController>();
		Debug.Assert(_pc != null);
		taken = true;
	}

	public void Throw()
	{
		SpringJoint thisSJ = GetComponent<SpringJoint>();
		if (thisSJ != null)
		{
			thisSJ.connectedBody = null;
			Destroy(thisSJ);
		}
	}

	private void Update()
	{
		if (taken) return;
		RaycastHit hit;
		if (Physics.SphereCast(transform.position, _pc.CharacterDataStore.CharacterPickUpDataStore.Radius, transform.forward, out hit, 0.1f, _pc.CharacterDataStore.CharacterPickUpDataStore.PickUpLayer))
		{
			if (!taken && hit.collider.GetComponent<GunPositionControl>().CanBePickedUp)
			{
				// Tell other necessary components that it has taken something
				OtherHand.GetComponent<Fingers>().taken = true;
				//Hip.GetComponent<PlayerController>().HandTaken = true;
				GetComponentInParent<PlayerController>().HandObject = hit.collider.gameObject;

				// Tell the collected weapon who picked it up
				hit.collider.GetComponent<GunPositionControl>().Owner = Hip;
				hit.collider.GetComponent<Rigidbody>().isKinematic = true;
				hit.collider.gameObject.layer = gameObject.layer;
				// Change the Gun's vertical offset according to duck or chicken
				hit.collider.GetComponent<GunPositionControl>().VerticalOffset = GunVerticalOffset;

				PickUpItem(hit.collider.tag);
				taken = true;
			}
		}
	}

	void OnJointBreak()
	{
		taken = false;

	}

	void PickUpItem(string _tag)
	{
		GetComponentInParent<PlayerController2>().OnPickUpItem(_tag);
	}

	public void SetTaken(bool _taken)
	{
		taken = _taken;
	}

}
