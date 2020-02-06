using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public CanonInfo(GameObject canon, CanonState state)
        {
            Entity = canon;
            State = state;
            Timer = 0;
        }

        public void Reset()
        {
            State = CanonState.Unactivated;
            Timer = 0;
        }
    }

    public GameObject Bagel;
    public GameObject Team1Canon;
    public GameObject Team2Canon;

    private CanonInfo Team1CanonInfo;
    private CanonInfo Team2CanonInfo;

    // Start is called before the first frame update
    void Start()
    {
        Team1CanonInfo = new CanonInfo(Team1Canon, CanonState.Unactivated);
        Team2CanonInfo = new CanonInfo(Team2Canon, CanonState.Unactivated);

        EventManager.Instance.AddHandler<BagelSent>(OnBagelSent);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<BagelSent>(OnBagelSent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnBagelSent(BagelSent e)
    {
        if(e.Canon == Team1Canon)
        {
            if(Team1CanonInfo.State == CanonState.Unactivated)
            {
                Team1CanonInfo.State = CanonState.Firing;
                Team2CanonInfo.Reset();
            }
        }
        else
        {
            if (Team2CanonInfo.State == CanonState.Unactivated)
            {
                Team2CanonInfo.State = CanonState.Firing;
                Team1CanonInfo.Reset();
            }
        }
    }
}
