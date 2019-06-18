using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Team1") || other.CompareTag("Team2"))
		{
			EventManager.Instance.TriggerEvent(new PlayerDied(other.gameObject, other.GetComponent<PlayerController>().PlayerNumber));
		}
		else if (other.CompareTag("Team1Resource"))
		{
			GameManager.GM.Team1ResourceSpawnIndex = (GameManager.GM.Team1ResourceSpawnIndex + 1) % GameManager.GM.Team1ResourceRespawnPoints.Length;
			other.transform.position = GameManager.GM.Team1ResourceRespawnPoints[GameManager.GM.Team1ResourceSpawnIndex].transform.position;
			EventManager.Instance.TriggerEvent(new ObjectDespawned(other.gameObject));
		}
		else if (other.CompareTag("Team2Resource"))
		{
			GameManager.GM.Team2ResourceSpawnIndex = (GameManager.GM.Team2ResourceSpawnIndex + 1) % GameManager.GM.Team2ResrouceRespawnPoints.Length;
			other.transform.position = GameManager.GM.Team2ResrouceRespawnPoints[GameManager.GM.Team2ResourceSpawnIndex].transform.position;
			EventManager.Instance.TriggerEvent(new ObjectDespawned(other.gameObject));
		}
		else if (other.CompareTag("Weapon") || other.CompareTag("Hook") || other.CompareTag("SuckGun"))
		{
			EventManager.Instance.TriggerEvent(new ObjectDespawned(other.gameObject));
		}
	}
}
