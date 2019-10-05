﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PunNetworkManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public bool AutoConnect = true;

    /// <summary>Used as PhotonNetwork.GameVersion.</summary>
    public byte Version = 1;
    public Transform[] SpawnPosition;

    public GameObject[] PlayerPrefabs; // set in inspectors

    public void Start()
    {
        if (this.AutoConnect)
        {
            this.ConnectNow();
        }

    }

    public void ConnectNow()
    {
        Debug.Log("ConnectAndJoinRandom.ConnectNow() will now call: PhotonNetwork.ConnectUsingSettings().");
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = this.Version + "." + SceneManagerHelper.ActiveSceneBuildIndex;
        PhotonNetwork.AuthValues = new Photon.Realtime.AuthenticationValues("Player" + Random.Range(0, 100));
        PhotonNetwork.LocalPlayer.NickName = "Player" + Random.Range(0, 1000000);
    }


    // below, we implement some callbacks of the Photon Realtime API.
    // Being a MonoBehaviourPunCallbacks means, we can override the few methods which are needed here.


    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected. This script now calls: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 6, CleanupCacheOnLeave = true }, null);
    }

    // the following methods are implemented to give you some context. re-implement them as needed.
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnDisconnected(" + cause + ")");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running.");
        if (this.PlayerPrefabs != null)
        {
            Vector3 spawnPos = Vector3.up;
            int numplayerinroom = PhotonNetwork.CountOfPlayersInRooms;
            if (SpawnPosition != null)
                spawnPos = this.SpawnPosition[numplayerinroom % SpawnPosition.Length].position;
            GameObject go = PhotonNetwork.Instantiate(PlayerPrefabs[numplayerinroom % PlayerPrefabs.Length].name, spawnPos, Quaternion.identity, 0);
            EventManager.Instance.TriggerEvent(new GameStart());
        }
    }
}