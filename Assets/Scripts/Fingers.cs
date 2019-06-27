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
	private string[] _playercanpickuptags;
	private PlayerController _pc;

	// Use this for initialization
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		_playercanpickuptags = GameManager.GM.PlayerCanPickupTags;
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
		RaycastHit hit;
		if (Physics.SphereCast(transform.position, _pc.CharacterDataStore.CharacterPickUpDataStore.Radius, transform.forward, out hit, 0.1f, _pc.CharacterDataStore.CharacterPickUpDataStore.PickUpLayer))
		{
			print(hit.collider.name);
			if (!taken && hit.collider.GetComponent<GunPositionControl>().CanBePickedUp)
			{
				// Tell other necessary components that it has taken something
				OtherHand.GetComponent<Fingers>().taken = true;
				Hip.GetComponent<PlayerController>().HandTaken = true;
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

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, _pc.CharacterDataStore.CharacterPickUpDataStore.Radius);
	}

	//void OnCollisionEnter(Collision col)
	//{
	//	// Make it so that you cannot stick to the ground, payload or any other players
	//	if (GameManager.GM.AllPlayers == (GameManager.GM.AllPlayers | (1 << col.collider.gameObject.layer)) || !_playercanpickuptags.Contains(col.collider.tag))
	//		return;

	//	if (!taken && col.collider.GetComponent<GunPositionControl>().CanBePickedUp)
	//	{
	//		// Tell other necessary components that it has taken something
	//		OtherHand.GetComponent<Fingers>().taken = true;
	//		Hip.GetComponent<PlayerController>().HandTaken = true;
	//		GetComponentInParent<PlayerController>().HandObject = col.gameObject;

	//		// Tell the collected weapon who picked it up
	//		col.collider.GetComponent<GunPositionControl>().Owner = Hip;
	//		col.collider.GetComponent<Rigidbody>().isKinematic = true;
	//		col.collider.gameObject.layer = gameObject.layer;
	//		// Change the Gun's vertical offset according to duck or chicken
	//		col.collider.GetComponent<GunPositionControl>().VerticalOffset = GunVerticalOffset;

	//		PickUpItem(col.collider.tag);
	//		taken = true;
	//	}
	//}

	void OnJointBreak()
	{
		taken = false;

	}

	void PickUpItem(string _tag)
	{
		GetComponentInParent<PlayerController>().OnPickUpItem(_tag);
	}

	public void SetTaken(bool _taken)
	{
		taken = _taken;
	}

}
