using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrawlModeReforgedObjectiveManager : ObjectiveManager
{
    public int Team1Score;
    public int Team2Score;
    private int winner
    {
        get
        {
            if (Team1Score == Team2Score) return 0;
            return Team1Score > Team2Score ? 1 : 2;
        }
    }

    private TextMeshProUGUI Team1ScoreText;
    private TextMeshProUGUI Team2ScoreText;
    private GameObject Team1ScorePlusText;
    private GameObject Team2ScorePlusText;

    private GameObject Team1Board;
    private GameObject Team2Board;

    private GameObject Team1ScoreImpact;
    private GameObject Team2ScoreImpact;

    private BrawlModeReforgedModeData ModeData;
    private BrawlModeReforgedUIData UIData;

    private bool gameEnd;
    private bool gameStart;

    private float OneSecTimer;
    private int Timer;
    private float Team1HopTimer;
    private float Team2HopTimer;
    private float Team1PlusHopTimer;
    private float Team2PlusHopTimer;

    private Vector3 Team1ShakeTarget;
    private Vector3 Team2ShakeTarget;
    private Vector3 Team1ShakeDir;
    private Vector3 Team2ShakeDir;

    private Vector3 Team1BoardOriPos;
    private Vector3 Team2BoardOriPos;

    private GameObject Players;

    public BrawlModeReforgedObjectiveManager(BrawlModeReforgedModeData Data,BrawlModeReforgedUIData UI) : base()
    {

        ModeData = Data;
        UIData = UI;

        Timer = Data.TotalTime;

        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.AddHandler<BagelSent>(OnBagelSent);

        GameObject Title = TutorialCanvas.Find("ObjectiveImages").Find("Title").gameObject;
        Title.GetComponent<TextMeshProUGUI>().text = "Score " + ModeData.TargetScore.ToString() + " First";

        Team1Board = GameUI.Find("Team1Background").gameObject;
        Team2Board = GameUI.Find("Team2Background").gameObject;

        Team1ScoreText = Team1Board.transform.Find("Team1Score").GetComponent<TextMeshProUGUI>();
        Team2ScoreText = Team2Board.transform.Find("Team2Score").GetComponent<TextMeshProUGUI>();
        Team1ScorePlusText = Team1Board.transform.Find("Team1ScorePlus").gameObject;
        Team2ScorePlusText = Team2Board.transform.Find("Team2ScorePlus").gameObject;

        Team1ScoreImpact = Team1Board.transform.Find("Team1ScoreImpact").gameObject;
        Team2ScoreImpact = Team2Board.transform.Find("Team2ScoreImpact").gameObject;

        Team1BoardOriPos = Team1Board.GetComponent<RectTransform>().localPosition;
        Team2BoardOriPos = Team2Board.GetComponent<RectTransform>().localPosition;

        GetShakeInfo(true);
        GetShakeInfo(false);

        Team1Score = 0;
        Team2Score = 0;
        RefreshScore();
        gameEnd = false;

        Team1HopTimer =  Team2HopTimer = UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime + 1;
        Team1PlusHopTimer = Team2PlusHopTimer = UI.ScorePlusTextHopScaleUpTime + UI.ScorePlusTextHopScaleDownTime + UI.ScorePlusTextHopStayTime + 1;

        Players = GameObject.Find("Players").gameObject;
    }

    public override void Destroy()
    {
        EventManager.Instance.RemoveHandler<GameStart>(OnGameStart);
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.RemoveHandler<BagelSent>(OnBagelSent);
    }

    public override void Update()
    {
        CloseScoreShake();

        if (gameStart)
        {
            ScoreHop();
            ScorePlusHop();
        }

        if (gameEnd || !gameStart) return;



    }

    private void RefreshScore()
    {
        Team1ScoreText.text = Team1Score.ToString();
        Team2ScoreText.text = Team2Score.ToString();
    }

    private void CheckWin()
    {
        Vector3 Position = (Team1Board.transform.position + Team2Board.transform.position) / 2;

        if (Team1Score >= ModeData.TargetScore)
        {
            gameEnd = true;
            EventManager.Instance.TriggerEvent(new GameEnd(1, Camera.main.ScreenToWorldPoint(Position), GameWinType.ScoreWin));
        }
        else if(Team2Score >= ModeData.TargetScore)
        {
            gameEnd = true;
            EventManager.Instance.TriggerEvent(new GameEnd(2, Camera.main.ScreenToWorldPoint(Position), GameWinType.ScoreWin));
        }
    }

    private void GetScore(int amount, bool Team1)
    {
        if (Team1)
        {
            Team1Score += amount;
            Team1HopTimer = 0;
            //Team1PlusHopTimer = UIData.ScorePlusTextHopScaleUpTime + UIData.ScorePlusTextHopScaleDownTime + UIData.ScorePlusTextHopStayTime + 1;
            //Team1ScorePlusText.GetComponent<TextMeshProUGUI>().text = "+" + amount.ToString();
        }
        else
        {
            Team2Score += amount;
            Team2HopTimer = 0;
            //Team2PlusHopTimer = UIData.ScorePlusTextHopScaleUpTime + UIData.ScorePlusTextHopScaleDownTime + UIData.ScorePlusTextHopStayTime + 1;
            //Team2ScorePlusText.GetComponent<TextMeshProUGUI>().text = "+" + amount.ToString();
        }
    }

    private void OnBagelSent(BagelSent e)
    {
        if (gameEnd || !gameStart)
        {
            return;
        }

        if (e.Basket == BrawlModeReforgedArenaManager.Team1Basket)
        {
            /*foreach (Transform child in Players.transform)
            {
                if (child.gameObject.tag.Contains("1"))
                {
                    GenerateScorePlus(child.gameObject, true, true, ModeData.DeliveryPoint);
                }
            }*/

            GenerateScorePlus(e.Basket, false, true, ModeData.DeliveryPoint);

            GetScore(ModeData.DeliveryPoint, true);
        }
        else
        {
            /*foreach (Transform child in Players.transform)
            {
                if (child.gameObject.tag.Contains("2"))
                {
                    GenerateScorePlus(child.gameObject, true, false, ModeData.DeliveryPoint);
                }
            }*/

            GenerateScorePlus(e.Basket, false, false, ModeData.DeliveryPoint);

            GetScore(ModeData.DeliveryPoint, false);
        }

        RefreshScore();
        CheckWin();
    }

    private void GenerateScorePlus(GameObject Holder,bool Character,bool Team1, int amount)
    {
        Vector3 Offset;
        Vector3 FadeOffset;
        float Scale;
        if (Character)
        {
            Offset = UIData.ScorePlusTextCharacterOffset;
            FadeOffset = UIData.ScorePlusTextCharacterFadeOffset;
            Scale = UIData.ScorePlusTextCharacterScale;
        }
        else
        {
            Offset = UIData.ScorePlusTextBasketOffset;
            FadeOffset = UIData.ScorePlusTextBasketFadeOffset;
            Scale = UIData.ScorePlusTextBasketScale;
        }

        GameObject Plus = GameObject.Instantiate(UIData.ScorePlusTextPrefab);
        Plus.transform.position = Offset + Holder.transform.position;
        Plus.transform.localScale = Scale * Vector3.one;
        Plus.transform.SetParent(Holder.transform);

        Plus.transform.Find("ScorePlus").GetComponent<ScorePlusText>().Holder = Holder;
        Plus.transform.Find("ScorePlus").GetComponent<ScorePlusText>().Team1 = Team1;
        Plus.transform.Find("ScorePlus").GetComponent<TextMeshProUGUI>().text = "+" + amount.ToString();
        Plus.transform.Find("ScorePlus").GetComponent<ScorePlusText>().FadeOffset = FadeOffset;
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if (gameEnd || !gameStart)
        {
            return;
        }

        if ((!e.HitterIsValid || e.PlayerHitter == null) && Utility.GetPlayerNumber()<=2)
        {
            //return;
        }

        if (e.Player.tag.Contains("1"))
        {
            /*foreach(Transform child in Players.transform)
            {
                if (child.gameObject.tag.Contains("2"))
                {
                    GenerateScorePlus(child.gameObject, true, false, ModeData.NormalKillPoint);
                }
            }*/

            if(e.PlayerHitter !=null && e.HitterIsValid && e.PlayerHitter.GetComponent<PlayerController>() && e.PlayerHitter.tag.Contains("2"))
            {
                GenerateScorePlus(e.PlayerHitter, true, false, ModeData.NormalKillPoint);
            }
            else
            {
                Team2PlusHopTimer = UIData.ScorePlusTextHopScaleUpTime + UIData.ScorePlusTextHopScaleDownTime + UIData.ScorePlusTextHopStayTime + 1;
                Team2ScorePlusText.GetComponent<TextMeshProUGUI>().text = "+" + ModeData.NormalKillPoint.ToString();
            }

            GetScore(ModeData.NormalKillPoint, false);
        }
        else
        {
            /*foreach (Transform child in Players.transform)
            {
                if (child.gameObject.tag.Contains("1"))
                {
                    GenerateScorePlus(child.gameObject, true, true, ModeData.NormalKillPoint);
                }
            }*/
            if (e.PlayerHitter != null && e.HitterIsValid&& e.PlayerHitter.GetComponent<PlayerController>() && e.PlayerHitter.tag.Contains("1"))
            {
                GenerateScorePlus(e.PlayerHitter, true, true, ModeData.NormalKillPoint);
            }
            else
            {
                Team1PlusHopTimer = UIData.ScorePlusTextHopScaleUpTime + UIData.ScorePlusTextHopScaleDownTime + UIData.ScorePlusTextHopStayTime + 1;
                Team1ScorePlusText.GetComponent<TextMeshProUGUI>().text = "+" + ModeData.NormalKillPoint.ToString();
            }


            GetScore(ModeData.NormalKillPoint, true);
        }

        RefreshScore();
        CheckWin();
    }

    private void OnGameStart(GameStart e)
    {
        gameStart = true;
    }

    private void ScorePlusHop()
    {
        Team1PlusHopTimer += Time.deltaTime;
        Team2PlusHopTimer += Time.deltaTime;

        Team1ScorePlusText.GetComponent<TextMeshProUGUI>().enabled = true;
        Team2ScorePlusText.GetComponent<TextMeshProUGUI>().enabled = true;

        if (Team1PlusHopTimer <= UIData.ScorePlusTextHopScaleUpTime)
        {
            Team1ScorePlusText.transform.localScale = Mathf.Lerp(UIData.ScorePlusTextHopStartScale, UIData.ScorePlusTextHopBigScale, Team1PlusHopTimer / UIData.ScorePlusTextHopScaleUpTime) * Vector3.one;
        }
        else if(Team1PlusHopTimer <= UIData.ScorePlusTextHopScaleUpTime + UIData.ScorePlusTextHopScaleDownTime)
        {
            float Timer = Team1PlusHopTimer - UIData.ScorePlusTextHopScaleUpTime;
            Team1ScorePlusText.transform.localScale = Mathf.Lerp(UIData.ScorePlusTextHopBigScale, UIData.ScorePlusTextHopEndScale, Timer / UIData.ScorePlusTextHopScaleDownTime)*Vector3.one;

        }
        else if(Team1PlusHopTimer <= UIData.ScorePlusTextHopScaleUpTime + UIData.ScorePlusTextHopScaleDownTime + UIData.ScorePlusTextHopStayTime)
        {
            Team1ScorePlusText.transform.localScale = Vector3.one * UIData.ScorePlusTextHopEndScale;
        }
        else
        {
            Team1ScorePlusText.GetComponent<TextMeshProUGUI>().enabled = false;
        }

        if (Team2PlusHopTimer <= UIData.ScorePlusTextHopScaleUpTime)
        {
            Team2ScorePlusText.transform.localScale = Mathf.Lerp(UIData.ScorePlusTextHopStartScale, UIData.ScorePlusTextHopBigScale, Team2PlusHopTimer / UIData.ScorePlusTextHopScaleUpTime) * Vector3.one;
        }
        else if (Team2PlusHopTimer <= UIData.ScorePlusTextHopScaleUpTime + UIData.ScorePlusTextHopScaleDownTime)
        {
            float Timer = Team2PlusHopTimer - UIData.ScorePlusTextHopScaleUpTime;
            Team2ScorePlusText.transform.localScale = Mathf.Lerp(UIData.ScorePlusTextHopBigScale, UIData.ScorePlusTextHopEndScale, Timer / UIData.ScorePlusTextHopScaleDownTime) * Vector3.one;
        }
        else if (Team2PlusHopTimer <= UIData.ScorePlusTextHopScaleUpTime + UIData.ScorePlusTextHopScaleDownTime + UIData.ScorePlusTextHopStayTime)
        {
            Team2ScorePlusText.transform.localScale = Vector3.one * UIData.ScorePlusTextHopEndScale;
        }
        else
        {
            Team2ScorePlusText.GetComponent<TextMeshProUGUI>().enabled = false;
        }
    }

    private void ScoreHop()
    {
        Team1HopTimer += Time.deltaTime;
        Team2HopTimer += Time.deltaTime;

        GameObject Text;
        GameObject Board;

        Text = Team1ScoreText.gameObject;
        Board = Team1Board;


        if(Team1HopTimer <= UIData.ScoreHopFirstPhaseTime)
        {
            Color color = Text.GetComponent<TextMeshProUGUI>().color;
            Text.GetComponent<TextMeshProUGUI>().color = Color.Lerp(new Color(color.r, color.g, color.b, UIData.ScoreTextHopStartAlpha), new Color(color.r, color.g, color.b, UIData.ScoreTextHopEndAlpha), Team1HopTimer / UIData.ScoreHopFirstPhaseTime);
            Text.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreTextHopStartScale, UIData.ScoreTextHopNormalScale, Team1HopTimer / UIData.ScoreHopFirstPhaseTime);
        }
        else if(Team1HopTimer <= UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime)
        {


            Color color = Text.GetComponent<TextMeshProUGUI>().color;
            Text.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 1);

            if (Team1HopTimer <= UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime / 2)
            {
                Text.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreTextHopNormalScale, UIData.ScoreTextHopSmallScale, (Team1HopTimer-UIData.ScoreHopFirstPhaseTime) / (UIData.ScoreHopSecondPhaseTime / 2));
                //Board.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreBoardHopNormalScale, UIData.ScoreBoardHopBigScale, (Team1HopTimer - UIData.ScoreHopFirstPhaseTime) / (UIData.ScoreHopSecondPhaseTime / 2));
            }
            else
            {
                Text.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreTextHopSmallScale, UIData.ScoreTextHopNormalScale, (Team1HopTimer - UIData.ScoreHopFirstPhaseTime - UIData.ScoreHopSecondPhaseTime / 2) / (UIData.ScoreHopSecondPhaseTime / 2));
                //Board.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreBoardHopBigScale, UIData.ScoreBoardHopNormalScale, (Team1HopTimer - UIData.ScoreHopFirstPhaseTime - UIData.ScoreHopSecondPhaseTime / 2) / (UIData.ScoreHopSecondPhaseTime / 2));
            }
        }
        else
        {
            Color color = Text.GetComponent<TextMeshProUGUI>().color;
            Text.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 1);
            Text.transform.localScale = Vector3.one * UIData.ScoreTextHopNormalScale;
        }

        if(Team1HopTimer >= UIData.ScoreHopBoardHopBeginTime && Team1HopTimer <= UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime)
        {
            float Total = UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime - UIData.ScoreHopBoardHopBeginTime;
            float Current = UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime - Team1HopTimer;

            if (Team1HopTimer - Time.deltaTime <= UIData.ScoreHopBoardHopBeginTime)
            {
                Team1ScoreImpact.GetComponent<Image>().enabled = true;
                Team1ScoreImpact.GetComponent<Animator>().Play("Team1ScoreImpact", -1, 0);
                Team1PlusHopTimer = 0;
            }

            if (Current < Total/2)
            {
                Board.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreBoardHopNormalScale, UIData.ScoreBoardHopBigScale, Current / (Total/2));
            }
            else
            {
                Board.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreBoardHopBigScale, UIData.ScoreBoardHopNormalScale,  (Current-Total/2) / (Total / 2));
            }
        }
        else
        {
            Board.transform.localScale = Vector3.one * UIData.ScoreBoardHopNormalScale;
        }

        Text = Team2ScoreText.gameObject;
        Board = Team2Board;

        if (Team2HopTimer <= UIData.ScoreHopFirstPhaseTime)
        {
            Color color = Text.GetComponent<TextMeshProUGUI>().color;
            Text.GetComponent<TextMeshProUGUI>().color = Color.Lerp(new Color(color.r, color.g, color.b, UIData.ScoreTextHopStartAlpha), new Color(color.r, color.g, color.b, UIData.ScoreTextHopEndAlpha), Team2HopTimer / UIData.ScoreHopFirstPhaseTime);
            Text.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreTextHopStartScale, UIData.ScoreTextHopNormalScale, Team2HopTimer / UIData.ScoreHopFirstPhaseTime);
        }
        else if (Team2HopTimer <= UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime)
        {

            Color color = Text.GetComponent<TextMeshProUGUI>().color;
            Text.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 1);

            if (Team2HopTimer <= UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime / 2)
            {
                Text.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreTextHopNormalScale, UIData.ScoreTextHopSmallScale, (Team2HopTimer - UIData.ScoreHopFirstPhaseTime) / (UIData.ScoreHopSecondPhaseTime / 2));
                //Board.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreBoardHopNormalScale, UIData.ScoreBoardHopBigScale, (Team2HopTimer - UIData.ScoreHopFirstPhaseTime) / (UIData.ScoreHopSecondPhaseTime / 2));
            }
            else
            {
                Text.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreTextHopSmallScale, UIData.ScoreTextHopNormalScale, (Team2HopTimer - UIData.ScoreHopFirstPhaseTime - UIData.ScoreHopSecondPhaseTime / 2) / (UIData.ScoreHopSecondPhaseTime / 2));
                //Board.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreBoardHopBigScale, UIData.ScoreBoardHopNormalScale, (Team2HopTimer - UIData.ScoreHopFirstPhaseTime - UIData.ScoreHopSecondPhaseTime / 2) / (UIData.ScoreHopSecondPhaseTime / 2));
            }
        }
        else
        {
            Color color = Text.GetComponent<TextMeshProUGUI>().color;
            Text.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 1);
            Text.transform.localScale = Vector3.one * UIData.ScoreTextHopNormalScale;
        }

        if (Team2HopTimer >= UIData.ScoreHopBoardHopBeginTime && Team2HopTimer <= UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime)
        {
            float Total = UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime - UIData.ScoreHopBoardHopBeginTime;
            float Current = UIData.ScoreHopFirstPhaseTime + UIData.ScoreHopSecondPhaseTime - Team2HopTimer;

            if (Team2HopTimer - Time.deltaTime <= UIData.ScoreHopBoardHopBeginTime)
            {
                Team2ScoreImpact.GetComponent<Image>().enabled = true;
                Team2ScoreImpact.GetComponent<Animator>().Play("Team2ScoreImpact", -1, 0);
                Team2PlusHopTimer = 0;
            }

            if (Current < Total / 2)
            {
                Board.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreBoardHopNormalScale, UIData.ScoreBoardHopBigScale, Current / (Total / 2));
            }
            else
            {
                Board.transform.localScale = Vector3.one * Mathf.Lerp(UIData.ScoreBoardHopBigScale, UIData.ScoreBoardHopNormalScale, (Current - Total / 2) / (Total / 2));
            }

        }
        else
        {
            Board.transform.localScale = Vector3.one * UIData.ScoreBoardHopNormalScale;
        }
    }

    private void CloseScoreShake()
    {
        if(Team1Score >= ModeData.CloseScore)
        {
            Team1Board.transform.localPosition += Team1ShakeDir * UIData.ShakeSpeed * Time.deltaTime;
            if(Vector3.Dot(Team1Board.GetComponent<RectTransform>().localPosition - Team1ShakeTarget, Team1ShakeDir) > 0)
            {
                Team1Board.GetComponent<RectTransform>().localPosition = Team1ShakeTarget;
                GetShakeInfo(true);
            }
        }
        else
        {
            Team1Board.GetComponent<RectTransform>().localPosition = Team1BoardOriPos;
        }

        if (Team2Score >= ModeData.CloseScore)
        {
            Team2Board.transform.localPosition += Team2ShakeDir * UIData.ShakeSpeed * Time.deltaTime;
            if (Vector3.Dot(Team2Board.GetComponent<RectTransform>().localPosition - Team2ShakeTarget, Team2ShakeDir) > 0)
            {
                Team2Board.GetComponent<RectTransform>().localPosition = Team2ShakeTarget;
                GetShakeInfo(false);
            }
        }
        else
        {
            Team2Board.GetComponent<RectTransform>().localPosition = Team2BoardOriPos;
        }
    }

    private void GetShakeInfo(bool Team1)
    {
        if (Team1)
        {
            Vector3 Dir = Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector3.right;

            Team1ShakeTarget = Team1BoardOriPos + Dir * UIData.ShakeRadius;
            Team1ShakeDir = (Team1ShakeTarget - Team1Board.GetComponent<RectTransform>().localPosition).normalized;
        }
        else
        {
            Vector3 Dir = Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector3.right;

            Team2ShakeTarget = Team2BoardOriPos + Dir * UIData.ShakeRadius;
            Team2ShakeDir = (Team2ShakeTarget - Team2Board.GetComponent<RectTransform>().localPosition).normalized;
        }
    }
}
