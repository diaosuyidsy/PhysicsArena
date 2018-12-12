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

    public void VisualEffect (bool on, int curwp, int MAXWP)
    {
        foreach (GameObject go in BombVFXs)
        {
            go.GetComponent<ParticleSystem> ().Stop (true);
        }
        if (!on) return;
        int amount = Mathf.Abs ((MAXWP / 2 - curwp) / 2);
        for (int i = 0; i < amount; i++)
        {
            BombVFXs[i].GetComponent<ParticleSystem> ().Play (true);
        }
    }
}
