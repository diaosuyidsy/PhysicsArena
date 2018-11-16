using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class CarPath : MonoBehaviour
{

	public Transform[] target;
	public float speed;

	private int current = 5;
	
	void OnTriggerStay(Collider other)
	{
		if (current == target.Length)
		{
			Debug.Log("Team 1 Wins");
		}
		
		if (other.CompareTag("Team1") && current < target.Length)
		{
			if (transform.position != target[current].position)
			{
				Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
				GetComponent<Rigidbody>().MovePosition(pos);
			}
			else current = current + 1;

		}

		if (current == 0)
		{
			Debug.Log("Team 2 Wins");
		}
		
		if (other.CompareTag("Team2") && current > 0)
		{
			if (transform.position != target[current].position)
			{
				Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
				GetComponent<Rigidbody>().MovePosition(pos);
			}
			else current = current - 1;
			
		}	
	}
}
