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
	[Header("Winner")]
	public GameObject ChickensWin;
	public GameObject DucksWin;

	[Header("Time")] 
	public float TextToName = 2f;
	public float NameToRecord = 1.5f;
	
	[Header("Best Killer")]
	public GameObject MostKills;
	public GameObject MostKillsNameGM;
	public TextFxUGUI MostKillsNameFX;
	public GameObject KillRecordGM;
	public TextFxUGUI KillRecordFX;
	
	[Header("Best Suicider")]
	public GameObject MostSuicide;
	public GameObject MostSuicideNameGM;
	public TextFxUGUI MostSuicideNameFX;
	public GameObject SuicideRecordGM;	
	public TextFxUGUI SuicideRecordFX;

	[Header("Best Teammate-Killer")]
	public GameObject TMKiller;
	public GameObject TMKillerNameGM;
	public TextFxUGUI TMKillerNameFX;
	public GameObject TMKillerRecordGM;
	public TextFxUGUI TMKillerRecordFX;
	
	private int MostKillPlayer;
	private int MostSuicidePlayer;
	private int MostTMQPlayer;

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

	IEnumerator ShowKillerName()
	{
		yield return new WaitForSeconds(TextToName);
		
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

		MostKillsNameFX.SetText(GameManager.GM.PlayersInformation[MostKillPlayer].PlayerName);
		MostKillsNameFX.SetColour(GameManager.GM.PlayersInformation[MostKillPlayer].PlayerColor);
		
		MostKillsNameGM.SetActive(true);
		
		yield return new WaitForSeconds(NameToRecord);
		
		KillRecordFX.SetText("who killed " + GameManager.GM.KillRecord[MostKillPlayer] + " other birdies");
		KillRecordGM.SetActive(true);
	}
	

	public void DisplaySuicider()
	{
		MostSuicide.SetActive(true);

		StartCoroutine(ShowSuiciderName());
	}

	IEnumerator ShowSuiciderName()
	{
		yield return new WaitForSeconds(TextToName);
		
		var maxSuicideValue = Mathf.Max(GameManager.GM.SuicideRecord.ToArray());
		for (int tracker = 0; tracker < 6; tracker++)
		{
			if (GameManager.GM.SuicideRecord[tracker] == maxSuicideValue)
			{
				MostSuicidePlayer = tracker;
				break;
			}

		}
		
		MostSuicideNameFX.SetText(GameManager.GM.PlayersInformation[MostSuicidePlayer].PlayerName);
		MostSuicideNameFX.SetColour(GameManager.GM.PlayersInformation[MostSuicidePlayer].PlayerColor);
		
		MostSuicideNameGM.SetActive(true);
		
		yield return new WaitForSeconds(NameToRecord);
		
		SuicideRecordFX.SetText("who killed themselves for " + GameManager.GM.SuicideRecord[MostSuicidePlayer] + " times!");
		SuicideRecordGM.SetActive(true);
	}

	public void DisplayTMKiller()
	{
		TMKiller.SetActive(true);

		StartCoroutine(ShowTMKillerName());
	}

	IEnumerator ShowTMKillerName()
	{
		yield return new WaitForSeconds(TextToName);
		
		var maxTMQValue = Mathf.Max(GameManager.GM.TeammateMurderRecord.ToArray());
		for (int tracker = 0; tracker < 6; tracker++)
		{
			if (GameManager.GM.TeammateMurderRecord[tracker] == maxTMQValue)
			{
				MostTMQPlayer = tracker;
				break;
			}
		}
		
		TMKillerNameFX.SetText(GameManager.GM.PlayersInformation[MostTMQPlayer].PlayerName);
		TMKillerNameFX.SetColour(GameManager.GM.PlayersInformation[MostTMQPlayer].PlayerColor);
		
		TMKillerNameGM.SetActive(true);
		
		yield return new WaitForSeconds(NameToRecord);
		
		TMKillerRecordFX.SetText("who killed their teammates for " + GameManager.GM.TeammateMurderRecord[MostTMQPlayer] + " times!");
		TMKillerRecordGM.SetActive(true);
	}
}
