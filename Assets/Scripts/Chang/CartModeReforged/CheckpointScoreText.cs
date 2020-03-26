using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointScoreText : MonoBehaviour
{
    public float StartScale;
    public float HopScale;
    public float HopTime;
    public float LifeTime;

    private float Timer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        if(Timer <= HopTime / 2)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(StartScale, HopScale, Timer / (HopTime / 2));
        }
        else if(Timer <= HopTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(HopScale, StartScale,  (Timer-HopTime/2) / (HopTime / 2));
        }
        else if(Timer >= LifeTime)
        {
            Destroy(gameObject);
        }

        
    }
}
