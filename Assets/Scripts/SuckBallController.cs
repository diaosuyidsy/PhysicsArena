using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuckBallController : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> InRangePlayers;

    public GameObject lineEndPrefab;
    private rtSuck _rts;
    private string _opponentTeamTag = "";
    private List<GameObject> _lineEnds = new List<GameObject>();
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();

    private void Awake()
    {
        _rts = GetComponentInParent<rtSuck>();
        InRangePlayers = new List<GameObject>();
    }

    private void OnEnable()
    {
        // _opponentTeamTag = _gpc.Owner.CompareTag("Team1") ? "Team2" : "Team1";
        _opponentTeamTag = "Team";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_rts.isSucking()) return;
        GameObject go = other.gameObject;
        if (go.tag.Contains(_opponentTeamTag) && other.GetType() == typeof(CapsuleCollider))
        {
            /*GameObject lrContainer = new GameObject("LineRenderer");
            lrContainer.transform.parent = transform;*/

            // Add a linerenderer to the in range players and configure the linerenderer properly
            if (go.GetComponent<LineRenderer>() != null) return;
            LineRenderer lr = go.AddComponent<LineRenderer>();
            //lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.material = new Material(Shader.Find("SuckGunLine"));
            //lr.widthMultiplier = 0.18f;
            lr.widthCurve = AnimationCurve.Linear(0, 0.05f, 1, 0.1f);
            lr.positionCount = 2;
            lr.numCapVertices = 90;
            lr.useWorldSpace = true;
            lr.startColor = Color.yellow;
            lr.sortingOrder = -1;
            // Add the gameobject to the list of InRangePlayers
            InRangePlayers.Add(go);

            GameObject lineEnd = GameObject.Instantiate(lineEndPrefab, transform);
            //lineEnd.transform.localScale = Vector3.one * 0.5f;
            lineEnd.SetActive(true);
            _lineEnds.Add(lineEnd);
            _lineRenderers.Add(lr);
        }
    }

    public void MakeLineSicker()
    {
        foreach (var lr in _lineRenderers)
        {
            lr.widthMultiplier += 2f * Time.deltaTime;
        }
    }

    private void Update()
    {
        for (int i = 0; i < InRangePlayers.Count; i++)
        {
            Vector3 playerPos = _lineRenderers[i].gameObject.transform.position;
            Vector3 ballPos = transform.position;
            _lineRenderers[i].SetPosition(0, playerPos);
            _lineRenderers[i].SetPosition(1, ballPos);
            float dis = Vector3.Distance(playerPos, ballPos);
            if (dis < 0.8f)
            {
                _lineRenderers[i].widthMultiplier = 3f;
            }
            else
            {
                _lineRenderers[i].widthMultiplier = 1 / Vector3.Distance(playerPos, ballPos) * 2.5f;
            }

            Vector3 dir = playerPos - ballPos;
            _lineEnds[i].transform.position = ballPos;
            _lineEnds[i].transform.forward = dir;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains(_opponentTeamTag) && other.GetType() == typeof(CapsuleCollider) && !_rts.isSucking())
        {
            _lineRenderers.Remove(other.gameObject.GetComponent<LineRenderer>());
            Destroy(other.gameObject.GetComponent<LineRenderer>());
            if (_lineEnds.Count > 0)
            {
                GameObject lineEnd = _lineEnds[0];
                _lineEnds.Remove(_lineEnds[0]);
                Destroy(lineEnd);
            }

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
            _lineRenderers.Remove(go.GetComponent<LineRenderer>());
            Destroy(go.GetComponent<LineRenderer>());
            if (_lineEnds.Count > 0)
            {
                GameObject lineEnd = _lineEnds[0];
                _lineEnds.Remove(_lineEnds[0]);
                Destroy(lineEnd);
            }
        }
        InRangePlayers = new List<GameObject>();
        _lineEnds.Clear();
        _lineRenderers.Clear();
    }
}
