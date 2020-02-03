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
    public LayerMask PlayerLayer;
    public ApocalypseArenaData Data;

    public GameObject ExplosionVFX;

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
        Dictionary<GameObject, Vector3> PlayerToDir = new Dictionary<GameObject, Vector3>();

        foreach(GameObject mark in MarkToPlayer.Keys)
        {
            RaycastHit[] AllHits = Physics.SphereCastAll(mark.transform.position, Data.ApocalypseRadius, Vector3.up, 0, PlayerLayer);

            for(int i = 0; i < AllHits.Length; i++)
            {
                GameObject Player;

                if (AllHits[i].collider.gameObject.GetComponent<PlayerController>())
                {
                    Player = AllHits[i].collider.gameObject;
                }
                else
                {
                    Player = AllHits[i].collider.gameObject.GetComponentInParent<PlayerController>().gameObject;
                }

                Vector3 Offset = Player.transform.position - mark.transform.position;
                Offset.y = 0;

                if(Mathf.Abs(Offset.x) < 0.01f && Mathf.Abs(Offset.z) < 0.01f)
                {
                    float angle = Random.Range(0, 2 * Mathf.PI);

                    Offset.x = Mathf.Sin(angle);
                    Offset.z = Mathf.Cos(angle);
                }

                if (PlayerToDir.ContainsKey(Player))
                {
                    PlayerToDir[Player] = (PlayerToDir[Player] + Offset.normalized).normalized;
                }
                else
                {
                    PlayerToDir.Add(Player, Offset.normalized);
                }
                GameObject.Instantiate(ExplosionVFX, mark.transform.position, ExplosionVFX.transform.rotation);
            }
            Destroy(mark);
        }

        foreach(GameObject player in PlayerToDir.Keys)
        {
            player.GetComponent<IHittable>().OnImpact(Data.ApocalypsePower * PlayerToDir[player], ForceMode.Impulse, player, ImpactType.BazookaGun);
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

        foreach(GameObject mark in MarkToPlayer.Keys)
        {
            Destroy(mark);
        }

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
