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

	[Header("Block Master")]
	public GameObject Block;
	public GameObject BlockNameGM;
	public TextFxUGUI BlockNameFX;
	public GameObject BlockRecordGM;
	public TextFxUGUI BlockRecordFX;

	[Header("Summary")]
	public GameObject ScoreBoard;
	public GameObject KillMasterSummary;
	public GameObject SuicideMasterSummary;
	public GameObject TMKillerSummary;
	public GameObject BlockMasterSummary;
	public Sprite BlueBirdImage;
	public Sprite GreenBirdImage;
	public Sprite OrangeBirdImage;
	public Sprite PurpleBirdImage;
	public Sprite RedBirdImage;
	public Sprite YellowBirdImage;
	public GameObject SummaryPanel;


	private float _interval = 9f;

	private int MostKillPlayer;
	private int MostSuicidePlayer;
	private int MostTMQPlayer;
	private int MostBlockPlayer;

	private Dictionary<string, Sprite> _playerLookUpDict;

	//private int maxKillValue = Mathf.Max(GameManager.GM.KillRecord.ToArray());
	//private int maxSuicideValue = Mathf.Max(GameManager.GM.SuicideRecord.ToArray());
	//private int maxTMQValue = Mathf.Max(GameManager.GM.TeammateMurderRecord.ToArray());
	//private int maxBlockValue = Mathf.Max(GameManager.GM.BlockTimes.ToArray());

	private void Awake()
	{
		_playerLookUpDict = new Dictionary<string, Sprite>();
		_playerLookUpDict["Yellow"] = YellowBirdImage;
		_playerLookUpDict["Orange"] = OrangeBirdImage;
		_playerLookUpDict["Pink"] = RedBirdImage;
		_playerLookUpDict["Blue"] = BlueBirdImage;
		_playerLookUpDict["Purple"] = PurpleBirdImage;
		_playerLookUpDict["Green"] = GreenBirdImage;
	}

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

	public void DisplayScore()
	{
		_fillSummary();
		StartCoroutine(ScoreDisplayer());
	}

	private void _fillSummary()
	{
		// Fill best killer summary
		int maxKillValue = 0;
		int maxKillPlayer = 0;
		int maxSuicideValue = 0;
		int maxSuicidePlayer = 0;
		int maxTMQValue = 0;
		int maxTMQPlayer = 0;
		int maxBlockValue = 0;
		int maxBlockPlayer = 0;

		for (int i = 0; i < 6; i++)
		{
			if (GameManager.GM.KillRecord[i] > maxKillValue)
			{
				maxKillValue = GameManager.GM.KillRecord[i];
				maxKillPlayer = i;
			}
			if (GameManager.GM.SuicideRecord[i] > maxSuicideValue)
			{
				maxSuicideValue = GameManager.GM.SuicideRecord[i];
				maxSuicidePlayer = i;
			}
			if (GameManager.GM.TeammateMurderRecord[i] > maxTMQValue)
			{
				maxTMQValue = GameManager.GM.TeammateMurderRecord[i];
				maxTMQPlayer = i;
			}
			if (GameManager.GM.BlockTimes[i] > maxBlockValue)
			{
				maxBlockValue = GameManager.GM.BlockTimes[i];
				maxBlockPlayer = i;
			}
		}
		KillMasterSummary.transform.GetChild(0).GetComponent<Image>().color = GameManager.GM.PlayersInformation[maxKillPlayer].PlayerColor;
		SuicideMasterSummary.transform.GetChild(0).GetComponent<Image>().color = GameManager.GM.PlayersInformation[maxSuicidePlayer].PlayerColor;
		TMKillerSummary.transform.GetChild(0).GetComponent<Image>().color = GameManager.GM.PlayersInformation[maxTMQPlayer].PlayerColor;
		BlockMasterSummary.transform.GetChild(0).GetComponent<Image>().color = GameManager.GM.PlayersInformation[maxBlockPlayer].PlayerColor;


		KillMasterSummary.transform.GetChild(1).GetComponent<Image>().sprite = _playerLookUpDict[GameManager.GM.PlayersInformation[maxKillPlayer].PlayerName];
		SuicideMasterSummary.transform.GetChild(1).GetComponent<Image>().sprite = _playerLookUpDict[GameManager.GM.PlayersInformation[maxSuicidePlayer].PlayerName];
		TMKillerSummary.transform.GetChild(1).GetComponent<Image>().sprite = _playerLookUpDict[GameManager.GM.PlayersInformation[maxTMQPlayer].PlayerName];
		BlockMasterSummary.transform.GetChild(1).GetComponent<Image>().sprite = _playerLookUpDict[GameManager.GM.PlayersInformation[maxBlockPlayer].PlayerName];

		KillMasterSummary.transform.GetChild(2).GetComponent<Text>().text = maxKillValue.ToString();
		SuicideMasterSummary.transform.GetChild(2).GetComponent<Text>().text = maxSuicideValue.ToString();
		TMKillerSummary.transform.GetChild(2).GetComponent<Text>().text = maxTMQValue.ToString();
		BlockMasterSummary.transform.GetChild(2).GetComponent<Text>().text = maxBlockValue.ToString();
	}

	IEnumerator ScoreDisplayer()
	{
		var maxKillValue = Mathf.Max(GameManager.GM.KillRecord.ToArray());
		var maxSuicideValue = Mathf.Max(GameManager.GM.SuicideRecord.ToArray());
		var maxTMQValue = Mathf.Max(GameManager.GM.TeammateMurderRecord.ToArray());
		var maxBlockValue = Mathf.Max(GameManager.GM.BlockTimes.ToArray());

		if (maxKillValue > 0)
		{
			DisplayKiller();

			yield return new WaitForSeconds(_interval);
		}

		if (maxSuicideValue > 0)
		{
			DisplaySuicider();

			yield return new WaitForSeconds(_interval);
		}

		if (maxTMQValue > 0)
		{
			DisplayTMKiller();

			yield return new WaitForSeconds(_interval);
		}

		if (maxBlockValue > 0)
		{
			DisplayBlocker();
			yield return new WaitForSeconds(_interval);
		}

		SummaryPanel.SetActive(true);

	}

	public void DisplayKiller()
	{
		MostKills.SetActive(true);

		StartCoroutine(ShowKillerName());
	}

	IEnumerator ShowKillerName()
	{
		var maxKillValue = Mathf.Max(GameManager.GM.KillRecord.ToArray());

		yield return new WaitForSeconds(TextToName);

		// Find player with most kills.
		//var maxKillValue = Mathf.Max(GameManager.GM.KillRecord.ToArray());

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


		if (maxKillValue > 1)
		{
			KillRecordFX.SetText("who killed " + GameManager.GM.KillRecord[MostKillPlayer] + " other birdies!");
		}
		else
		{
			KillRecordFX.SetText("who killed " + GameManager.GM.KillRecord[MostKillPlayer] + " other birdy!");
		}
		KillRecordGM.SetActive(true);
	}

	public void DisplaySuicider()
	{
		MostSuicide.SetActive(true);

		StartCoroutine(ShowSuiciderName());
	}

	IEnumerator ShowSuiciderName()
	{
		var maxSuicideValue = Mathf.Max(GameManager.GM.SuicideRecord.ToArray());
		yield return new WaitForSeconds(TextToName);

		//var maxSuicideValue = Mathf.Max(GameManager.GM.SuicideRecord.ToArray());
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

		if (maxSuicideValue > 1)
		{
			SuicideRecordFX.SetText("who killed themselves for " + GameManager.GM.SuicideRecord[MostSuicidePlayer] + " times!");
		}
		else
		{
			SuicideRecordFX.SetText("who killed themselves for " + GameManager.GM.SuicideRecord[MostSuicidePlayer] + " time!");
		}
		SuicideRecordGM.SetActive(true);
	}

	public void DisplayTMKiller()
	{
		TMKiller.SetActive(true);

		StartCoroutine(ShowTMKillerName());
	}

	IEnumerator ShowTMKillerName()
	{
		var maxTMQValue = Mathf.Max(GameManager.GM.TeammateMurderRecord.ToArray());
		yield return new WaitForSeconds(TextToName);

		//var maxTMQValue = Mathf.Max(GameManager.GM.TeammateMurderRecord.ToArray());
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

		if (maxTMQValue > 1)
		{
			TMKillerRecordFX.SetText("who killed their teammates for " + GameManager.GM.TeammateMurderRecord[MostTMQPlayer] + " times!");
		}
		else
		{
			TMKillerRecordFX.SetText("who killed their teammates for " + GameManager.GM.TeammateMurderRecord[MostTMQPlayer] + " time!");
		}
		TMKillerRecordGM.SetActive(true);
	}
	public void DisplayBlocker()
	{
		Block.SetActive(true);

		StartCoroutine(ShowBlockName());
	}

	IEnumerator ShowBlockName()
	{
		var maxBlockValue = Mathf.Max(GameManager.GM.BlockTimes.ToArray());

		yield return new WaitForSeconds(TextToName);

		//int maxBlockValue = Mathf.Max(GameManager.GM.BlockTimes.ToArray());
		for (int tracker = 0; tracker < 6; tracker++)
		{
			if (GameManager.GM.BlockTimes[tracker] == maxBlockValue)
			{
				MostBlockPlayer = tracker;
				break;
			}
		}

		BlockNameFX.SetText(GameManager.GM.PlayersInformation[MostBlockPlayer].PlayerName);
		BlockNameFX.SetColour(GameManager.GM.PlayersInformation[MostBlockPlayer].PlayerColor);

		BlockNameGM.SetActive(true);

		yield return new WaitForSeconds(NameToRecord);

		if (maxBlockValue > 1)
		{
			BlockRecordFX.SetText("who blocked attacks for " + GameManager.GM.BlockTimes[MostBlockPlayer] + " times!");
		}
		else
		{
			BlockRecordFX.SetText("who blocked attacks for " + GameManager.GM.BlockTimes[MostBlockPlayer] + " time!");
		}
		BlockRecordGM.SetActive(true);
	}
}
