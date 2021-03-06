﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NetworkServices
{
    private static NetworkAudioManager _audiomanager;
    public static NetworkAudioManager AudioManager
    {
        get
        {
            Debug.Assert(_audiomanager != null);
            return _audiomanager;
        }
        set
        {
            _audiomanager = value;
        }
    }

    private static NetworkGameFeelManager _gameFeelManager;
    public static NetworkGameFeelManager GameFeelManager
    {
        get
        {
            Debug.Assert(_gameFeelManager != null);
            return _gameFeelManager;
        }
        set
        {
            _gameFeelManager = value;
        }
    }

    private static NetworkVFXManager _visualEffectManager;
    public static NetworkVFXManager VisualEffectManager
    {
        get
        {
            Debug.Assert(_visualEffectManager != null);
            return _visualEffectManager;
        }
        set
        {
            _visualEffectManager = value;
        }
    }

    private static WeaponGenerationManager _weaponGenerationManager;
    public static WeaponGenerationManager WeaponGenerationManager
    {
        get
        {
            Debug.Assert(_weaponGenerationManager != null);
            return _weaponGenerationManager;
        }
        set
        {
            _weaponGenerationManager = value;
        }
    }

    private static NetworkStatisticManager _statisticsmanager;
    public static NetworkStatisticManager StatisticsManager
    {
        get
        {
            Debug.Assert(_statisticsmanager != null);
            return _statisticsmanager;
        }
        set
        {
            _statisticsmanager = value;
        }
    }

    private static TinylyticsHandler _tinylyticsmanager;
    public static TinylyticsHandler TinylyticsManager
    {
        get
        {
            Debug.Assert(_tinylyticsmanager != null);
            return _tinylyticsmanager;
        }
        set
        {
            _tinylyticsmanager = value;
        }
    }

    private static Config _config;
    public static Config Config
    {
        get
        {
            Debug.Assert(_config != null);
            return _config;
        }
        set
        {
            _config = value;
        }
    }

    private static NetworkGameStateManager _gamestatemanager;
    public static NetworkGameStateManager GameStateManager
    {
        get
        {
            Debug.Assert(_gamestatemanager != null);
            return _gamestatemanager;
        }
        set
        {
            _gamestatemanager = value;
        }
    }

    private static ObjectiveManager _gameObjectiveManager;
    public static ObjectiveManager GameObjectiveManager
    {
        get
        {
            Debug.Assert(_gameObjectiveManager != null);
            return _gameObjectiveManager;
        }
        set
        {
            _gameObjectiveManager = value;
        }
    }
}
