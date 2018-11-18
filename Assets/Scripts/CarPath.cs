using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class CarPath : MonoBehaviour
{
	public Transform[] target;
	public float speed;
	public int current;
	private int dir;
	private bool ending = false;
	
	void OnTriggerStay(Collider other)
	{
		if(ending)
			return;
		// When the car gets to Team 1's home, it stops and print out "Team 1 wins."
		if (current == target.Length)
		{
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			ending = true;
			Debug.Log("Team 1 Wins");
		}
		// When the car gets to Team 2's home, it stops and print out "Team 2 wins."
		if (current == -1)
		{
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			ending = true;
			Debug.Log("Team 2 Wins");
		}
		// When Team 1 player enters trigger, move the car toward next element in array.
		else if (other.CompareTag("Team1") && current < target.Length)
		{
			if (transform.position != target[current].position)
			{   // If wrong direction, change [current].
				if(dir == 2)
				{
					current ++;
				}
				dir = 1;			
				// Rotate car facing target location.			
				Vector3 relativePos = target[current].position - transform.position;
				transform.rotation = Quaternion.LookRotation (relativePos);
				// Move car toward target location.
				Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
				GetComponent<Rigidbody>().MovePosition(pos);
				//Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
				//GetComponent<Rigidbody>().MovePosition(pos);
			}
			else current ++;
		}
		// When Team 2 player enters trigger, move the car toward previous element in array.
		else if (other.CompareTag("Team2") && current >= 0)
		{
			if (transform.position != target[current].position)
			{// If wrong direction, change [current]   
				if(dir == 1)
				{
					current --;
				}
				dir = 2;
				// Rotate car facing target location.
				Vector3 relativePos = target[current].position - transform.position;
				transform.rotation = Quaternion.LookRotation (relativePos);
				// Move car toward target location.
				Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
				GetComponent<Rigidbody>().MovePosition(pos);
			}
			else current --;
		}	
	}
}
