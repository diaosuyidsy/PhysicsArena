using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameNetwork : MonoBehaviour
{
    public AudioData AudioData;
    public VFXData VFXData;
    public ConfigData ConfigData;
    public GameFeelData GameFeelData;
    public BrawlModeData BrawlModeData;
    public GameMapData GameMapData;

    private void Awake()
    {
        ServicesNetwork.NetworkEventManager = new NetworkEventManager();
        ServicesNetwork.Config = null;
        ServicesNetwork.AudioManager = new AudioManager(AudioData);
        ServicesNetwork.GameFeelManager = new GameFeelNetworkManager(GameFeelData);
        ServicesNetwork.VisualEffectManager = new VFXNetworkManager(VFXData);
        // ServicesNetwork.WeaponGenerationManager = new WeaponGenerationManager(GameMapData, WeaponData);
        // ServicesNetwork.StatisticsManager = new StatisticsManager();
        // ServicesNetwork.TinylyticsManager = new TinylyticsHandler();
        ServicesNetwork.GameStateManager = new GameStateNetworkManager(GameMapData, ConfigData, gameObject);
        // switch (GameMapData.GameMapMode)
        // {
        //     case GameMapMode.FoodCartMode:
        //         Services.GameObjectiveManager = new FoodModeObjectiveManager();
        //         break;
        //     case GameMapMode.BrawlMode:
        //         Services.GameObjectiveManager = new BrawlModeObjectiveManager(BrawlModeData);
        //         break;
        //     default:
        //         ServicesNetwork.GameObjectiveManager = new EmptyObjectiveManager();
        //         break;
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // ServicesNetwork.WeaponGenerationManager.Update();
        // ServicesNetwork.GameStateManager.Update();
        // ServicesNetwork.GameObjectiveManager.Update();
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

    private void OnDestroy()
    {
        ServicesNetwork.AudioManager.Destroy();
        ServicesNetwork.AudioManager = null;

        ServicesNetwork.GameFeelManager.Destory();
        ServicesNetwork.GameFeelManager = null;

        ServicesNetwork.VisualEffectManager.Destory();
        ServicesNetwork.VisualEffectManager = null;

        // ServicesNetwork.StatisticsManager.Destory();
        // ServicesNetwork.StatisticsManager = null;

        // ServicesNetwork.TinylyticsManager.Destory();
        // ServicesNetwork.TinylyticsManager = null;

        ServicesNetwork.Config.Destroy();
        ServicesNetwork.Config = null;

        ServicesNetwork.GameStateManager.Destroy();
        ServicesNetwork.GameStateManager = null;

        ServicesNetwork.NetworkEventManager.Destroy();
        ServicesNetwork.NetworkEventManager = null;

        // ServicesNetwork.GameObjectiveManager.Destroy();
        // ServicesNetwork.GameObjectiveManager = null;
    }
}
