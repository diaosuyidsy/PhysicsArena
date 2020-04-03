using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CheckpointSide
{
    Team1,
    Team2
}

public class Checkpoint : MonoBehaviour
{
    public CheckpointSide Side;
    public int Score;

    public bool Occupied;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
