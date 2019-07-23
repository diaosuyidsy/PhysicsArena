using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowArea : MonoBehaviour
{
	[Range(0f, 1f)]
	public float SlowAmount;

	private SlowEffect _se;

	private void Awake()
	{
		_se = new SlowEffect(50f, SlowAmount);
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.GetComponent<PlayerController>() != null)
		{
			other.gameObject.GetComponent<PlayerController>().OnImpact(_se);
			print("Hello");
		}
	}

	private void OnTriggerExit(Collider other)
	{

	}
}
