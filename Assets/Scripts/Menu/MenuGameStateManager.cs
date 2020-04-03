using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuGameStateManager : GameStateManagerBase
{
    public MenuGameStateManager(GameMapData _gmp, ConfigData _cfd, GameObject _gm)
    {
        _gameMapdata = _gmp;
        PlayersInformation = new PlayerInformation(new int[6] { -1, -1, -1, -1, -1, -1 }, new int[6] { -1, -1, -1, -1, -1, -1 }, new int[6] { -1, -1, -1, -1, -1, -1 });
        CameraTargets = new List<Transform>();
        PlayerControllers = new PlayerController[PlayersInformation.ColorIndex.Length];
    }

    public void SetPlayerInformation(int rewiredID, int gameplayerID, int colorindex)
    {
        PlayersInformation.RewiredID[colorindex] = rewiredID;
        PlayersInformation.GamePlayerID[colorindex] = gameplayerID;
        PlayersInformation.ColorIndex[colorindex] = colorindex;
    }

    public void ClearPlayerInformation(int colorIndex)
    {
        PlayersInformation.RewiredID[colorIndex] = -1;
        PlayersInformation.GamePlayerID[colorIndex] = -1;
        PlayersInformation.ColorIndex[colorIndex] = -1;
    }
}
