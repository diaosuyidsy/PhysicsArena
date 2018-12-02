using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour {
	
	public float Cooldown;
	
	public GameObject Player1Body;
	public GameObject Player1UI;
	[SerializeField] private Transform player1Hip;
	[SerializeField] private Transform respawnPoint1;
	
	public GameObject Player2Body;
	public GameObject Player2UI;
	[SerializeField] private Transform player2Hip;
	[SerializeField] private Transform respawnPoint2;
	

	/*private void Start()
	{
		Player1Body.SetActive(true);
		Player2Body.SetActive(true);
	}*/

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Team1"))
		{
			player1Hip.transform.position = respawnPoint1.transform.position;
			Player1Body.SetActive(false);
			Player1UI.SetActive(false);
			StartCoroutine(Respawn1());
		}
		else if (other.CompareTag("Team2"))
		{
			player2Hip.transform.position = respawnPoint2.transform.position;
			Player2Body.SetActive(false);
			Player2UI.SetActive(false);
			StartCoroutine(Respawn2());
		}
		/*player3.transform.position = respawnPoint3.transform.position;
		player4.transform.position = respawnPoint4.transform.position;
		player5.transform.position = respawnPoint5.transform.position;
		player6.transform.position = respawnPoint6.transform.position;*/
	}

	IEnumerator Respawn1()
	{
		yield return new WaitForSeconds(Cooldown);
		Player1Body.SetActive(true);
		Player1UI.SetActive(true);
	}
	IEnumerator Respawn2()
	{
		yield return new WaitForSeconds(Cooldown);
		Player2Body.SetActive(true);
		Player2UI.SetActive(true);
	}
	// Use this for initialization
	/*void Start () {
		//If you want, add this line:
		Death = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(Death == true) {
			//If my enemy is death, a timer will start.
			Timer += Time.deltaTime; 
		}
		//If the timer is bigger than cooldown.
		if(Timer >= Cooldown) {
			//It will create a new Player, at this position.
			OnTriggerEnter();
			//My enemy won't be dead anymore.
			Death = false;
			//Timer will restart.
			Timer = 0;
		}
	}*/
}
