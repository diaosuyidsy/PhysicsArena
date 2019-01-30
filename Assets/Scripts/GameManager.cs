﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Rewired;
using UnityEngine.SocialPlatforms.Impl;


public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    public Image EndImageBackground;
    [HideInInspector]
    public List<GameObject> Players;
    [Tooltip("Need to fill all six Beforehand")]
    public GameObject[] APlayers;
    public Dictionary<string, Material> NameToMaterialsDict;
    public LayerMask AllPlayers;
    public Transform RespawnPointHolder;
    public GameObject[] Team1ResourceRespawnPoints;
    public GameObject[] Team2ResrouceRespawnPoints;
    [HideInInspector]
    public int Team1ResourceSpawnIndex = 0;
    [HideInInspector]
    public int Team2ResourceSpawnIndex = 0;
    [HideInInspector]
    public GameObject[] Team1RespawnPts;
    [HideInInspector]
    public GameObject[] Team2RespawnPts;
    public float RespawnTime = 5f;
    [Tooltip("This is CD for on drop respawn method")]
    public float WeaponRespawnTime = 2f;
    public float DropWeaponVelocityThreshold = 2f;
    public string[] PlayerCanPickupTags;
    public GameObject TeamRed1Explosion;
    public GameObject TeamBlue2Explosion;
    public Color EndGameBackgroundImageColor;
    public Material[] CharacterMaterials;
    [HideInInspector]
    public int Winner;
    [HideInInspector]
    public int SuckGunTutorialTimes = 6;

    #region Stats Variable
    [HideInInspector]
    public List<int> SuicideRecord;
    [HideInInspector]
    public List<int> KillRecord;
    [HideInInspector]
    public List<int> TeammateMurderRecord;
    [HideInInspector]
    public List<float> CartTime;
    [HideInInspector]
    public List<int> BlockTimes;
    [HideInInspector]
    public List<int> FoodScoreTimes;
    [HideInInspector]
    public List<float> WaterGunUseTime;
    [HideInInspector]
    public List<int> HookGunUseTimes;
    [HideInInspector]
    public List<int> HookGunSuccessTimes;
    [HideInInspector]
    public List<int> SuckedPlayersTimes;
    [HideInInspector]
    public PlayerInformation[] PlayersInformation;
    #endregion

    private int Team1RespawnIndex = 0;
    private int Team2RespawnIndex = 0;

    //teleport a weapon to a random position within a given space
    public Vector3 weaponSpawnerCenter;
    public Vector3 weaponSpawnerSize;

    // Need to manually set up world center until we figure out a way to automatically do it.
    [Header("World Limit Setting")]
    public Vector3 WorldCenter;
    public Vector3 WorldSize;

    public int weaponTracker = 2;

    private bool _won = false;
    private Player _player;
    private DarkCornerEffect _dcfx;


    private void Awake()
    {
        GM = this;
        Players = new List<GameObject>();
        NameToMaterialsDict = new Dictionary<string, Material>()
        {
            {"Yellow", CharacterMaterials[0] },
            {"Pink", CharacterMaterials[1] },
            {"Orange", CharacterMaterials[2] },
            {"Blue", CharacterMaterials[3] },
            {"Purple", CharacterMaterials[4] },
            {"Green", CharacterMaterials[5] },

        };
        SuicideRecord = new List<int>(new int[] { 0, 0, 0, 0, 0, 0 });
        KillRecord = new List<int>(new int[] { 0, 0, 0, 0, 0, 0 });
        TeammateMurderRecord = new List<int>(new int[] { 0, 0, 0, 0, 0, 0 });
        CartTime = new List<float>(new float[] { 0f, 0f, 0f, 0f, 0f, 0f }); // Initialize The cart time to be 6 zeroes
        BlockTimes = new List<int>(new int[] { 0, 0, 0, 0, 0, 0 }); // Initialize the block time to be 6 zeroes
        FoodScoreTimes = new List<int>(new int[] { 0, 0, 0, 0, 0, 0 }); // Initalize the Food Score time to be 6 zeroes
        WaterGunUseTime = new List<float>(new float[] { 0f, 0f, 0f, 0f, 0f, 0f });
        HookGunUseTimes = new List<int>(new int[] { 0, 0, 0, 0, 0, 0, });
        HookGunSuccessTimes = new List<int>(new int[] { 0, 0, 0, 0, 0, 0, });
        SuckedPlayersTimes = new List<int>(new int[] { 0, 0, 0, 0, 0, 0 });
        PlayersInformation = new PlayerInformation[6];
        _player = ReInput.players.GetPlayer(0);
        _dcfx = Camera.main.GetComponent<DarkCornerEffect>();
    }

    private void Start()
    {
        // Respawn Point Holder Setup
        Transform team1 = RespawnPointHolder.GetChild(0);
        Transform team2 = RespawnPointHolder.GetChild(1);
        Team1RespawnPts = new GameObject[team1.childCount];
        Team2RespawnPts = new GameObject[team2.childCount];
        for (int i = 0; i < team1.childCount; i++)
        {
            Team1RespawnPts[i] = team1.GetChild(i).gameObject;
        }
        for (int i = 0; i < team2.childCount; i++)
        {
            Team2RespawnPts[i] = team2.GetChild(i).gameObject;
        }
        SuckGunTutorialTimes = 4;
        //_fillPlayerInformation();
    }

    public void FillPlayerInformation()
    {
        _fillPlayerInformation();
    }

    private void _fillPlayerInformation()
    {
        if (CanvasController.CC == null)
        {
            PlayersInformation = new PlayerInformation[]
            {
                new PlayerInformation("Yellow", Color.yellow),
                new PlayerInformation("Orange", Color.red),
                new PlayerInformation("Pink", Color.magenta),
                new PlayerInformation("Blue", Color.blue),
                new PlayerInformation("Purple", Color.cyan),
                new PlayerInformation("Green", Color.green),
            };
        }
        for (int i = 0; i < 6; i++)
        {
            if (CanvasController.CC != null)
                PlayersInformation[i] = CanvasController.CC.FinalInformation[i];
        }
        Destroy(CanvasController.CC.gameObject);
    }

    IEnumerator EndImageShow(float time, GameObject _tar = null)
    {
        Vector3 _tarPos = Vector3.zero;
        if (_tar != null)
        {
            _tarPos = new Vector3(_tar.transform.position.x, _tar.transform.position.y, _tar.transform.position.z);
        }
        float elapsedTime = 0f;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector2 targetPosition = Vector2.zero;

        // Set up CenterPosition
        if (_tar != null)
        {
            targetPosition = Camera.main.WorldToScreenPoint(_tarPos);
            targetPosition.y = screenHeight - targetPosition.y;
            _dcfx.CenterPosition = targetPosition;
        }

        float maxLength = getMaxLength(targetPosition);
        _dcfx.enabled = true;
        _dcfx.Length = maxLength;
        float deltaLength = maxLength * 0.75f / time;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            if (_tar != null)
            {
                targetPosition = Camera.main.WorldToScreenPoint(_tarPos);
                targetPosition.y = screenHeight - targetPosition.y;
                _dcfx.CenterPosition = Vector2.Lerp(_dcfx.CenterPosition, targetPosition, elapsedTime / time);
            }
            _dcfx.Length -= Time.deltaTime * deltaLength;
            yield return new WaitForEndOfFrame();
        }

        // Now the Image is focused on the winning objective
        // And displaying the winning party
        GetComponent<Scoreboard>().DisplayWinner();

        yield return new WaitForSeconds(3f);
        // TODO: The transition from focusing to completely black
        elapsedTime = 0f;
        deltaLength = maxLength * 0.45f / 1f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            _dcfx.Length -= Time.deltaTime * deltaLength;
            yield return new WaitForEndOfFrame();
        }
        // Now the screen should be entirely black
        // And we need to disable the screen dark corner effect
        _dcfx.enabled = false;
        // And here we need to let player be able to restart
        GetComponent<InputController>().CanRestart();
        yield return StartCoroutine(StartTransitionColor(2f));
        
        GetComponent<Scoreboard>().DisplayScore();
        //GetComponent<Scoreboard>().DisplayKiller();
        //yield return new WaitForSeconds(9f);
        //GetComponent<Scoreboard>().DisplaySuicider();
        //yield return new WaitForSeconds(9f);
        //GetComponent<Scoreboard>().DisplayTMKiller();
        //yield return new WaitForSeconds(9f);
        //GetComponent<Scoreboard>().DisplayBlocker();
    }

    // This function takes the center position on sreen
    // and calculates the maximum length of its position to the four corners
    private float getMaxLength(Vector2 centerposition)
    {
        Vector2[] screenVertices = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(Screen.width, 0f),
            new Vector2(Screen.width, Screen.height),
            new Vector2(0f, Screen.height),
        };

        float maxLength = 0f;
        for (int i = 0; i < 4; i++)
        {
            float length = Vector2.Distance(centerposition, screenVertices[i]);
            if (length > maxLength)
            {
                maxLength = length;
            }
        }
        return maxLength;
    }

    IEnumerator StartTransitionColor(float time)
    {
        float elapsedTime = 0f;
        EndImageBackground.gameObject.SetActive(true);
        Color initialColor = EndImageBackground.color;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            EndImageBackground.color = Color.Lerp(initialColor, EndGameBackgroundImageColor, elapsedTime / time);
            yield return new WaitForEndOfFrame();
        }
    }
    //make the space for weapon to respawn (weapon-spawner) visible in scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(weaponSpawnerCenter, weaponSpawnerSize);
        Gizmos.color = new Color(0, 0, 0, 0.5f);
        Gizmos.DrawCube(WorldCenter, WorldSize);
    }

    //move weapon to weapon-spawner
    public void HideWeapon(GameObject weapon)
    {
        MoveWeaponToSpawnArea(weapon);
        weaponTracker--;
        print("current gun = " + weaponTracker);
        weapon.SetActive(false);
        StartCoroutine(ActivateWeapon(WeaponRespawnTime, weapon));
    }

    public void MoveWeaponToSpawnArea(GameObject weapon)
    {
        Vector3 targetPos = weaponSpawnerCenter + new Vector3(Random.Range(-weaponSpawnerSize.x / 2, weaponSpawnerSize.x / 2), Random.Range(-weaponSpawnerSize.y / 2, weaponSpawnerSize.y / 2), Random.Range(-weaponSpawnerSize.z / 2, weaponSpawnerSize.z / 2));
        weapon.transform.position = targetPos;
    }

    //activate weapon after WeaponRespawnTime
    IEnumerator ActivateWeapon(float time, GameObject go)
    {
        yield return new WaitForSeconds(time);
        go.SetActive(true);
        weaponTracker++;
    }

    public void SetToRespawn(GameObject player, float yOffset)
    {
        if (player.CompareTag("Team1"))
        {
            Team1RespawnIndex = (Team1RespawnIndex + 1) % Team1RespawnPts.Length;
            Vector3 pos = Team1RespawnPts[Team1RespawnIndex].transform.position;
            pos.y += yOffset;
            player.transform.position = pos;
        }
        else
        {
            Team2RespawnIndex = (Team2RespawnIndex + 1) % Team2RespawnPts.Length;
            Vector3 pos = Team2RespawnPts[Team2RespawnIndex].transform.position;
            pos.y += yOffset;
            player.transform.position = pos;
        }
    }

    public void GameOver(int winner, GameObject _tar)
    {
        if (_won)
            return;
        Winner = winner;
        StartCoroutine(EndImageShow(3f, _tar));
        _won = true;
    }

    // This function is used for testing purposes only
    public void ForceGameOver(int winner)
    {
        GameOver(winner, APlayers[winner - 1]);
    }

    private void Update()
    {
        //// Debug for on cart time statistics
        //ConsoleProDebug.Watch("Player 0 On Cart Time", CartTime[0].ToString());
        //ConsoleProDebug.Watch("Player 1 On Cart Time", CartTime[1].ToString());
        //ConsoleProDebug.Watch("Player 2 On Cart Time", CartTime[2].ToString());
        //ConsoleProDebug.Watch("Player 3 On Cart Time", CartTime[3].ToString());
        //ConsoleProDebug.Watch("Player 4 On Cart Time", CartTime[4].ToString());
        //ConsoleProDebug.Watch("Player 5 On Cart Time", CartTime[5].ToString());
        //ConsoleProDebug.Watch("Player 0 Block Times", BlockTimes[0].ToString());
        //ConsoleProDebug.Watch("Player 1 Block Times", BlockTimes[1].ToString());
        //ConsoleProDebug.Watch("Player 2 Block Times", BlockTimes[2].ToString());
        //ConsoleProDebug.Watch("Player 3 Block Times", BlockTimes[3].ToString());
        //ConsoleProDebug.Watch("Player 4 Block Times", BlockTimes[4].ToString());
        //ConsoleProDebug.Watch("Player 5 Block Times", BlockTimes[5].ToString());
        //ConsoleProDebug.Watch("Player 0 Carry Food Times", FoodScoreTimes[0].ToString());
        //ConsoleProDebug.Watch("Player 1 Carry Food Times", FoodScoreTimes[1].ToString());
        //ConsoleProDebug.Watch("Player 2 Carry Food Times", FoodScoreTimes[2].ToString());
        //ConsoleProDebug.Watch("Player 3 Carry Food Times", FoodScoreTimes[3].ToString());
        //ConsoleProDebug.Watch("Player 4 Carry Food Times", FoodScoreTimes[4].ToString());
        //ConsoleProDebug.Watch("Player 5 Carry Food Times", FoodScoreTimes[5].ToString());
        //ConsoleProDebug.Watch("Player 0 Water Gun Spray Time", WaterGunUseTime[0].ToString());
        //ConsoleProDebug.Watch("Player 1 Water Gun Spray Time", WaterGunUseTime[1].ToString());
        //ConsoleProDebug.Watch("Player 2 Water Gun Spray Time", WaterGunUseTime[2].ToString());
        //ConsoleProDebug.Watch("Player 3 Water Gun Spray Time", WaterGunUseTime[3].ToString());
        //ConsoleProDebug.Watch("Player 4 Water Gun Spray Time", WaterGunUseTime[4].ToString());
        //ConsoleProDebug.Watch("Player 5 Water Gun Spray Time", WaterGunUseTime[5].ToString());
        //ConsoleProDebug.Watch("Player 0 Hooked Time", HookGunUseTimes[0].ToString());
        //ConsoleProDebug.Watch("Player 1 Hooked Time", HookGunUseTimes[1].ToString());
        //ConsoleProDebug.Watch("Player 2 Hooked Time", HookGunUseTimes[2].ToString());
        //ConsoleProDebug.Watch("Player 3 Hooked Time", HookGunUseTimes[3].ToString());
        //ConsoleProDebug.Watch("Player 4 Hooked Time", HookGunUseTimes[4].ToString());
        //ConsoleProDebug.Watch("Player 5 Hooked Time", HookGunUseTimes[5].ToString());
        //ConsoleProDebug.Watch("Player 0 Hooked Success Time", HookGunSuccessTimes[0].ToString());
        //ConsoleProDebug.Watch("Player 1 Hooked Success Time", HookGunSuccessTimes[1].ToString());
        //ConsoleProDebug.Watch("Player 2 Hooked Success Time", HookGunSuccessTimes[2].ToString());
        //ConsoleProDebug.Watch("Player 3 Hooked Success Time", HookGunSuccessTimes[3].ToString());
        //ConsoleProDebug.Watch("Player 4 Hooked Success Time", HookGunSuccessTimes[4].ToString());
        //ConsoleProDebug.Watch("Player 5 Hooked Success Time", HookGunSuccessTimes[5].ToString());
        //ConsoleProDebug.Watch("Player 0 Suck Time", SuckedPlayersTimes[0].ToString());
        //ConsoleProDebug.Watch("Player 1 Suck Time", SuckedPlayersTimes[1].ToString());
        //ConsoleProDebug.Watch("Player 2 Suck Time", SuckedPlayersTimes[2].ToString());
        //ConsoleProDebug.Watch("Player 3 Suck Time", SuckedPlayersTimes[3].ToString());
        //ConsoleProDebug.Watch("Player 4 Suck Time", SuckedPlayersTimes[4].ToString());
        //ConsoleProDebug.Watch("Player 5 Suck Time", SuckedPlayersTimes[5].ToString());
        //// Debug End
        CheckRestart();
        SetWeaponSpawn();
    }

    // This method set the weaponspawn area to follow the center of the player
    // Also, clamp the weeaponspawn area to not let it exceed the boundaries of the world
    void SetWeaponSpawn()
    {
        weaponSpawnerCenter.x = Camera.main.GetComponent<CameraController>().FollowTarget.x;
        weaponSpawnerCenter.z = Camera.main.GetComponent<CameraController>().FollowTarget.z;

        // Trying to clamp the weapon Spawn Area within the world space
        float xmin = WorldCenter.x - WorldSize.x / 2 + weaponSpawnerSize.x / 2;
        float xmax = WorldCenter.x + WorldSize.x / 2 - weaponSpawnerSize.x / 2;
        float zmin = WorldCenter.z - WorldSize.z / 2 + weaponSpawnerSize.z / 2;
        float zmax = WorldCenter.z + WorldSize.z / 2 - weaponSpawnerSize.z / 2;

        weaponSpawnerCenter.x = Mathf.Clamp(weaponSpawnerCenter.x, xmin, xmax);
        weaponSpawnerCenter.z = Mathf.Clamp(weaponSpawnerCenter.z, zmin, zmax);
    }

    void CheckRestart()
    {
        if (_player.GetButtonDown("Restart"))
        {
            SceneManager.LoadScene("Suyi");
        }

    }
}

public sealed class PlayerInformation
{
    public string PlayerName;
    public Color PlayerColor;

    public PlayerInformation()
    {
        PlayerName = "";
        PlayerColor = Color.white;
    }

    public PlayerInformation(string name, Color color)
    {
        PlayerName = name;
        PlayerColor = color;
    }
}
