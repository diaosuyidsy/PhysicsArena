using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowArea : MonoBehaviour
{
	[Range(0f, 1f)]
	public float SlowAmount;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<PlayerController>() != null)
			other.gameObject.GetComponent<PlayerController>().OnImpact(new PermaSlowEffect(0f, SlowAmount));
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.GetComponent<PlayerController>() != null)
			other.gameObject.GetComponent<PlayerController>().OnImpact(new RemovePermaSlowEffect(0f, 0f));
	}
}
