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
        Services.AudioManager = new AudioManager(AudioData);
        Services.GameFeelManager = new GameFeelManager(GameFeelData);
        Services.VisualEffectManager = new VFXManager(VFXData);
    }

    private void OnDestroy()
    {
        Services.AudioManager.Destroy();
        Services.AudioManager = null;

        Services.GameFeelManager.Destory();
        Services.GameFeelManager = null;

        Services.VisualEffectManager.Destory();
        Services.VisualEffectManager = null;

        Services.Config.Destroy();
        Services.Config = null;

    }
}
