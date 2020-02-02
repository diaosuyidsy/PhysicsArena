using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ApocalypseTeam
{
    Neutral,
    Red,
    Blue
}

public class ApocalypseManager : MonoBehaviour
{
    public GameObject Trigger;
    public GameObject MarkPrefab;
    public GameObject Players;

    public LayerMask GroundLayer;
    public ApocalypseArenaData Data;

    public Color MarkDefaultColor;
    public Color MarkAlertColor;

    public Material RedMat;
    public Material BlueMat;

    private ApocalypseTeam TargetTeam;

    private float PrepareTimer;
    private float ActivatedTimer;
    private bool InPreparation;

    private Dictionary<GameObject, GameObject> MarkToPlayer;

    private const float MinFollowDis = 0.01f;

    private const float MarkHeight = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        TargetTeam = ApocalypseTeam.Neutral;

        MarkToPlayer = new Dictionary<GameObject, GameObject>();
        ResetApocalypse();


        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
    }

    // Update is called once per frame
    void Update()
    {
        ApocalypseTiming();
        MarkFollowPlayer();
    }

    private void ApocalypseTiming()
    {
        if (TargetTeam == ApocalypseTeam.Red || TargetTeam == ApocalypseTeam.Blue)
        {
            if (InPreparation)
            {
                PrepareTimer += Time.deltaTime;
                if (PrepareTimer >= Data.ApocalypsePrepareTime)
                {
                    InPreparation = false;
                    ActivateApocalypse();
                }
            }
            else
            {
                ActivatedTimer += Time.deltaTime;
                if (ActivatedTimer >= Data.ApocalypseTime)
                {
                    ApocalypseFall();
                }
                else if(ActivatedTimer >= Data.ApocalypseTime - Data.ApocalypseAlertTime)
                {
                    foreach(GameObject mark in MarkToPlayer.Keys)
                    {
                        mark.GetComponent<SpriteRenderer>().color = MarkAlertColor;
                    }
                }
            }
        }
    }

    private void ApocalypseFall()
    {
        foreach(GameObject mark in MarkToPlayer.Keys)
        {
            Destroy(mark);
        }

        ResetApocalypse();
    }

    private void ActivateApocalypse()
    {
        foreach(Transform child in Players.transform)
        {
            if(child.GetComponent<PlayerController>().enabled && ( child.tag.Contains("1") && TargetTeam == ApocalypseTeam.Red || child.tag.Contains("2") && TargetTeam == ApocalypseTeam.Blue))
            {
                RaycastHit hit;
                if (Physics.Raycast(child.position, Vector3.down, out hit, 5, GroundLayer))
                {
                    if (hit.collider != null)
                    {
                        GameObject Mark = GameObject.Instantiate(MarkPrefab);
                        Mark.GetComponent<SpriteRenderer>().color = MarkDefaultColor;
                        Mark.transform.position = hit.point + Vector3.up * MarkHeight;

                        MarkToPlayer.Add(Mark, child.gameObject);
                    }
                }
            }

        }
    }

    private void MarkFollowPlayer()
    {
        foreach(GameObject mark in MarkToPlayer.Keys)
        {
            Vector3 Offset = MarkToPlayer[mark].transform.position - mark.transform.position;
            Offset.y = 0;
            float Dis = Offset.magnitude;

            if (Dis >= MinFollowDis)
            {
                if (Dis >= Data.ApocalypseFollowSpeed * Time.deltaTime)
                {
                    mark.transform.position += Data.ApocalypseFollowSpeed * Time.deltaTime * Offset.normalized;
                }
                else
                {
                    mark.transform.position += Dis * Offset.normalized;
                }
            }
        }
    }

    private void ResetApocalypse()
    {
        InPreparation = true;
        PrepareTimer = 0;
        ActivatedTimer = 0;
        MarkToPlayer.Clear();
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if (MarkToPlayer.ContainsValue(e.Player))
        {
            foreach(KeyValuePair<GameObject,GameObject> Pair in MarkToPlayer)
            {
                if(Pair.Value == e.Player)
                {
                    MarkToPlayer.Remove(Pair.Key);
                    Destroy(Pair.Key);
                    break;
                }
            }

            if (MarkToPlayer.Count == 0)
            {
                ResetApocalypse();
            }
        }

        if(e.ImpactObject == Trigger)
        {
            Material[] mats = GetComponent<Renderer>().materials;
            
            switch (TargetTeam)
            {
                case ApocalypseTeam.Neutral:
                    if (e.Player.tag.Contains("1"))
                    {
                        TargetTeam = ApocalypseTeam.Red;
                        mats[1] = RedMat;

                        ResetApocalypse();
                    }
                    else
                    {
                        TargetTeam = ApocalypseTeam.Blue;
                        mats[1] = BlueMat;

                        ResetApocalypse();
                    }

                    break;
                case ApocalypseTeam.Red:
                    if (e.Player.tag.Contains("2"))
                    {
                        TargetTeam = ApocalypseTeam.Blue;
                        mats[1] = BlueMat;

                        ResetApocalypse();
                    }
                    break;
                case ApocalypseTeam.Blue:
                    if (e.Player.tag.Contains("1"))
                    {
                        TargetTeam = ApocalypseTeam.Red;
                        mats[1] = RedMat;

                        ResetApocalypse();
                    }
                    break;
            }

            GetComponent<Renderer>().materials = mats;
        }
    }
}
