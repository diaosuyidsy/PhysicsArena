using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using TextFx;
using Unity.Collections;
using GUIText = Rewired.Internal.GUIText;

public class Scoreboard : MonoBehaviour
{
	public GameObject ChickensWin;
	public GameObject DucksWin;
	
	public GameObject MostKills;
	public GameObject MostKillsNameGM;
	public TextFxUGUI MostKillNameFx;
	public GameObject KillRecord;
	public TextFxUGUI KillRecordFx;
	
	public GameObject MostSuicide;
	public TextFxUGUI MostSuicideNameFx;
	public GameObject MostSuicideNameGM;
	public GameObject SuicideRecord;
	public TextFxUGUI SuicideRecordFx;
	
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
		
		//MostKillsName.text = GameManager.GM.PlayersInformation[MostKillPlayer].PlayerName;

		MostKillNameFx.SetText(GameManager.GM.PlayersInformation[MostKillPlayer].PlayerName);
		MostKillNameFx.SetColour(GameManager.GM.PlayersInformation[MostKillPlayer].PlayerColor);
		
		MostKillsNameGM.SetActive(true);
		
		yield return new WaitForSeconds(1.5f);
		
		KillRecordFx.SetText("who killed " + GameManager.GM.KillRecord[MostKillPlayer] + " other birdies");
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
		
		MostSuicideNameFx.SetText(GameManager.GM.PlayersInformation[MostSuicidePlayer].PlayerName);
		MostSuicideNameFx.SetColour(GameManager.GM.PlayersInformation[MostSuicidePlayer].PlayerColor);
		
		MostSuicideNameGM.SetActive(true);
		
		yield return new WaitForSeconds(1.5f);
		
		SuicideRecordFx.SetText("who killed themselves for " + GameManager.GM.SuicideRecord[MostSuicidePlayer] + " times!");
		SuicideRecord.SetActive(true);
	}
}
