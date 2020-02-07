﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CanonState
{
    Unactivated,
    Cooldown,
    Firing
}

public class BrawlModeReforgedArenaManager : MonoBehaviour
{
    private class CanonInfo
    {
        public GameObject Entity;
        public CanonState State;
        public float Timer;
        public GameObject CooldownMark;
        public GameObject Mark;
        public GameObject LockedPlayer;


        public CanonInfo(GameObject canon, CanonState state, GameObject cdmark)
        {
            Entity = canon;
            State = state;
            CooldownMark = cdmark;
            CooldownMark.SetActive(false);

            Timer = 0;
            Mark = null;
            LockedPlayer = null;
        }

        public void TransitionTo(CanonState state)
        {
            State = state;
            Timer = 0;

            switch (State)
            {
                case CanonState.Unactivated:
                    CooldownMark.SetActive(false);
                    break;
                case CanonState.Cooldown:
                    CooldownMark.SetActive(true);
                    break;
                case CanonState.Firing:
                    CooldownMark.SetActive(true);
                    break;
            }
        }
    }

    public BrawlModeReforgedModeData Data;

    public GameObject Bagel;
    public GameObject Team1Canon;
    public GameObject Team2Canon;
    public GameObject Team1CanonCooldownMark;
    public GameObject Team2CanonCooldownMark;

    private CanonInfo Team1CanonInfo;
    private CanonInfo Team2CanonInfo;

    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;
    public GameObject Players;

    public GameObject MarkPrefab;
    public GameObject ExplosionVFX;
    public Color MarkDefaultColor;
    public Color MarkAlertColor;

    // Start is called before the first frame update
    void Start()
    {
        Team1CanonInfo = new CanonInfo(Team1Canon, CanonState.Unactivated,Team1CanonCooldownMark);
        Team2CanonInfo = new CanonInfo(Team2Canon, CanonState.Unactivated,Team2CanonCooldownMark);

        EventManager.Instance.AddHandler<BagelSent>(OnBagelSent);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<BagelSent>(OnBagelSent);
    }

    // Update is called once per frame
    void Update()
    {
        CheckCanon();
    }

    private void CheckCanon()
    {
        if(Team1CanonInfo.State != CanonState.Unactivated)
        {
            if(Team1CanonInfo.State == CanonState.Cooldown)
            {
                CanonCooldown(Team1CanonInfo);
            }
            else
            {
                CanonFiring(Team1CanonInfo);
            }
        }
        else if(Team2CanonInfo.State != CanonState.Unactivated)
        {
            if (Team2CanonInfo.State == CanonState.Cooldown)
            {
                CanonCooldown(Team2CanonInfo);
            }
            else
            {
                CanonFiring(Team2CanonInfo);
            }
        }
    }

    private void CanonCooldown(CanonInfo Info)
    {
        Info.Timer += Time.deltaTime;

        Info.CooldownMark.GetComponent<Image>().fillAmount = Info.Timer / Data.CanonCooldown;

        if(Info.Timer >= Data.CanonCooldown)
        {
            Info.TransitionTo(CanonState.Firing);
            LockPlayer(Info);
        }

    }

    private void CanonFiring(CanonInfo Info)
    {
        Info.Timer += Time.deltaTime;
        if(Info.Timer >= Data.CanonFireTime)
        {
            BagelFall(Info);
            Info.TransitionTo(CanonState.Cooldown);
        }
        else if(Info.Timer >= Data.CanonFireTime - Data.CanonFireAlertTime)
        {
            Info.Mark.GetComponent<SpriteRenderer>().color = MarkAlertColor;
        }

        MarkFollow(Info);
    }

    private void MarkFollow(CanonInfo Info)
    {
        Vector3 Offset = Info.LockedPlayer.transform.position - Info.Mark.transform.position;
        Offset.y = 0;
        float Dis = Offset.magnitude;

        if (Dis >= 0.01f)
        {
            if (Dis >= Data.FollowSpeed * Time.deltaTime)
            {
                Info.Mark.transform.position += Data.FollowSpeed * Time.deltaTime * Offset.normalized;
            }
            else
            {
                Info.Mark.transform.position += Dis * Offset.normalized;
            }
        }
    }

    private void LockPlayer(CanonInfo Info)
    {
        List<GameObject> AvailablePlayer = new List<GameObject>();
        List<Vector3> AvailablePoint = new List<Vector3>();

        foreach (Transform child in Players.transform)
        {
            if (child.GetComponent<PlayerController>().enabled && (child.tag.Contains("1") && Info.Entity == Team2Canon || child.tag.Contains("2") && Info.Entity == Team1Canon))
            {
                RaycastHit hit;
                if (Physics.Raycast(child.position, Vector3.down, out hit, 5, GroundLayer))
                {
                    if (hit.collider != null)
                    {
                        AvailablePlayer.Add(child.gameObject);
                        AvailablePoint.Add(hit.point);

                    }
                }
            }

        }

        if (AvailablePlayer.Count > 0)
        {
            int index = Random.Range(0, AvailablePlayer.Count);

            GameObject Mark = GameObject.Instantiate(MarkPrefab);
            Mark.GetComponent<SpriteRenderer>().color = MarkDefaultColor;
            Mark.transform.position = AvailablePoint[index] + Vector3.up * 0.01f;

            Info.Mark = Mark;
            Info.LockedPlayer = AvailablePlayer[index];

        }
    }

    private void BagelFall(CanonInfo Info)
    {
        RaycastHit[] AllHits = Physics.SphereCastAll(Info.Mark.transform.position, Data.CanonRadius, Vector3.up, 0, PlayerLayer);

        for (int i = 0; i < AllHits.Length; i++)
        {
            GameObject Player;

            if (AllHits[i].collider.gameObject.GetComponent<PlayerController>())
            {
                Player = AllHits[i].collider.gameObject;
            }
            else if (AllHits[i].collider.gameObject.GetComponentInParent<PlayerController>())
            {
                Player = AllHits[i].collider.gameObject.GetComponentInParent<PlayerController>().gameObject;
            }
            else
            {
                Player = AllHits[i].collider.gameObject.GetComponent<WeaponBase>().Owner;
            }

            Vector3 Offset = Player.transform.position - Info.Mark.transform.position;
            Offset.y = 0;

            if (Mathf.Abs(Offset.x) < 0.01f && Mathf.Abs(Offset.z) < 0.01f)
            {
                float angle = Random.Range(0, 2 * Mathf.PI);

                Offset.x = Mathf.Sin(angle);
                Offset.z = Mathf.Cos(angle);
            }

            Player.GetComponent<IHittable>().OnImpact(Data.CanonPower * Offset.normalized, ForceMode.Impulse, Player, ImpactType.BazookaGun);
        }

        GameObject.Instantiate(ExplosionVFX, Info.Mark.transform.position, ExplosionVFX.transform.rotation);
        Destroy(Info.Mark);
    }

    private void OnBagelSent(BagelSent e)
    {
        if(e.Canon == Team1Canon)
        {
            if(Team1CanonInfo.State == CanonState.Unactivated)
            {
                Team1CanonInfo.State = CanonState.Firing;
                Team2CanonInfo.TransitionTo(CanonState.Unactivated);
            }
        }
        else
        {
            if (Team2CanonInfo.State == CanonState.Unactivated)
            {
                Team2CanonInfo.State = CanonState.Firing;
                Team1CanonInfo.TransitionTo(CanonState.Unactivated);
            }
        }
    }
}
