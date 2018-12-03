using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager GM;
    public Text Result;
    public GameObject[] Players;
    public GameObject RunSandVFX;
    public LayerMask AllPlayers;
    public GameObject[] Team1RespawnPts;
    public GameObject[] Team2RespawnPts;
    public float RespawnTime = 5f;
    public float WeaponRespawnTime = 2f;

    private int Team1RespawnIndex = 0;
    private int Team2RespawnIndex = 0;
    
   //teleport a weapon to a random position within a given space
    public Vector3 weaponSpawnerCenter;
    public Vector3 weaponSpawnerSize;
    private Vector3 targetPos;
    public int weaponTracker = 2;

    private void Awake ()
    {
        GM = this;
    }

    //make the space for weapon to respawn (weapon-spawner) visible in scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1,0,0,0.5f);
        Gizmos.DrawCube(weaponSpawnerCenter,weaponSpawnerSize);
    }

    //move weapon to weapon-spawner
    public void HideWeapon(GameObject weapon)
    {
        targetPos = weaponSpawnerCenter + new Vector3(Random.Range(-weaponSpawnerSize.x / 2, weaponSpawnerSize.x / 2), Random.Range(-weaponSpawnerSize.y / 2, weaponSpawnerSize.y / 2), Random.Range(-weaponSpawnerSize.z / 2, weaponSpawnerSize.z / 2));
        weapon.transform.position = targetPos;
        weaponTracker--;
        print("current gun = " + weaponTracker);
        weapon.SetActive(false);
        StartCoroutine(ActivateWeapon(WeaponRespawnTime, weapon)); 
    }
    
    //activate weapon after WeaponRespawnTime
    IEnumerator ActivateWeapon (float time, GameObject go)
    {
        yield return new WaitForSeconds (time);
        go.SetActive (true);
        weaponTracker++;
    }
    
    public void SetToRespawn (GameObject player)
    {
        if (player.CompareTag ("Team1"))
        {
            Team1RespawnIndex = (Team1RespawnIndex + 1) % Team1RespawnPts.Length;
            player.transform.position = Team1RespawnPts[Team1RespawnIndex].transform.position;
        }
        else
        {
            Team2RespawnIndex = (Team2RespawnIndex + 1) % Team2RespawnPts.Length;
            player.transform.position = Team2RespawnPts[Team2RespawnIndex].transform.position;
        }
    }

    public void GameOver (int winner)
    {
        Result.text = "TEAM " + (winner == 1 ? "ONE" : "TWO") + " VICTORY";
        Result.transform.parent.gameObject.SetActive (true);
    }

    private void Update ()
    {
        CheckRestart ();
        weaponSpawnerCenter = Camera.main.GetComponent<CameraController>().FollowTarget;
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
            SceneManager.LoadScene ("MasterScene");
        }
#endif

    }
}
