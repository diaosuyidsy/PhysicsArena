using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuckBallController : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> InRangePlayers;

    private GunPositionControl _gpc;
    private rtSuck _rts;
    private string _opponentTeamTag = "";

    private void Awake()
    {
        _gpc = GetComponentInParent<GunPositionControl>();
        _rts = GetComponentInParent<rtSuck>();
        InRangePlayers = new List<GameObject>();
    }

    private void OnEnable()
    {
        _opponentTeamTag = _gpc.Owner.CompareTag("Team1") ? "Team2" : "Team1";
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        if (go.CompareTag(_opponentTeamTag) && other.GetType() == typeof(CapsuleCollider))
        {
            // Add a linerenderer to the in range players and configure the linerenderer properly
            if (go.GetComponent<LineRenderer>() != null) return;
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.widthMultiplier = 0.02f;
            lr.positionCount = 2;
            lr.numCapVertices = 90;
            lr.useWorldSpace = true;
            lr.startColor = Color.yellow;
            // Add the gameobject to the list of InRangePlayers
            InRangePlayers.Add(go);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(_opponentTeamTag) && other.GetType() == typeof(CapsuleCollider))
        {
            LineRenderer lr = other.GetComponent<LineRenderer>();
            lr.SetPosition(0, other.transform.position);
            lr.SetPosition(1, transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_opponentTeamTag) && other.GetType() == typeof(CapsuleCollider) && !_rts.isSucking())
        {
            Destroy(other.gameObject.GetComponent<LineRenderer>());
            // Delete the gameobject from the list in range players
            foreach (GameObject go in InRangePlayers)
            {
                if (go == other.gameObject)
                {
                    InRangePlayers.Remove(other.gameObject);
                    break;
                }
            }
        }
    }

    // Clean all linerenders and clear the InRangePlayers
    // Call this when done with ball suck
    public void CleanUpAll()
    {
        foreach (GameObject go in InRangePlayers)
        {
            Destroy(go.GetComponent<LineRenderer>());
        }
        InRangePlayers = new List<GameObject>();
    }
}
