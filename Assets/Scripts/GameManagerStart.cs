using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerStart : MonoBehaviour
{
	public static GameManagerStart GMS;
	public GameObject WinningObj;
	public GameObject Payload;
	public GameObject Or;
	public GameObject Food;

	private void Awake()
	{
		GMS = this;
	}

	private IEnumerator PlayIntro()
	{
		yield return new WaitForSeconds(1f);
		
		WinningObj.SetActive(true);
		
		yield return new WaitForSeconds(5f);
		
		Payload.SetActive(true);
		
		yield return new WaitForSeconds(7f);
		
		Or.SetActive(true);
		
		yield return new WaitForSeconds(3f);
		
		Food.SetActive(true);
		
		yield return new WaitForSeconds(3f);

		SceneManager.LoadSceneAsync(1);
	}

	public void StartIntro()
	{
		StartCoroutine(PlayIntro());
	}
}
