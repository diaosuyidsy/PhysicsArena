using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComicMenuGame : MonoBehaviour
{
    public CharacterData CharacterData;
    public AudioData AudioData;
    public VFXData VFXData;
    public ConfigData ConfigData;
    public GameFeelData GameFeelData;
    public GameMapData GameMapData;

    private void Awake()
    {
        Services.Config = new Config(ConfigData, GameMapData, CharacterData);
        Services.AudioManager = new AudioManager(AudioData, gameObject);
        Services.GameFeelManager = new GameFeelManager(GameFeelData);
        Services.VisualEffectManager = new VFXManager(VFXData);
        Services.GameStateManager = new MenuGameStateManager(GameMapData, ConfigData, gameObject);
        Services.ActionManager = new ActionManager();
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
    void Update()
    {
        Services.ActionManager.Update();
        Services.GameStateManager.Update();

    }
    private void OnDestroy()
    {
        Services.ActionManager.Destroy();
        Services.ActionManager = null;

        Services.AudioManager.Destroy();
        Services.AudioManager = null;

        Services.GameFeelManager.Destory();
        Services.GameFeelManager = null;

        Services.VisualEffectManager.Destory();
        Services.VisualEffectManager = null;

        Services.GameStateManager.Destroy();
        Services.GameStateManager = null;

        Services.Config.Destroy();
        Services.Config = null;

    }
}
