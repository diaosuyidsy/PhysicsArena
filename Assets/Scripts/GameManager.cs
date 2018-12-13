using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    public Text Result;
    public Image EndImage;
    [HideInInspector]
    public GameObject[] Players;
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
    [Tooltip ("This is CD for on drop respawn method")]
    public float WeaponRespawnTime = 2f;
    public float DropWeaponVelocityThreshold = 2f;
    public string[] PlayerCanPickupTags;

    private int Team1RespawnIndex = 0;
    private int Team2RespawnIndex = 0;

    private float EndImageScale = 208f;

    //teleport a weapon to a random position within a given space
    public Vector3 weaponSpawnerCenter;
    public Vector3 weaponSpawnerSize;

    // Need to manually set up world center until we figure out a way to automatically do it.
    [Header ("World Limit Setting")]
    public Vector3 WorldCenter;
    public Vector3 WorldSize;

    public int weaponTracker = 2;

    private bool _won = false;

    private void Awake ()
    {
        GM = this;
        GameObject[] team1Players = GameObject.FindGameObjectsWithTag ("Team1");
        GameObject[] team2Players = GameObject.FindGameObjectsWithTag ("Team2");
        Players = new GameObject[team1Players.Length + team2Players.Length];
        int index = 0;
        foreach (GameObject go in team1Players)
        {
            Players[index] = go;
            index++;
        }
        foreach (GameObject go in team2Players)
        {
            Players[index] = go;
            index++;
        }
    }

    private void Start ()
    {
        // Respawn Point Holder Setup
        Transform team1 = RespawnPointHolder.GetChild (0);
        Transform team2 = RespawnPointHolder.GetChild (1);
        Team1RespawnPts = new GameObject[team1.childCount];
        Team2RespawnPts = new GameObject[team2.childCount];
        for (int i = 0; i < team1.childCount; i++)
        {
            Team1RespawnPts[i] = team1.GetChild (i).gameObject;
        }
        for (int i = 0; i < team2.childCount; i++)
        {
            Team2RespawnPts[i] = team2.GetChild (i).gameObject;
        }
    }

    IEnumerator EndImageShow (float time, GameObject _tar = null)
    {
        float elapsedTime = 0f;
        EndImage.gameObject.SetActive (true);
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            if (_tar != null)
            {
                EndImage.rectTransform.position = Camera.main.WorldToScreenPoint (_tar.transform.position);
                Vector3 targetPosition = new Vector3 (EndImage.rectTransform.position.x, EndImage.rectTransform.position.y, 0f);
                EndImage.rectTransform.position = Vector3.Lerp (EndImage.rectTransform.position, targetPosition, 0.5f);
            }
            yield return new WaitForEndOfFrame ();
            EndImage.rectTransform.sizeDelta -= new Vector2 (EndImageScale, EndImageScale);
        }
    }
    //make the space for weapon to respawn (weapon-spawner) visible in scene
    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = new Color (1, 0, 0, 0.5f);
        Gizmos.DrawCube (weaponSpawnerCenter, weaponSpawnerSize);
        Gizmos.color = new Color (0, 0, 0, 0.5f);
        Gizmos.DrawCube (WorldCenter, WorldSize);
    }

    //move weapon to weapon-spawner
    public void HideWeapon (GameObject weapon)
    {
        MoveWeaponToSpawnArea (weapon);
        weaponTracker--;
        print ("current gun = " + weaponTracker);
        weapon.SetActive (false);
        StartCoroutine (ActivateWeapon (WeaponRespawnTime, weapon));
    }

    public void MoveWeaponToSpawnArea (GameObject weapon)
    {
        Vector3 targetPos = weaponSpawnerCenter + new Vector3 (Random.Range (-weaponSpawnerSize.x / 2, weaponSpawnerSize.x / 2), Random.Range (-weaponSpawnerSize.y / 2, weaponSpawnerSize.y / 2), Random.Range (-weaponSpawnerSize.z / 2, weaponSpawnerSize.z / 2));
        weapon.transform.position = targetPos;
    }

    //activate weapon after WeaponRespawnTime
    IEnumerator ActivateWeapon (float time, GameObject go)
    {
        yield return new WaitForSeconds (time);
        go.SetActive (true);
        weaponTracker++;
    }

    public void SetToRespawn (GameObject player, float yOffset)
    {
        if (player.CompareTag ("Team1"))
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

    public void GameOver (int winner, GameObject _tar)
    {
        if (_won)
            return;
        //Result.text = "TEAM " + (winner == 1 ? "ONE" : "TWO") + " VICTORY";
        //Result.transform.parent.gameObject.SetActive (true);
        StartCoroutine (EndImageShow (2f, _tar));
        _won = true;
    }

    private void Update ()
    {
        CheckRestart ();
        SetWeaponSpawn ();
    }

    // This method set the weaponspawn area to follow the center of the player
    // Also, clamp the weeaponspawn area to not let it exceed the boundaries of the world
    void SetWeaponSpawn ()
    {
        weaponSpawnerCenter.x = Camera.main.GetComponent<CameraController> ().FollowTarget.x;
        weaponSpawnerCenter.z = Camera.main.GetComponent<CameraController> ().FollowTarget.z;

        // Trying to clamp the weapon Spawn Area within the world space
        float xmin = WorldCenter.x - WorldSize.x / 2 + weaponSpawnerSize.x / 2;
        float xmax = WorldCenter.x + WorldSize.x / 2 - weaponSpawnerSize.x / 2;
        float zmin = WorldCenter.z - WorldSize.z / 2 + weaponSpawnerSize.z / 2;
        float zmax = WorldCenter.z + WorldSize.z / 2 - weaponSpawnerSize.z / 2;

        weaponSpawnerCenter.x = Mathf.Clamp (weaponSpawnerCenter.x, xmin, xmax);
        weaponSpawnerCenter.z = Mathf.Clamp (weaponSpawnerCenter.z, zmin, zmax);
    }

    void CheckRestart ()
    {

#if UNITY_EDITOR_OSX
        if (Input.GetKeyDown(KeyCode.Joystick1Button10))
        {
            SceneManager.LoadScene("MasterScene");
        }

#endif
#if UNITY_EDITOR_WIN
        if (Input.GetKeyDown (KeyCode.Joystick1Button6))
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
        }
#endif

    }
}
