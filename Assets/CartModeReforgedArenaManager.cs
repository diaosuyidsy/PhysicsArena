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

    private enum CheckPointSide
    {
        Team1,
        Team2
    }

    public CartModeReforgedModeData Data;
    public CartModeReforgedFeelData FeelData;

    public GameObject Players;
    public GameObject Cart;
    public GameObject CheckPoints;
    public GameObject Team1ExpCounter;
    public GameObject Team1LevelText;
    public GameObject Team1ExpText;
    public GameObject Team2ExpCounter;
    public GameObject Team2LevelText;
    public GameObject Team2ExpText;

    public float CartRadius;
    public GameObject CartDotline;


    private List<GameObject> CheckPointList;
    private int TargetWayPointIndex;
    private int CurrentTeam1Checkpoints;
    private int CurrentTeam2Checkpoints;
    private CartSide CurrentSide;
    private CartSide LastSide;
    private float CurrentSpeed;


    private int Team1Level;
    private int Team2Level;
    private int Team1Exp;
    private int Team2Exp;

    private float Team1LevelTextHopTimer;
    private float Team1ExpTextHopTimer;
    private float Team2LevelTextHopTimer;
    private float Team2ExpTextHopTimer;
    private int Team1AccumulatedExp;
    private int Team2AccumulatedExp;

    private float Team1CheckPointHopTimer;
    private float Team2CheckPointHopTimer;

    private float CheckPointExpTimer;

    private bool GameStart;
    private bool GameEnd;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);

        TargetWayPointIndex = -1;
        CurrentSide = LastSide = CartSide.Neutral;
        CartDotline.GetComponent<SpriteRenderer>().color = FeelData.NeutralDotlineColor;

        Team1LevelTextHopTimer = FeelData.TextHopTime;
        Team1ExpTextHopTimer = FeelData.TextHopTime + FeelData.ExpTextStayTime;
        Team2LevelTextHopTimer = FeelData.TextHopTime;
        Team2ExpTextHopTimer = FeelData.TextHopTime + FeelData.ExpTextStayTime;

        Team1CheckPointHopTimer = FeelData.TextHopTime;
        Team2CheckPointHopTimer = FeelData.TextHopTime;

        InitCheckPoint();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<GameStart>(OnGameStart);
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
    }

    // Update is called once per frame
    void Update()
    {
        DetectCartSide();
        SetDotline();
        CartMove();
        SetTextHop();
        SetCheckPointScale();
        CountCheckPointExp();
    }

    private void DetectCartSide()
    {
        int Team1Count = 0;
        int Team2Count = 0;

        foreach(Transform child in Players.transform)
        {
            if(child.gameObject.activeSelf && PlayerInCart(child.gameObject))
            {
                if (child.tag.Contains("1"))
                {
                    Team1Count++;
                }
                else
                {
                    Team2Count++;
                }
            }
        }
        if (CurrentSide != CartSide.Neutral)
        {
            LastSide = CurrentSide;
        }
        if (Team1Count>0 && Team2Count > 0)
        {
            if(Team1Level > Team2Level)
            {
                CurrentSide = CartSide.Team1;
                CurrentSpeed = Data.BaseCartSpeed + Data.CartSpeedBonusPerLevel * (Team1Level-Team2Level);
                CurrentSpeed = 0;
            }
            else if (Team2Level > Team1Level)
            {
                CurrentSide = CartSide.Team2;
                CurrentSpeed = Data.BaseCartSpeed + Data.CartSpeedBonusPerLevel * (Team2Level - Team1Level);
                CurrentSpeed = 0;
            }
            else
            {
                CurrentSide = CartSide.Neutral;
                CurrentSpeed = 0;
            }

            CurrentSide = CartSide.Neutral;
        }
        else if (Team1Count > 0)
        {
            CurrentSide = CartSide.Team1;
            CurrentSpeed = Data.BaseCartSpeed + Data.CartSpeedBonusPerLevel * Team1Level;
        }
        else if (Team2Count > 0)
        {
            CurrentSide = CartSide.Team2;
            CurrentSpeed = Data.BaseCartSpeed + Data.CartSpeedBonusPerLevel * Team2Level;
        }
        else
        {
            CurrentSide = CartSide.Neutral;
            CurrentSpeed = 0;
        }
    }

    private void SetDotline()
    {
        switch (CurrentSide)
        {
            case CartSide.Neutral:
                CartDotline.GetComponent<SpriteRenderer>().color = FeelData.NeutralDotlineColor;
                break;
            case CartSide.Team1:
                CartDotline.GetComponent<SpriteRenderer>().color = FeelData.Team1DotlineColor;
                break;
            case CartSide.Team2:
                CartDotline.GetComponent<SpriteRenderer>().color = FeelData.Team2DotlineColor;
                break;
        }
    }

    private void CartMove()
    {
        if(CurrentSide == CartSide.Team1)
        {
            if(TargetWayPointIndex < 0)
            {
                TargetWayPointIndex = CheckPointList.Count / 2;
            }
            else if(LastSide == CartSide.Team2)
            {
                TargetWayPointIndex++;
            }
        }
        else if(CurrentSide == CartSide.Team2)
        {
            if (TargetWayPointIndex < 0)
            {
                TargetWayPointIndex = CheckPointList.Count / 2 - 1;
            }
            else if(LastSide == CartSide.Team1)
            {
                TargetWayPointIndex--;
            }
        }
        else
        {
            return;
        }

        Vector3 Offset = CheckPointList[TargetWayPointIndex].transform.position - Cart.transform.position;
        Offset.y = 0;

        Cart.transform.position += CurrentSpeed * Offset.normalized*Time.deltaTime;
        Cart.transform.forward = Offset.normalized;

        if(Vector3.Dot(CheckPointList[TargetWayPointIndex].transform.position - Cart.transform.position, Offset)<0)
        {
            Cart.transform.position = new Vector3(CheckPointList[TargetWayPointIndex].transform.position.x, Cart.transform.position.y, CheckPointList[TargetWayPointIndex].transform.position.z);
            if(CurrentSide == CartSide.Team1)
            {
                CurrentTeam1Checkpoints++;
                CurrentTeam2Checkpoints--;
                if(CurrentTeam2Checkpoints == 0)
                {
                    GameEnd = true;
                    EventManager.Instance.TriggerEvent(new GameEnd(1, Cart.transform, GameWinType.CartWin));
                }
                else
                {
                    TargetWayPointIndex++;
                }
            }
            else if(CurrentSide == CartSide.Team2)
            {
                CurrentTeam1Checkpoints--;
                CurrentTeam2Checkpoints++;
                if (CurrentTeam1Checkpoints == 0)
                {
                    GameEnd = true;
                    EventManager.Instance.TriggerEvent(new GameEnd(2, Cart.transform, GameWinType.CartWin));
                }
                else
                {
                    TargetWayPointIndex--;
                }
            }

            SetCheckPointAppearance();
        }


    }

    private void InitCheckPoint()
    {
        CheckPointList = new List<GameObject>();

        foreach(Transform child in CheckPoints.transform)
        {
            CheckPointList.Add(child.gameObject);
        }

        CurrentTeam1Checkpoints = CurrentTeam2Checkpoints = CheckPointList.Count / 2;

        SetCheckPointAppearance();
    }

    private void SetTextHop()
    {
        Team1LevelTextHopTimer += Time.deltaTime;
        Team1ExpTextHopTimer += Time.deltaTime;
        Team2ExpTextHopTimer += Time.deltaTime;
        Team2LevelTextHopTimer += Time.deltaTime;

        if(Team1LevelTextHopTimer <= FeelData.TextHopTime/2)
        {
            Team1LevelText.transform.localScale = Vector3.one * Mathf.Lerp(FeelData.TextDefaultScale, FeelData.TextHopScale, Team1LevelTextHopTimer / (FeelData.TextHopTime / 2));
        }
        else if(Team1LevelTextHopTimer <= FeelData.TextHopTime)
        {
            Team1LevelText.transform.localScale = Vector3.one * Mathf.Lerp(FeelData.TextHopScale, FeelData.TextDefaultScale,  (Team1LevelTextHopTimer- FeelData.TextHopTime / 2) / (FeelData.TextHopTime / 2));
        }
        else
        {
            Team1LevelText.transform.localScale = Vector3.one * FeelData.TextDefaultScale;
        }

        if (Team2LevelTextHopTimer <= FeelData.TextHopTime / 2)
        {
            Team2LevelText.transform.localScale = Vector3.one * Mathf.Lerp(FeelData.TextDefaultScale, FeelData.TextHopScale, Team2LevelTextHopTimer / (FeelData.TextHopTime / 2));
        }
        else if (Team2LevelTextHopTimer <= FeelData.TextHopTime)
        {
            Team2LevelText.transform.localScale = Vector3.one * Mathf.Lerp(FeelData.TextHopScale, FeelData.TextDefaultScale,  (Team2LevelTextHopTimer - FeelData.TextHopTime / 2) / (FeelData.TextHopTime / 2));
        }
        else
        {
            Team2LevelText.transform.localScale = Vector3.one * FeelData.TextDefaultScale;
        }

        if(Team1ExpTextHopTimer <= FeelData.TextHopTime / 2)
        {
            Team1ExpText.GetComponent<TextMeshProUGUI>().enabled = true;
            Team1ExpText.transform.localScale = Vector3.one * Mathf.Lerp(FeelData.TextDefaultScale, FeelData.TextHopScale, Team1ExpTextHopTimer / (FeelData.TextHopTime / 2));
        }
        else if(Team1ExpTextHopTimer <= FeelData.TextHopTime)
        {
            Team1ExpText.GetComponent<TextMeshProUGUI>().enabled = true;
            Team1ExpText.transform.localScale = Vector3.one * Mathf.Lerp(FeelData.TextHopScale, FeelData.TextDefaultScale,  (Team1ExpTextHopTimer- FeelData.TextHopTime / 2) / (FeelData.TextHopTime / 2));
        }
        else if(Team1ExpTextHopTimer <= FeelData.TextHopTime + FeelData.ExpTextStayTime)
        {
            Team1ExpText.GetComponent<TextMeshProUGUI>().enabled = true;
            Team1ExpText.transform.localScale = Vector3.one * FeelData.TextDefaultScale;
        }
        else
        {
            Team1ExpText.GetComponent<TextMeshProUGUI>().enabled = false;
            Team1ExpText.transform.localScale = Vector3.one * FeelData.TextDefaultScale;
            Team1AccumulatedExp = 0;
        }

        if (Team2ExpTextHopTimer <= FeelData.TextHopTime / 2)
        {
            Team2ExpText.GetComponent<TextMeshProUGUI>().enabled = true;
            Team2ExpText.transform.localScale = Vector3.one * Mathf.Lerp(FeelData.TextDefaultScale, FeelData.TextHopScale, Team2ExpTextHopTimer / (FeelData.TextHopTime / 2));
        }
        else if (Team2ExpTextHopTimer <= FeelData.TextHopTime)
        {
            Team2ExpText.GetComponent<TextMeshProUGUI>().enabled = true;
            Team2ExpText.transform.localScale = Vector3.one * Mathf.Lerp(FeelData.TextHopScale, FeelData.TextDefaultScale,  (Team2ExpTextHopTimer - FeelData.TextHopTime / 2) / (FeelData.TextHopTime / 2));
        }
        else if (Team2ExpTextHopTimer <= FeelData.TextHopTime + FeelData.ExpTextStayTime)
        {
            Team2ExpText.GetComponent<TextMeshProUGUI>().enabled = true;
            Team2ExpText.transform.localScale = Vector3.one * FeelData.TextDefaultScale;
        }
        else
        {
            Team2ExpText.GetComponent<TextMeshProUGUI>().enabled = false;
            Team2ExpText.transform.localScale = Vector3.one * FeelData.TextDefaultScale;
            Team2AccumulatedExp = 0;
        }
    }

    private void SetUI()
    {
        Team1LevelText.GetComponent<TextMeshProUGUI>().text = Team1Level.ToString();
        Team1ExpText.GetComponent<TextMeshProUGUI>().text = "+" + Team1AccumulatedExp.ToString();
        Team1ExpCounter.GetComponent<Image>().fillAmount = (float)Team1Exp / Data.LevelUpExp;

        Team2LevelText.GetComponent<TextMeshProUGUI>().text = Team2Level.ToString();
        Team2ExpText.GetComponent<TextMeshProUGUI>().text = "+" + Team2AccumulatedExp.ToString();
        Team2ExpCounter.GetComponent<Image>().fillAmount = (float)Team2Exp / Data.LevelUpExp;
    }

    private void SetCheckPointScale()
    {
        Team1CheckPointHopTimer += Time.deltaTime;
        Team2CheckPointHopTimer += Time.deltaTime;

        if (CurrentTeam1Checkpoints == CurrentTeam2Checkpoints)
        {
            for (int i = 0; i < CurrentTeam1Checkpoints; i++)
            {
                CheckPointList[i].transform.localScale = new Vector3(FeelData.NormalScale, 0.1f, FeelData.NormalScale);
            }

            for (int i = CurrentTeam1Checkpoints; i < CheckPointList.Count; i++)
            {
                CheckPointList[i].transform.localScale = new Vector3(FeelData.NormalScale, 0.1f, FeelData.NormalScale);
            }




        }
        else if (CurrentTeam1Checkpoints > CurrentTeam2Checkpoints)
        {

            for (int i = CheckPointList.Count / 2; i < CurrentTeam1Checkpoints; i++)
            {
                if (Team1CheckPointHopTimer <= FeelData.TextHopTime / 2)
                {
                    float Scale = Mathf.Lerp(FeelData.ActivatedScale, FeelData.HopScale, Team1CheckPointHopTimer / (FeelData.TextHopTime / 2));
                    CheckPointList[i].transform.localScale = new Vector3(Scale, 0.1f, Scale);
                }
                else if (Team1CheckPointHopTimer <= FeelData.TextHopTime)
                {
                    float Scale = Mathf.Lerp(FeelData.HopScale, FeelData.ActivatedScale, (Team1CheckPointHopTimer - FeelData.TextHopTime / 2) / (FeelData.TextHopTime / 2));
                    CheckPointList[i].transform.localScale = new Vector3(Scale, 0.1f, Scale);
                }
                else
                {
                    CheckPointList[i].transform.localScale = new Vector3(FeelData.ActivatedScale, 0.1f, FeelData.ActivatedScale);
                }
            }

            for (int i = CurrentTeam1Checkpoints; i < CheckPointList.Count; i++)
            {
                CheckPointList[i].transform.localScale = new Vector3(FeelData.NormalScale, 0.1f, FeelData.NormalScale);
            }
        }
        else
        {
            for (int i = 0; i < CurrentTeam1Checkpoints; i++)
            {
                CheckPointList[i].transform.localScale = new Vector3(FeelData.NormalScale, 0.1f, FeelData.NormalScale);
            }

            for (int i = CurrentTeam1Checkpoints; i < CheckPointList.Count / 2; i++)
            {
                if (Team2CheckPointHopTimer <= FeelData.TextHopTime / 2)
                {
                    float Scale = Mathf.Lerp(FeelData.ActivatedScale, FeelData.HopScale, Team2CheckPointHopTimer / (FeelData.TextHopTime / 2));
                    CheckPointList[i].transform.localScale = new Vector3(Scale, 0.1f, Scale);
                }
                else if (Team2CheckPointHopTimer <= FeelData.TextHopTime)
                {
                    float Scale = Mathf.Lerp(FeelData.HopScale, FeelData.ActivatedScale, (Team2CheckPointHopTimer - FeelData.TextHopTime / 2) / (FeelData.TextHopTime / 2));
                    CheckPointList[i].transform.localScale = new Vector3(Scale, 0.1f, Scale);
                }
                else
                {
                    CheckPointList[i].transform.localScale = new Vector3(FeelData.ActivatedScale, 0.1f, FeelData.ActivatedScale);
                }
            }
        }
    }

    private void SetCheckPointAppearance()
    {

        if (CurrentTeam1Checkpoints == CurrentTeam2Checkpoints)
        {
            for (int i = 0; i < CurrentTeam1Checkpoints; i++)
            {
                CheckPointList[i].GetComponent<Renderer>().material = FeelData.EmissiveRed;
                Material mat = CheckPointList[i].GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");
                //mat.SetColor("_EmissionColor", FeelData.RedCheckpointColor * FeelData.DeactivatedEmission);

                CheckPointList[i].transform.localScale = new Vector3(FeelData.NormalScale, 0.1f, FeelData.NormalScale);
            }

            for (int i = CurrentTeam1Checkpoints; i < CheckPointList.Count; i++)
            {
                CheckPointList[i].GetComponent<Renderer>().material = FeelData.EmissiveBlue;
                Material mat = CheckPointList[i].GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");
                //mat.SetColor("_EmissionColor", FeelData.BlueCheckpointColor * FeelData.DeactivatedEmission);

                CheckPointList[i].transform.localScale = new Vector3(FeelData.NormalScale, 0.1f, FeelData.NormalScale);
            }

            


        }
        else if(CurrentTeam1Checkpoints > CurrentTeam2Checkpoints)
        {
            for (int i = 0; i < CurrentTeam1Checkpoints; i++)
            {
                CheckPointList[i].GetComponent<Renderer>().material = FeelData.EmissiveRed;
                Material mat = CheckPointList[i].GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");
                //mat.SetColor("_EmissionColor", FeelData.RedCheckpointColor * FeelData.ActivatedEmission);

                
            }

            for(int i= CheckPointList.Count / 2 - 1; i < CurrentTeam1Checkpoints; i++)
            {
                if(Team1CheckPointHopTimer <= FeelData.TextHopTime / 2)
                {
                    float Scale = Mathf.Lerp(FeelData.ActivatedScale, FeelData.HopScale, Team1CheckPointHopTimer / (FeelData.TextHopTime / 2));
                    CheckPointList[i].transform.localScale = new Vector3(Scale , 0.1f, Scale);
                }
                else if(Team1CheckPointHopTimer <= FeelData.TextHopTime)
                {
                    float Scale = Mathf.Lerp(FeelData.HopScale, FeelData.ActivatedScale, (Team1CheckPointHopTimer- FeelData.TextHopTime / 2) / (FeelData.TextHopTime / 2));
                    CheckPointList[i].transform.localScale = new Vector3(Scale, 0.1f, Scale);
                }
                else
                {
                    CheckPointList[i].transform.localScale = new Vector3(FeelData.ActivatedScale, 0.1f, FeelData.ActivatedScale);
                }
            }

            for (int i = CurrentTeam1Checkpoints; i < CheckPointList.Count; i++)
            {
                CheckPointList[i].GetComponent<Renderer>().material = FeelData.EmissiveBlue;
                Material mat = CheckPointList[i].GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");
                //mat.SetColor("_EmissionColor", FeelData.BlueCheckpointColor * FeelData.DeactivatedEmission);

                CheckPointList[i].transform.localScale = new Vector3(FeelData.NormalScale, 0.1f, FeelData.NormalScale);
            }
        }
        else
        {
            for (int i = 0; i < CurrentTeam1Checkpoints; i++)
            {
                CheckPointList[i].GetComponent<Renderer>().material = FeelData.EmissiveRed;
                Material mat = CheckPointList[i].GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");
                //mat.SetColor("_EmissionColor", FeelData.RedCheckpointColor * FeelData.DeactivatedEmission);

                CheckPointList[i].transform.localScale = new Vector3(FeelData.NormalScale, 0.1f, FeelData.NormalScale);
            }

            for (int i = CurrentTeam1Checkpoints; i < CheckPointList.Count; i++)
            {
                CheckPointList[i].GetComponent<Renderer>().material = FeelData.EmissiveBlue;
                Material mat = CheckPointList[i].GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");
                //mat.SetColor("_EmissionColor", FeelData.BlueCheckpointColor * FeelData.ActivatedEmission);
            }


            for (int i = CurrentTeam1Checkpoints; i < CheckPointList.Count / 2; i++)
            {

                if (Team2CheckPointHopTimer <= FeelData.TextHopTime / 2)
                {
                    float Scale = Mathf.Lerp(FeelData.ActivatedScale, FeelData.HopScale, Team2CheckPointHopTimer / (FeelData.TextHopTime / 2));
                    CheckPointList[i].transform.localScale = new Vector3(Scale, 0.1f, Scale);
                }
                else if (Team2CheckPointHopTimer <= FeelData.TextHopTime)
                {
                    float Scale = Mathf.Lerp(FeelData.HopScale, FeelData.ActivatedScale, (Team2CheckPointHopTimer - FeelData.TextHopTime / 2) / (FeelData.TextHopTime / 2));
                    CheckPointList[i].transform.localScale = new Vector3(Scale, 0.1f, Scale);
                }
                else
                {
                    CheckPointList[i].transform.localScale = new Vector3(FeelData.ActivatedScale, 0.1f, FeelData.ActivatedScale);
                }
            }
        }


    }

    private bool PlayerInCart(GameObject Player)
    {
        Vector3 Offset = Cart.transform.position - Player.transform.position;
        if (Offset.y <= 5)
        {
            Offset.y = 0;
        }
        return Offset.magnitude <= CartRadius;
    }

    private void CountCheckPointExp()
    {
        if(CurrentTeam1Checkpoints > CurrentTeam2Checkpoints)
        {
            CheckPointExpTimer += Time.deltaTime;
            if (CheckPointExpTimer >= Data.CheckPointExpTimeInterval)
            {
                GainExp((CurrentTeam1Checkpoints - CheckPointList.Count / 2) * Data.CheckPointExp,true);
                CheckPointExpTimer = 0;

                Team1CheckPointHopTimer = 0;
            }

        }
        else if (CurrentTeam2Checkpoints > CurrentTeam1Checkpoints)
        {
            CheckPointExpTimer += Time.deltaTime;
            if (CheckPointExpTimer >= Data.CheckPointExpTimeInterval)
            {
                GainExp((CurrentTeam2Checkpoints - CheckPointList.Count / 2) * Data.CheckPointExp, false);
                CheckPointExpTimer = 0;

                Team2CheckPointHopTimer = 0;
            }
        }
        else
        {
            CheckPointExpTimer = 0;
        }

        SetUI();
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if(GameEnd || !GameStart)
        {
            return;
        }


        int Amount = Data.KillExp;

        if (e.Player.tag.Contains("1"))
        {

            if (Team1Level > Team2Level)
            {
                Amount += Data.KillExpBonusPerLevelDif * (Team1Level - Team2Level);
            }
            GainExp(Amount, false);
        }
        else
        {
            if (Team2Level > Team1Level)
            {
                Amount += Data.KillExpBonusPerLevelDif * (Team2Level - Team1Level);
            }
            GainExp(Amount, true);
        }

        SetUI();
    }

    private void GainExp(int Amount, bool Team1)
    {
        if (Team1)
        {
            Team1Exp += Amount;
            if (Team1Exp >= Data.LevelUpExp)
            {
                int LevelUp = Team1Exp / Data.LevelUpExp;
                Team1Level += LevelUp;
                Team1Exp -= LevelUp * Data.LevelUpExp;

                Team1LevelTextHopTimer = 0;
            }

            Team1ExpTextHopTimer = 0;
            Team1AccumulatedExp += Amount;

        }
        else
        {
            Team2Exp += Amount;
            if (Team2Exp >= Data.LevelUpExp)
            {
                int LevelUp = Team2Exp / Data.LevelUpExp;
                Team2Level += LevelUp;
                Team2Exp -= LevelUp * Data.LevelUpExp;

                Team2LevelTextHopTimer = 0;
            }

            Team2ExpTextHopTimer = 0;
            Team2AccumulatedExp += Amount;
        }


    }

    private void OnGameStart(GameStart e)
    {
        GameStart = true;
    }
}
