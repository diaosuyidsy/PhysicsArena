using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegControl : MonoBehaviour
{
	public PlayerController Hip;
	public float WalkThrust = 50f;
	private bool _walked = false;

	private void OnCollisionEnter(Collision collision)
	{
		if (_walked)
			return;
		_walked = true;
		//if (Hip.IsOccupied)
		//    Hip.ApplyWalkForce(WalkThrust);
	}

	private void OnCollisionExit(Collision collision)
	{
		_walked = false;
	}

}
