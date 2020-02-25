using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonFeelData : MonoBehaviour
{
    public float MaxLeverAngle;
    public float MaxPipeAngle;
    public float LeverRotateSpeed;
    public float PipeRotateSpeed;

    public Vector3 PipeEndStartLocalPos;
    public float PipeEndFireTime;
    public float PipeEndRecoverTime;
    public float PipeEndShakeDis;

    public float MarkAppearTime;
    public Color MarkDefaultColor;
    public Color MarkAlertColor;

    public GameObject BombPrefab;
    public float BombInvisibleHeight;
    public float BombRiseTime;
    public float BombFallTime;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
