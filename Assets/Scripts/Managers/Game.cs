using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public AudioData AudioData;
	public VFXData VFXData;
	public ConfigData ConfigData;
	public WeaponData WeaponData;

	private void Awake()
	{
		Services.Config = new Config(ConfigData);
		Services.AudioManager = new AudioManager(AudioData);
		Services.GameFeelManager = new GameFeelManager();
		Services.VisualEffectManager = new VFXManager(VFXData);
		Services.WeaponGenerationManager = new WeaponGenerationManager(WeaponData, transform.Find("Weapons").gameObject);
		Services.StatisticsManager = new StatisticsManager();
		Services.TinylyticsManager = new TinylyticsHandler();
	}

	// Update is called once per frame
	void Update()
	{
		Services.WeaponGenerationManager.Update();
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
	}
}
