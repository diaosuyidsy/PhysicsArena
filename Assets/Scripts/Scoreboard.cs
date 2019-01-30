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

    private float _interval = 9f;
    
    private int MostKillPlayer;
    private int MostSuicidePlayer;
    private int MostTMQPlayer;
    private int MostBlockPlayer;

    //private int maxKillValue = Mathf.Max(GameManager.GM.KillRecord.ToArray());
    //private int maxSuicideValue = Mathf.Max(GameManager.GM.SuicideRecord.ToArray());
    //private int maxTMQValue = Mathf.Max(GameManager.GM.TeammateMurderRecord.ToArray());
    //private int maxBlockValue = Mathf.Max(GameManager.GM.BlockTimes.ToArray());
    
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
        StartCoroutine(ScoreDisplayer());
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
        }
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
