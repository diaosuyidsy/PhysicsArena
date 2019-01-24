using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
	public GameObject ChickensWin;
	public GameObject DucksWin;
	public GameObject MostKills;
	public Text MostKillsName;
	public GameObject MostSuicide;
	public Text MostSuicideName;
	private int MostKillPlayer;
	private int MostSuicidePlayer;

	public void Update()
	{
		//if (GameManager.GM.Winner == 1)
		//{
		//	ChickensWin.SetActive(true);
		//}
		//else
		//{
		//	DucksWin.SetActive(true);
		//}
		
		MostKills.SetActive(true);
		
		DisplayKillerName();
		
		MostSuicide.SetActive(true);
		
		DisplaySuiciderName();
			
	}
	private void DisplayKillerName()
	{
		var maxKillValue = Mathf.Max(GameManager.GM.KillRecord.ToArray());
		for (int tracker = 0; tracker < 6; tracker++)
		{
			if (GameManager.GM.KillRecord[tracker] == maxKillValue)
			{
				MostKillPlayer = tracker;
			}
			else
			{
				MostKillPlayer = MostKillPlayer + 1;
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
	}

	private void DisplaySuiciderName()
	{
		var maxSuicideValue = Mathf.Max(GameManager.GM.SuicideRecord.ToArray());
		for (int tracker = 0; tracker < 6; tracker++)
		{
			if (GameManager.GM.SuicideRecord[tracker] == maxSuicideValue)
			{
				MostSuicidePlayer = tracker;
			}
			else
			{
				MostSuicidePlayer = MostSuicidePlayer + 1;
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
	}
}
