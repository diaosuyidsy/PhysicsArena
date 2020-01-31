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
    public float ApocalypseTime;
    public GameObject Trigger;

    private ApocalypseTeam TargetTeam;
    private float Timer;

    // Start is called before the first frame update
    void Start()
    {
        TargetTeam = ApocalypseTeam.Neutral;

        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if(e.ImpactObject == Trigger)
        {
            switch (TargetTeam)
            {
                case ApocalypseTeam.Neutral:
                    if (e.Player.tag.Contains("1"))
                    {
                        TargetTeam = ApocalypseTeam.Red;
                    }
                    else
                    {
                        TargetTeam = ApocalypseTeam.Blue;
                    }

                    break;
                case ApocalypseTeam.Red:
                    if (e.Player.tag.Contains("2"))
                    {
                        TargetTeam = ApocalypseTeam.Blue;
                    }
                    break;
                case ApocalypseTeam.Blue:
                    if (e.Player.tag.Contains("1"))
                    {
                        TargetTeam = ApocalypseTeam.Red;
                    }
                    break;
            }
        }
    }
}
