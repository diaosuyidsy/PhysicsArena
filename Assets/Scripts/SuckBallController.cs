using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuckBallController : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> InRangePlayers
    {
        get
        {
            return new List<GameObject>(InRangePlayersDict.Keys);
        }
    }

    public Material lineMat;
    public GameObject lineEndPrefab;
    public SuckGunData SuckGunData;
    public Transform LineRendererContainer;
    private rtSuck _rts;

    // Key is Sucked Object, Value is Corresponding LineRendererObject
    private Dictionary<GameObject, GameObject> InRangePlayersDict = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        _rts = GetComponentInParent<rtSuck>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_rts.isSucking()) return;
        GameObject go = other.gameObject;
        if (!InRangePlayersDict.ContainsKey(go) && go.GetComponent<IHittable>() != null && (((1 << go.layer) | SuckGunData.CanSuckLayer) == SuckGunData.CanSuckLayer))
        {
            GameObject lro = _getOrCreateLineRendererObject();
            InRangePlayersDict.Add(go, lro);
        }
    }

    private GameObject _getOrCreateLineRendererObject()
    {
        Debug.Assert(LineRendererContainer != null);
        for (int i = 0; i < LineRendererContainer.childCount; i++)
        {
            GameObject go = LineRendererContainer.GetChild(i).gameObject;
            if (!InRangePlayersDict.ContainsValue(go))
            {
                go.GetComponent<LineRenderer>().enabled = true;
                go.transform.GetChild(0).gameObject.SetActive(true);
                return go;
            }
        }
        // Create LineRendererObject
        GameObject LineRendererObject = new GameObject("LineRenderer");
        LineRendererObject.transform.parent = LineRendererContainer;
        LineRendererObject.transform.localPosition = Vector3.one;
        LineRendererObject.transform.localScale = Vector3.one;
        // Add and setup Linerenderer Component to the object
        LineRenderer lr = LineRendererObject.AddComponent<LineRenderer>();
        lr.material = lineMat;
        lr.widthCurve = AnimationCurve.Linear(0, 0.05f, 1, 0.1f);
        lr.positionCount = 2;
        lr.numCapVertices = 90;
        lr.useWorldSpace = true;
        lr.startColor = Color.yellow;
        lr.sortingOrder = -1;
        // Create LineEnd Object In LineRendererObject
        GameObject lineEnd = GameObject.Instantiate(lineEndPrefab, LineRendererObject.transform);
        lineEnd.SetActive(true);
        return LineRendererObject;
    }

    private void _disableLineRendererFromDict(GameObject player)
    {
        if (!InRangePlayersDict.ContainsKey(player)) return;
        LineRenderer lr = InRangePlayersDict[player].GetComponent<LineRenderer>();
        lr.enabled = false;
        InRangePlayersDict[player].transform.GetChild(0).gameObject.SetActive(false);
    }

    private void Update()
    {
        foreach (var kvPair in InRangePlayersDict)
        {
            GameObject LineRendererObject = kvPair.Value;
            LineRenderer linerenderer = LineRendererObject.GetComponent<LineRenderer>();
            GameObject SuckedObject = kvPair.Key;
            Vector3 playerPos = SuckedObject.transform.position;
            Vector3 ballPos = transform.position;
            linerenderer.SetPosition(0, playerPos);
            linerenderer.SetPosition(1, ballPos);
            float dis = Vector3.Distance(playerPos, ballPos);
            if (dis < 0.8f)
            {
                linerenderer.widthMultiplier = 3f;
            }
            else
            {
                linerenderer.widthMultiplier = 1 / Vector3.Distance(playerPos, ballPos) * 2.5f;
            }
            Vector3 dir = playerPos - ballPos;
            LineRendererObject.transform.GetChild(0).position = ballPos;
            LineRendererObject.transform.GetChild(0).forward = dir;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<IHittable>() != null)
        {
            if (!InRangePlayersDict.ContainsKey(other.gameObject)) return;
            _disableLineRendererFromDict(other.gameObject);
            InRangePlayersDict.Remove(other.gameObject);
        }
    }

    // Clean all linerenders and clear the InRangePlayers
    // Call this when done with ball suck
    public void CleanUpAll()
    {
        foreach (var kvpair in InRangePlayersDict)
        {
            _disableLineRendererFromDict(kvpair.Key);
        }
        InRangePlayersDict.Clear();
    }
}
