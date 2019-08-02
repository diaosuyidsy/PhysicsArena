using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public AudioData AudioData;
	public VFXData VFXData;
	public ConfigData ConfigData;
	public WeaponData WeaponData;
	public GameMapData GameMapData;

	private void Awake()
	{
		Services.Config = new Config(ConfigData);
		Services.AudioManager = new AudioManager(AudioData);
		Services.GameFeelManager = new GameFeelManager();
		Services.VisualEffectManager = new VFXManager(VFXData);
		Services.WeaponGenerationManager = new WeaponGenerationManager(GameMapData, WeaponData, transform.Find("Weapons").gameObject);
		Services.StatisticsManager = new StatisticsManager();
		Services.TinylyticsManager = new TinylyticsHandler();
		Services.GameStateManager = new GameStateManager(GameMapData);
	}

	// Update is called once per frame
	void Update()
	{
		Services.WeaponGenerationManager.Update();
		Services.GameStateManager.Update();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawCube(new Vector3(0f, 6.5f, 0f), GameMapData.WeaponSpawnerSize);
		Gizmos.color = new Color(0, 0, 0, 0.5f);
		Gizmos.DrawCube(GameMapData.WorldCenter, GameMapData.WorldSize);
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
	}
}
