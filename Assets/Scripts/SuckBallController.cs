using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuckBallController : MonoBehaviour
{

    private GunPositionControl _gpc;
    private string _opponentTeamTag = "";

    private void Awake()
    {
        _gpc = GetComponentInParent<GunPositionControl>();
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
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.widthMultiplier = 0.02f;
            lr.positionCount = 2;
            lr.numCapVertices = 90;
            lr.useWorldSpace = false;
            lr.startColor = Color.yellow;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(_opponentTeamTag) && other.GetType() == typeof(CapsuleCollider))
        {
            LineRenderer lr = other.GetComponent<LineRenderer>();
            Vector3 diff = -transform.position + other.transform.position;
            diff.y *= -1f;
            lr.SetPosition(1, diff);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_opponentTeamTag) && other.GetType() == typeof(CapsuleCollider))
        {
            Destroy(other.gameObject.GetComponent<LineRenderer>());
        }
    }
}
