using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public CharacterData CharacterData;
    public AudioData AudioData;
    public VFXData VFXData;
    public ConfigData ConfigData;
    public WeaponData WeaponData;
    public GameFeelData GameFeelData;
    public ModeSepcificData ModeSpecificData;
    public GameMapData GameMapData;

    public ModeSepcificData ModeSpecificData_2Player;

    private void Awake()
    {
        Services.Config = new Config(ConfigData, GameMapData, CharacterData);
        Services.AudioManager = new AudioManager(AudioData);
        Services.GameFeelManager = new GameFeelManager(GameFeelData);
        Services.VisualEffectManager = new VFXManager(VFXData);
        Services.WeaponGenerationManager = new WeaponGenerationManager(GameMapData, WeaponData);
        Services.StatisticsManager = new StatisticsManager();
        Services.TinylyticsManager = new TinylyticsHandler();
        Services.GameStateManager = new GameStateManager(GameMapData, ConfigData, gameObject);
        switch (GameMapData.GameMapMode)
        {
            case GameMapMode.FoodCartMode:
                Services.GameObjectiveManager = new FoodModeObjectiveManager();
                break;
            case GameMapMode.BrawlMode:
                Services.GameObjectiveManager = new BrawlModeObjectiveManager((BrawlModeData)ModeSpecificData);
                break;
            case GameMapMode.DeathMode:
                if (Utility.GetPlayerNumber() <= 2)
                {
                    Services.GameObjectiveManager = new BrawlModeReforgedObjectiveManager((BrawlModeReforgedModeData)ModeSpecificData_2Player);
                }
                else
                {
                    Services.GameObjectiveManager = new BrawlModeReforgedObjectiveManager((BrawlModeReforgedModeData)ModeSpecificData);
                }
                Services.GameObjectiveManager = new BrawlModeReforgedObjectiveManager((BrawlModeReforgedModeData)ModeSpecificData);
                break;
            case GameMapMode.RaceMode:
                Services.GameObjectiveManager = new SushiModeObjectiveManager((SushiModeData)ModeSpecificData);
                break;
            case GameMapMode.SoccerMode:
                Services.GameObjectiveManager = new SoccerModeObjectiveManager((SoccerMapData)ModeSpecificData);
                break;
            default:
                Services.GameObjectiveManager = new EmptyObjectiveManager();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Services.WeaponGenerationManager.Update();
        Services.GameStateManager.Update();
        Services.GameObjectiveManager.Update();
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
        Services.AudioManager.Destroy();
        Services.AudioManager = null;

        Services.GameFeelManager.Destory();
        Services.GameFeelManager = null;

        Services.VisualEffectManager.Destory();
        Services.VisualEffectManager = null;

        Services.StatisticsManager.Destory();
        Services.StatisticsManager = null;

        Services.TinylyticsManager.Destory();
        Services.TinylyticsManager = null;

        Services.Config.Destroy();
        Services.Config = null;

        Services.GameStateManager.Destroy();
        Services.GameStateManager = null;

        Services.GameObjectiveManager.Destroy();
        Services.GameObjectiveManager = null;
    }
}
