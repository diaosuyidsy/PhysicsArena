using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config
{
    public ConfigData ConfigData;
    public GameMapData GameMapData;
    public Vector3[] Team1RespawnPoints;
    public Vector3[] Team2RespawnPoints;

    public Config(ConfigData _cd, GameMapData _gmp)
    {
        ConfigData = _cd;
        GameMapData = _gmp;
        Team1RespawnPoints = new Vector3[GameMapData.Team1RespawnPoints.Length];
        for (int i = 0; i < Team1RespawnPoints.Length; i++)
        {
            Team1RespawnPoints[i] = new Vector3(GameMapData.Team1RespawnPoints[i].x, GameMapData.Team1RespawnPoints[i].y, GameMapData.Team1RespawnPoints[i].z);
        }

        Team2RespawnPoints = new Vector3[GameMapData.Team2RespawnPoints.Length];
        for (int i = 0; i < Team2RespawnPoints.Length; i++)
        {
            Team2RespawnPoints[i] = new Vector3(GameMapData.Team2RespawnPoints[i].x, GameMapData.Team2RespawnPoints[i].y, GameMapData.Team2RespawnPoints[i].z);
        }
    }

    public void Destroy()
    {
    }
}
