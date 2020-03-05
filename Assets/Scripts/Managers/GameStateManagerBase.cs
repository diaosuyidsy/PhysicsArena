using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameStateManagerBase
{
    public PlayerController[] PlayerControllers;
    public PlayerInformation PlayersInformation;
    public List<Transform> CameraTargets;

    public GameMapData _gameMapdata;
    public GameStateManagerBase()
    {
    }

    public virtual void Update()
    {
    }

    public virtual void Destroy()
    {
    }
}
