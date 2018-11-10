using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
	public GameObject MoveCar;
	public float xspeed = 3f;
	public float zspeed;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		 
	}
	
	void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("Team1"))
		{
			MoveCar.transform.Translate(xspeed, 0, zspeed);
		}
		
		if (other.CompareTag("Team2"))
		{
			MoveCar.transform.Translate(-xspeed, 0, -zspeed);
		}
	}
	
}
