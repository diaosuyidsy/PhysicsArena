using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkGame : NetworkBehaviour
{
    public CharacterData CharacterData;
    public AudioData AudioData;
    public VFXData VFXData;
    public ConfigData ConfigData;
    public GameFeelData GameFeelData;
    public ModeSepcificData ModeSpecificData;
    public GameMapData GameMapData;

    public ModeSepcificData ModeSpecificData_2Player;

    public override void OnStartClient()
    {
        NetworkServices.Config = new Config(ConfigData, GameMapData, CharacterData);
        // NetworkServices.AudioManager = new NetworkAudioManager(AudioData);
        // NetworkServices.GameFeelManager = new NetworkGameFeelManager(GameFeelData);
        NetworkServices.VisualEffectManager = new NetworkVFXManager(VFXData, this);
        // NetworkServices.WeaponGenerationManager = new WeaponGenerationManager(GameMapData);
        // NetworkServices.StatisticsManager = new StatisticsManager();
        // NetworkServices.TinylyticsManager = new TinylyticsHandler();
        // NetworkServices.GameStateManager = new NetworkGameStateManager(GameMapData, ConfigData, gameObject);
        // switch (GameMapData.GameMapMode)
        // {
        //     case GameMapMode.FoodCartMode:
        //         NetworkServices.GameObjectiveManager = new FoodModeObjectiveManager();
        //         break;
        //     case GameMapMode.BrawlMode:
        //         NetworkServices.GameObjectiveManager = new BrawlModeObjectiveManager((BrawlModeData)ModeSpecificData);
        //         break;
        //     case GameMapMode.DeathMode:
        //         if (Utility.GetPlayerNumber() <= 2)
        //         {
        //             NetworkServices.GameObjectiveManager = new BrawlModeReforgedObjectiveManager((BrawlModeReforgedModeData)ModeSpecificData_2Player);
        //         }
        //         else
        //         {
        //             NetworkServices.GameObjectiveManager = new BrawlModeReforgedObjectiveManager((BrawlModeReforgedModeData)ModeSpecificData);
        //         }
        //         break;
        //     case GameMapMode.RaceMode:
        //         NetworkServices.GameObjectiveManager = new SushiModeObjectiveManager((SushiModeData)ModeSpecificData);
        //         break;
        //     case GameMapMode.SoccerMode:
        //         NetworkServices.GameObjectiveManager = new SoccerModeObjectiveManager((SoccerMapData)ModeSpecificData);
        //         break;
        //     default:
        //         NetworkServices.GameObjectiveManager = new EmptyObjectiveManager();
        //         break;
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // NetworkServices.WeaponGenerationManager.Update();
        // NetworkServices.GameStateManager.Update();
        // NetworkServices.GameObjectiveManager.Update();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(new Vector3(0f, 6.5f, 0f), GameMapData.WeaponSpawnerSize);
        Gizmos.color = new Color(0, 0, 0, 0.5f);
        Gizmos.DrawCube(GameMapData.WorldCenter, GameMapData.WorldSize);
        for (int i = 0; i < GameMapData.ChickenLandingPosition.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GameMapData.ChickenLandingPosition[i], 0.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(GameMapData.DuckLandingPostion[i], 0.2f);
        }
        for (int i = 0; i < GameMapData.Team1RespawnPoints.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GameMapData.Team1RespawnPoints[i], 0.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(GameMapData.Team2RespawnPoints[i], 0.2f);
        }
    }

    public override void OnNetworkDestroy()
    {
        // NetworkServices.AudioManager.Destroy();
        // NetworkServices.AudioManager = null;

        // NetworkServices.GameFeelManager.Destory();
        // NetworkServices.GameFeelManager = null;

        NetworkServices.VisualEffectManager.Destory();
        NetworkServices.VisualEffectManager = null;

        // NetworkServices.StatisticsManager.Destory();
        // NetworkServices.StatisticsManager = null;

        // NetworkServices.TinylyticsManager.Destory();
        // NetworkServices.TinylyticsManager = null;

        NetworkServices.Config.Destroy();
        NetworkServices.Config = null;

        // NetworkServices.GameStateManager.Destroy();
        // NetworkServices.GameStateManager = null;

        // NetworkServices.GameObjectiveManager.Destroy();
        // NetworkServices.GameObjectiveManager = null;
    }
}
