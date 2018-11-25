using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class rtEmit : MonoBehaviour
{
	public GameObject WaterBall;
	public float Speed;
	private bool isFire;

	// Update is called once per frame
	void Update () {
		Shoot();
	}

	private void Shoot()
	{
		float Fire = Input.GetAxis("XboxRT");
		Fire = Mathf.Approximately(Fire, 0f) || Mathf.Approximately(Fire, -1f) ? 0f : 1f;
		WaterBall.GetComponent<ObiEmitter>().speed = Fire * Speed;
		
	}

}
