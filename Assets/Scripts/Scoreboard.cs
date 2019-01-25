using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using TextFx;
using Unity.Collections;

public class Scoreboard : MonoBehaviour
{
	public GameObject ChickensWin;
	public GameObject DucksWin;
	public GameObject MostKills;
	public Text MostKillsName;
	public GameObject MostKillsNameGM;
	public GameObject KillRecord;
	public GameObject MostSuicide;
	public Text MostSuicideName;
	public GameObject MostSuicideNameGM;
	public GameObject SuicideRecord;
	
	private int MostKillPlayer;
	private int MostSuicidePlayer;

	public void DisplayWinner()
	{
		if (GameManager.GM.Winner == 1)
		{
			ChickensWin.SetActive(true);
		}
		else
		{
			DucksWin.SetActive(true);
		}			
	}

	public void DisplayKiller()
	{
		MostKills.SetActive(true);

		StartCoroutine(ShowKillerName());
	}

	public IEnumerator ShowKillerName()
	{
		yield return new WaitForSeconds(2f);
		
		// Find player with most kills.
		var maxKillValue = Mathf.Max(GameManager.GM.KillRecord.ToArray());

		for (int tracker = 0; tracker < 6; tracker++)
		{
			if (GameManager.GM.KillRecord[tracker] == maxKillValue)
			{
				MostKillPlayer = tracker;
				break;
			}
		}
		
		if (MostKillPlayer == 0)
		{
			MostKillsName.text = "Player 0";
		}
		else if (MostKillPlayer == 1)
		{
			MostKillsName.text = "Player 1";
		}
		else if (MostKillPlayer == 2)
		{
			MostKillsName.text = "Player 2";
		}
		else if (MostKillPlayer == 3)
		{
			MostKillsName.text = "Player 3";
		}
		else if (MostKillPlayer == 4)
		{
			MostKillsName.text = "Player 4";
		}
		else if (MostKillPlayer == 5)
		{
			MostKillsName.text = "Player 5";
		}
		
		MostKillsNameGM.SetActive(true);
		
		yield return new WaitForSeconds(1.5f);
		
		KillRecord.SetActive(true);
	}
	

	public void DisplaySuicider()
	{
		MostSuicide.SetActive(true);

		StartCoroutine(ShowSuiciderName());
	}

	IEnumerator ShowSuiciderName()
	{
		yield return new WaitForSeconds(2f);
		
		var maxSuicideValue = Mathf.Max(GameManager.GM.SuicideRecord.ToArray());
		for (int tracker = 0; tracker < 6; tracker++)
		{
			if (GameManager.GM.SuicideRecord[tracker] == maxSuicideValue)
			{
				MostSuicidePlayer = tracker;
				break;
			}

		}
		if (MostSuicidePlayer == 0)
		{
			MostSuicideName.text = "Player 0";
		}
		else if (MostSuicidePlayer == 1)
		{
			MostSuicideName.text = "Player 1";
		}
		else if (MostSuicidePlayer == 2)
		{
			MostSuicideName.text = "Player 2";
		}
		else if (MostSuicidePlayer == 3)
		{
			MostSuicideName.text = "Player 3";
		}
		else if (MostSuicidePlayer == 4)
		{
			MostSuicideName.text = "Player 4";
		}
		else if (MostSuicidePlayer == 5)
		{
			MostSuicideName.text = "Player 5";
		}
		
		MostSuicideNameGM.SetActive(true);
	}
}
