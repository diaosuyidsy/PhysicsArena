using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class CarPath : MonoBehaviour
{
	public Transform[] target;
	public float speed;
	private int current = 5;
	private int dir;
	
	void OnTriggerStay(Collider other)
	{
		if (current == target.Length)
		{
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			Debug.Log("Team 1 Wins");
		}
		if (current == 0)
		{
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			Debug.Log("Team 2 Wins");
		}

		//if (other.CompareTag("Team1") && other.CompareTag("Team2")) 
		//{
			//GetComponent<Rigidbody>().velocity = Vector3.zero;
		//}
		
		else if (other.CompareTag("Team1") && current < target.Length)
		{
			if (transform.position != target[current].position)
			{   if(dir == 2)
				{
					current = current + 1;
				}
				dir = 1;
				Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
				GetComponent<Rigidbody>().MovePosition(pos);
				Vector3 relativePos = target[current].position - transform.position;
				transform.rotation = Quaternion.LookRotation (relativePos);
			}
			else current = current + 1;
		}

		else if (other.CompareTag("Team2") && current >= 0)
		{
			if (transform.position != target[current].position)
			{   
				if(dir == 1)
				{
					current = current - 1;
				}
				dir = 2;
				Vector3 pos = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
				GetComponent<Rigidbody>().MovePosition(pos);
				Vector3 relativePos = target[current].position - transform.position;
				transform.rotation = Quaternion.LookRotation (relativePos);
			}
			else current = current - 1;	
		}	
	}
}
