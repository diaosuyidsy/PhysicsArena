using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
	public Rigidbody rb;
	public float Thrust;
	private float _fire;
	void Start ()
	{
		rb = GetComponent<Rigidbody>();
	}
	
	private bool StartToFire()
	{
		return Mathf.Approximately(_fire, 1f);
	}
	
	private void Update()
	{
		//float moveHorizontal = Input.GetAxis("Horizontal");
		//float moveVerticle = Input.GetAxis("Vertical");

		//Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVerticle);
		
		_fire = Input.GetAxis("XboxRT");
		_fire = Mathf.Approximately(_fire, 0f) || Mathf.Approximately(_fire, -1f) ? 0f : 1f;
		
		if (StartToFire())
		{
			rb.AddRelativeForce(Vector3.back * Thrust);
		}
	}
}
