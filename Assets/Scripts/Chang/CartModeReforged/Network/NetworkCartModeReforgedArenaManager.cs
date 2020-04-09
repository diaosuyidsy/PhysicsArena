using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class NetworkCartModeReforgedArenaManager : NetworkBehaviour
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

    public GameObject LevelUpTimerCounter;

    public GameObject OccupyCanvas;
    public GameObject OccupyCounter;

    public GameObject Team1SpeedLevelText;
    public GameObject Team2SpeedLevelText;
    public GameObject Team1SpeedLevelPlusText;
    public GameObject Team2SpeedLevelPlusText;

    [SyncVar(hook = nameof(SetTeam1SpeedLevel))]
    private int Team1SpeedLevel;
    [SyncVar(hook = nameof(SetTeam2SpeedLevel))]
    private int Team2SpeedLevel;
    [SyncVar(hook = nameof(SetTeam1SpeedLevelPlus))]
    private int Team1SpeedLevelPlus;
    [SyncVar(hook = nameof(SetTeam2SpeedLevelPlus))]
    private int Team2SpeedLevelPlus;


    private List<GameObject> CheckPointList;
    private int TargetWayPointIndex;

    [SyncVar(hook =nameof(SetDotline))]
    private CartSide CurrentSide;
    [SyncVar]
    private CartSide LastSide;

    [SyncVar]
    private CartState CurrentState;

    [SyncVar]
    private float CurrentSpeed;

    [SyncVar(hook = nameof(SetOccupyCircle))]
    private float CurrentOccupyProgress;

    private bool GameStart;
    private bool GameEnd;

    [SyncVar(hook = nameof(SetLevelUpTimerCounter))]
    private float SpeedLevelTimer;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);

        Team1SpeedLevel = Team2SpeedLevel = 1;
        Team1SpeedLevelPlus = Team2SpeedLevelPlus = 1;

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
        if (!isServer)
        {
            return;
        }

        OccupyCanvas.transform.position = new Vector3(Cart.transform.position.x, 0.1f, Cart.transform.position.z);

        ManageSpeed();

        DetectCartSide();
        if (CurrentState == CartState.Moving)
        {
            CartMove();
        }
        else
        {
            CartOccupy();
        }

    }

    private void SetLevelUpTimerCounter(float oldValue,float newValue)
    {
        LevelUpTimerCounter.GetComponent<Image>().fillAmount = newValue / Data.SpeedUpTime;
    }

    private void ManageSpeed()
    {
        if (GameEnd || !GameStart)
        {
            return;
        }

        SpeedLevelTimer += Time.deltaTime;
        if (SpeedLevelTimer >= Data.SpeedUpTime)
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
        }
    }

    private void DetectCartSide()
    {
        int Team1Count = 0;
        int Team2Count = 0;

        foreach (Transform child in Players.transform)
        {
            if (child.gameObject.activeSelf && PlayerInCart(child.gameObject))
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

        if (Team1Count > 0 && Team2Count > 0)
        {
            CurrentSide = CartSide.Neutral;
            CurrentSpeed = 0;
        }
        else if (Team1Count > 0)
        {
            CurrentSide = CartSide.Team1;
            CurrentSpeed = Data.CartSpeedWithCheckpoint[Team1SpeedLevel - 1];
        }
        else if (Team2Count > 0)
        {
            CurrentSide = CartSide.Team2;
            CurrentSpeed = Data.CartSpeedWithCheckpoint[Team2SpeedLevel - 1];
        }
        else
        {
            CurrentSide = CartSide.Neutral;
            CurrentSpeed = 0;
        }
    }

    private void SetDotline(CartSide oldValue,CartSide newValue)
    {
        switch (newValue)
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
            return;
        }

        if (CurrentSide == CartSide.Team1)
        {
            CurrentOccupyProgress += Data.OccupySpeedWithCheckpoint[Team1SpeedLevel - 1] * Time.deltaTime;
        }
        else
        {
            CurrentOccupyProgress += Data.OccupySpeedWithCheckpoint[Team2SpeedLevel - 1] * Time.deltaTime;
        }

        if (CurrentOccupyProgress >= 1)
        {
            CurrentOccupyProgress = 0;
            CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Occupied = true;
            CheckPointList[TargetWayPointIndex].GetComponent<MeshRenderer>().enabled = false;

            RpcDeactivateCheckpoint(CheckPointList[TargetWayPointIndex]);



            if (CurrentSide == CartSide.Team1)
            {
                Team1SpeedLevel+= CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score;
                //Team1SpeedLevelPlus += CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score;

                if (Team1SpeedLevel > Data.MaxLevel)
                {
                    Team1SpeedLevel = Data.MaxLevel;
                }



                RpcGenerateCheckpointScore(CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score, CheckPointList[TargetWayPointIndex].transform.position, true);

                TargetWayPointIndex++;
                if (TargetWayPointIndex >= CheckPointList.Count)
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

                RpcGenerateCheckpointScore(CheckPointList[TargetWayPointIndex].GetComponent<Checkpoint>().Score, CheckPointList[TargetWayPointIndex].transform.position, false);

                TargetWayPointIndex--;
                if (TargetWayPointIndex < 0)
                {
                    GameEnd = true;
                    EventManager.Instance.TriggerEvent(new GameEnd(2, Cart.transform, GameWinType.CartWin));
                    TargetWayPointIndex = 0;

                    return;
                }
            }

            CurrentState = CartState.Moving;

        }
    }

    [ClientRpc]
    private void RpcDeactivateCheckpoint(GameObject Checkpoint)
    {
        Checkpoint.GetComponent<Checkpoint>().Occupied = true;
        Checkpoint.GetComponent<MeshRenderer>().enabled = false;
    }

    [ClientRpc]
    private void RpcGenerateCheckpointScore(int Amount, Vector3 Pos, bool Team1)
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


        if (CurrentOccupyProgress < 0)
        {
            CurrentOccupyProgress = 0;
        }

        if (CurrentSide == CartSide.Team1)
        {
            if (TargetWayPointIndex < 0)
            {
                TargetWayPointIndex = CheckPointList.Count / 2;
            }
            else if (LastSide == CartSide.Team2)
            {
                TargetWayPointIndex++;
            }
        }
        else if (CurrentSide == CartSide.Team2)
        {
            if (TargetWayPointIndex < 0)
            {
                TargetWayPointIndex = CheckPointList.Count / 2 - 1;
            }
            else if (LastSide == CartSide.Team1)
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

        if (Offset.magnitude > 0)
        {
            Cart.transform.position += CurrentSpeed * Offset.normalized * Time.deltaTime;
            Cart.transform.forward = Offset.normalized;
        }



        Vector3 NewOffset = CheckPointList[TargetWayPointIndex].transform.position - Cart.transform.position;
        NewOffset.y = 0;

        if (Vector3.Dot(NewOffset, Offset) <= 0)
        {
            Cart.transform.position = new Vector3(CheckPointList[TargetWayPointIndex].transform.position.x, Cart.transform.position.y, CheckPointList[TargetWayPointIndex].transform.position.z);

            if (Occupiable(CheckPointList[TargetWayPointIndex]))
            {
                CurrentState = CartState.Occupying;
            }
            else
            {
                if (CurrentSide == CartSide.Team1)
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

        foreach (Transform child in CheckPoints.transform)
        {
            CheckPointList.Add(child.gameObject);
        }

    }


    private void SetTeam1SpeedLevel(int oldValue,int newValue)
    {
        Team1SpeedLevelText.GetComponent<TextMeshProUGUI>().text = newValue.ToString();
        if (Team1SpeedLevel < 5)
        {
            Team1SpeedLevelText.transform.localScale = FeelData.DefaultLevelTextScale * Vector3.one;
        }
        else if (Team1SpeedLevel < 10)
        {
            Team1SpeedLevelText.transform.localScale = FeelData.BiggerLevelTextScale * Vector3.one;
        }
        else
        {
            Team1SpeedLevelText.transform.localScale = FeelData.LargestLevelTextScale * Vector3.one;
        }
    }

    private void SetTeam2SpeedLevel(int oldValue,int newValue)
    {
        Team2SpeedLevelText.GetComponent<TextMeshProUGUI>().text = newValue.ToString();
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

    private void SetTeam1SpeedLevelPlus(int oldValue, int newValue)
    {
        Team1SpeedLevelPlusText.GetComponent<TextMeshProUGUI>().text = "+" + newValue.ToString();
    }

    private void SetTeam2SpeedLevelPlus(int oldValue,int newValue)
    {
        Team2SpeedLevelPlusText.GetComponent<TextMeshProUGUI>().text = "+" + newValue.ToString();
    }

    private void SetOccupyCircle(float oldValue, float newValue)
    {
        OccupyCounter.GetComponent<Image>().fillAmount = newValue;
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
        if (!isServer)
        {
            return;
        }

        if (GameEnd || !GameStart)
        {
            return;
        }
    }

    private void OnGameStart(GameStart e)
    {
        GameStart = true;
    }
}
