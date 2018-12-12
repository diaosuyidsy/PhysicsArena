using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombVFXController : MonoBehaviour
{
    private GameObject[] BombVFXs;
    private int index = 0;

    private void Start ()
    {
        // Initialized the bombs
        BombVFXs = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            BombVFXs[i] = transform.GetChild (i).gameObject;
        }
    }

    public void VisualEffect (int curwp)
    {

    }
}
