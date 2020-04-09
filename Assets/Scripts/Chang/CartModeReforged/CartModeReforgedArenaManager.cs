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

    private enum CartState
    {
        Moving,
        Occupying
    }

    public CartModeReforgedModeData Data;
    public CartModeReforgedFeelData FeelData;

    public GameObject Players;
    public GameObject Cart;
    public GameObject CheckPoints;

    public GameObject CheckpointScoreTextPrefab;

    public float CartRadius;
    public GameObject CartDotline;

    public GameObject LevelupTimerCounter;

    public GameObject OccupyCanvas;
    public GameObject OccupyCounter;

    public GameObject Team1SpeedLevelText;
    public GameObject Team2SpeedLevelText;
    public GameObject Team1SpeedLevelPlusText;
    public GameObject Team2SpeedLevelPlusText;

    private int Team1SpeedLevel;
    private int Team2SpeedLevel;
    private int Team1SpeedLevelPlus;
    private int Team2SpeedLevelPlus;


    private List<GameObject> CheckPointList;
    private int TargetWayPointIndex;

    private CartSide CurrentSide;
    private CartSide LastSide;
    private CartState CurrentState;
    private float CurrentSpeed;

    private float CurrentOccupyProgress;

    private bool GameStart;
    private bool GameEnd;

    private float SpeedLevelTimer;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);

        Team1SpeedLevel = Team2SpeedLevel = 1;
        Team1SpeedLevelPlus = Team2SpeedLevelPlus = 1;
        SetUI();

        TargetWayPointIndex = -1;
        CurrentSide = LastSide = CartSide.Neutral;
        CurrentState = CartState.Moving;
        CartDotline.GetComponent<SpriteRenderer>().color = FeelData.NeutralDotlineColor;

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
        OccupyCanvas.transform.position = new Vector3(Cart.transform.position.x, 0.1f, Cart.transform.position.z);

        ManageSpeed();

        DetectCartSide();
        SetDotline();
        if(CurrentState == CartState.Moving)
        {
            CartMove();
        }
        else
        {
            CartOccupy();
        }

    }

    private void ManageSpeed()
    {
        if (GameEnd || !GameStart)
        {
            return;
        }

        SpeedLevelTimer += Time.deltaTime;
        if(SpeedLevelTimer >= Data.SpeedUpTime)
        {
            Team1SpeedLevel += Team1SpeedLevelPlus;
            Team2SpeedLevel += Team2SpeedLevelPlus;
            SpeedLevelTimer = 0;

            if (Team1SpeedLevel > Data.MaxLevel)
            {
                Team1SpeedLevel = Data.MaxLevel;
            }

            if (Team2SpeedLevel > Data.MaxLevel)
            {
                Team2SpeedLevel = Data.MaxLevel;
            }

            SetUI();
        }

        LevelupTimerCounter.GetComponent<Image>().fillAmount = SpeedLevelTimer / Data.SpeedUpTime;
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

        if (Team1Count>0 && Team2Count>0)
        {
            CurrentSide = CartSide.Neutral;
            CurrentSpeed = 0;
        }
        else if (Team1Count > 0)
        {
            CurrentSide = CartSide.Team1;
            CurrentSpeed = Data.CartSpeedWithCheckpoint[Team1SpeedLevel-1];
        }
        else if(Team2Count > 0)
        {
            CurrentSide = CartSide.Team2;
            CurrentSpeed = Data.CartSpeedWithCheckpoint[Team2SpeedLevel-1];
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

    private void CartOccupy()
    {
        if (GameEnd || !GameStart)
        {
            return;
        }

        if (!Occupiable(CheckPointList[TargetWayPointIndex]))
        {
            CurrentState = CartState.Moving;

            CartMove();
            return;
        }

        if(CurrentSide == CartSide.Neutral)
        {
            CurrentOccupyProgress -= Data.RecoverSpeed * Time.deltaTime;
            if (CurrentOccupyProgress < 0)
            {
                CurrentOccupyProgress = 0;
            }
            OccupyCounter.GetComponent<Image>().fillAmount = CurrentOccupyProgress;
            return;
        }
        
        if(CurrentSide == CartSide.Team1)
        {
            CurrentOccupyProgress += Data.OccupySpeedWithCheckpoint[Team1SpeedLevel-1] * Time.deltaTime;
            OccupyCounter.GetComponent<Image>().fillAmount = CurrentOccupyProgress;
        }
        else
        {
            CurrentOccupyProgress += Data.OccupySpeedWithCheckpoint[Team2SpeedLevel-1] * Time.deltaTime;
            OccupyCounter.GetComponent<Image>().fillAmount = CurrentOccupyProgress;
        }



        if (CurrentOccupyProgress >= 1)
        {
            CurrentOccupyProgress = 0;
            CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Occupied = true;
            CheckPointList[TargetWayPointIndex].GetComponent<MeshRenderer>().enabled = false;



            if(CurrentSide == CartSide.Team1)
            {
                Team1SpeedLevel += CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score;
                //Team1SpeedLevelPlus += CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score;

                if (Team1SpeedLevel > Data.MaxLevel)
                {
                    Team1SpeedLevel = Data.MaxLevel;
                }

                GenerateCheckpointScore(CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score, CheckPointList[TargetWayPointIndex].transform.position, true);

                TargetWayPointIndex++;
                if(TargetWayPointIndex >= CheckPointList.Count)
                {
                    GameEnd = true;
                    EventManager.Instance.TriggerEvent(new GameEnd(1, Cart.transform, GameWinType.CartWin));
                    TargetWayPointIndex = CheckPointList.Count - 1;

                    return;
                }
            }
            else
            {
                Team2SpeedLevel += CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score;
                //Team2SpeedLevelPlus += CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score;

                if (Team2SpeedLevel > Data.MaxLevel)
                {
                    Team2SpeedLevel = Data.MaxLevel;
                }

                GenerateCheckpointScore(CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score, CheckPointList[TargetWayPointIndex].transform.position, false);

                TargetWayPointIndex--;
                if(TargetWayPointIndex < 0)
                {
                    GameEnd = true;
                    EventManager.Instance.TriggerEvent(new GameEnd(2, Cart.transform, GameWinType.CartWin));
                    TargetWayPointIndex = 0;

                    return;
                }
            }

            CurrentState = CartState.Moving;

            SetUI();

        }
    }

    private void GenerateCheckpointScore(int Amount, Vector3 Pos, bool Team1)
    {
        GameObject Text = GameObject.Instantiate(CheckpointScoreTextPrefab);
        Text.transform.position = Pos;
        Text.GetComponent<TextMesh>().text = "+" + Amount.ToString();
        if (Team1)
        {
            Text.GetComponent<TextMesh>().color = FeelData.Team1DotlineColor;
        }
        else
        {
            Text.GetComponent<TextMesh>().color = FeelData.Team2DotlineColor;
        }
    }

    private bool Occupiable(GameObject Checkpoint)
    {
        return !Checkpoint.GetComponent<Checkpoint>().Occupied && (CurrentSide != CartSide.Team2 && Checkpoint.GetComponent<Checkpoint>().Side == CheckpointSide.Team2 || CurrentSide != CartSide.Team1 && Checkpoint.GetComponent<Checkpoint>().Side == CheckpointSide.Team1);
    }

    private void CartMove()
    {
        if (GameEnd || !GameStart)
        {
            return;
        }

        CurrentOccupyProgress -= Data.RecoverSpeed * Time.deltaTime;
        OccupyCounter.GetComponent<Image>().fillAmount = CurrentOccupyProgress;


        if (CurrentOccupyProgress < 0)
        {
            CurrentOccupyProgress = 0;
        }

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

        if(Offset.magnitude > 0)
        {
            Cart.transform.position += CurrentSpeed * Offset.normalized * Time.deltaTime;
            Cart.transform.forward = Offset.normalized;
        }



        Vector3 NewOffset = CheckPointList[TargetWayPointIndex].transform.position - Cart.transform.position;
        NewOffset.y = 0;

        if (Vector3.Dot(NewOffset, Offset)<=0)
        {
            Cart.transform.position = new Vector3(CheckPointList[TargetWayPointIndex].transform.position.x, Cart.transform.position.y, CheckPointList[TargetWayPointIndex].transform.position.z);

            if (Occupiable(CheckPointList[TargetWayPointIndex]))
            {
                CurrentState = CartState.Occupying;
            }
            else
            {
                if(CurrentSide == CartSide.Team1)
                {
                    TargetWayPointIndex++;
                }
                else
                {
                    TargetWayPointIndex--;
                }
            }
        }


    }

    private void InitCheckPoint()
    {
        CheckPointList = new List<GameObject>();

        foreach(Transform child in CheckPoints.transform)
        {
            CheckPointList.Add(child.gameObject);
        }

    }


    private void SetUI()
    {
        Team1SpeedLevelText.GetComponent<TextMeshProUGUI>().text = Team1SpeedLevel.ToString();
        Team2SpeedLevelText.GetComponent<TextMeshProUGUI>().text = Team2SpeedLevel.ToString();

        Team1SpeedLevelPlusText.GetComponent<TextMeshProUGUI>().text = "+" + Team1SpeedLevelPlus.ToString();
        Team2SpeedLevelPlusText.GetComponent<TextMeshProUGUI>().text = "+" + Team2SpeedLevelPlus.ToString();

        if(Team1SpeedLevel < 5)
        {
            Team1SpeedLevelText.transform.localScale = FeelData.DefaultLevelTextScale * Vector3.one;
        }
        else if(Team1SpeedLevel < 10)
        {
            Team1SpeedLevelText.transform.localScale = FeelData.BiggerLevelTextScale* Vector3.one;
        }
        else
        {
            Team1SpeedLevelText.transform.localScale = FeelData.LargestLevelTextScale * Vector3.one;
        }

        if (Team2SpeedLevel < 5)
        {
            Team2SpeedLevelText.transform.localScale = FeelData.DefaultLevelTextScale * Vector3.one;
        }
        else if (Team2SpeedLevel < 10)
        {
            Team2SpeedLevelText.transform.localScale = FeelData.BiggerLevelTextScale * Vector3.one;
        }
        else
        {
            Team2SpeedLevelText.transform.localScale = FeelData.LargestLevelTextScale * Vector3.one;
        }

    }

   

    private bool PlayerInCart(GameObject Player)
    {
        Vector3 Offset = Cart.transform.position - Player.transform.position;
        if (Player.transform.position.y <= 5)
        {
            Offset.y = 0;
        }
        else
        {
            return false;
        }

        return Offset.magnitude <= CartRadius;
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if(GameEnd || !GameStart)
        {
            return;
        }
    }


    private void OnGameStart(GameStart e)
    {
        GameStart = true;
    }
}
