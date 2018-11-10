using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerrr : MonoBehaviour
{

	private Rigidbody rb;

	public float speed;
	
	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	private void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVerticle = Input.GetAxis("Vertical");
		
		Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVerticle);
		
		rb.AddForce(movement * speed);
	}
}
