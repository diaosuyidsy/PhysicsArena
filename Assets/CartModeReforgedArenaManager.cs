using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CartModeReforgedArenaManager : MonoBehaviour
{

    private enum CartSide
    {
        Neutral,
        Team1,
        Team2
    }

    public CartModeReforgedModeData Data;

    public GameObject Cart;
    public GameObject CheckPoints;
    public GameObject Team1ExpCounter;
    public GameObject Team1LevelText;
    public GameObject Team2ExpCounter;
    public GameObject Team2LevelText;


    public Color NeutralDotlineColor;
    public Color Team1DotlineColor;
    public Color Team2DotlineColor;

    private List<GameObject> InCartPlayerList;
    private List<GameObject> CheckPointList;
    private int TargetWayPointIndex;
    private CartSide CurrentSide;


    private int Team1Level;
    private int Team2Level;
    private int Team1Exp;
    private int Team2Exp;


    private bool GameStart;
    private bool GameEnd;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DetectCartSide()
    {

    }

    private void OnPlayerDied(PlayerDied e)
    {
        if(GameEnd || !GameStart)
        {
            return;
        }

        if (e.Player.tag.Contains("1"))
        {
            GainExp(Data.KillExp, false);
        }
        else
        {
            GainExp(Data.KillExp, true);
        }

        RefreshUI();
    }

    private void GainExp(int Amount, bool Team1)
    {
        if (Team1)
        {
            Team1Exp += Amount;
            if (Team2Level > Team1Level)
            {
                Team1Exp += Data.KillExpBonusPerLevelDif * (Team2Level - Team1Level);
            }
            if (Team1Exp >= Data.LevelUpExp)
            {
                int LevelUp = Team1Exp / Data.LevelUpExp;
                Team1Level += LevelUp;
                Team1Exp -= LevelUp * Data.LevelUpExp;
            }
        }
        else
        {
            Team2Exp += Amount;
            if (Team1Level > Team2Level)
            {
                Team2Exp += Data.KillExpBonusPerLevelDif * (Team1Level - Team2Level);
            }
            if (Team2Exp >= Data.LevelUpExp)
            {
                int LevelUp = Team2Exp / Data.LevelUpExp;
                Team2Level += LevelUp;
                Team2Exp -= LevelUp * Data.LevelUpExp;
            }
        }
    }

    private void OnGameStart(GameStart e)
    {
        GameStart = true;
    }

    private void RefreshUI()
    {
        Team1ExpCounter.GetComponent<Image>().fillAmount = (float)Team1Exp / Data.LevelUpExp;
        Team1LevelText.GetComponent<TextMeshProUGUI>().text = Team1Level.ToString();
        Team2ExpCounter.GetComponent<Image>().fillAmount = (float)Team2Exp / Data.LevelUpExp;
        Team2LevelText.GetComponent<TextMeshProUGUI>().text = Team2Level.ToString();
    }
}
