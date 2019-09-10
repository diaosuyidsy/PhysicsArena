using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class NetworkEventManager : IOnEventCallback
{
    public const byte GAME_START_EVENT = 0;
    public const byte GAME_END_EVENT = 1;
    public const byte PLAYER_HIT_EVENT = 2;
    public const byte PLAYER_DIED_EVENT = 3;
    public const byte PLAYER_STUNNED_EVENT = 4;
    public const byte PLAYER_UNSTUNNED_EVENT = 5;
    public const byte PLAYER_RESPAWN_EVENT = 6;
    public const byte PLAYER_SLOWED_EVENT = 7;
    public const byte PLAYER_UNSLOWED_EVENT = 8;
    public const byte BLOCK_START_EVENT = 9;
    public const byte BLOCK_END_EVENT = 10;
    public const byte PUNCH_START_EVENT = 11;
    public const byte PUNCH_HOLDING_EVENT = 12;
    public const byte PUNCH_RELEASED_EVENT = 13;
    public const byte PUNCH_DONE_EVENT = 14;
    public const byte FOOT_STEP_EVENT = 15;
    public const byte PLAYER_JUMP_EVENT = 16;
    public const byte PLAYER_LAND_EVENT = 17;
    public const byte OBJECT_DESPAWNED_EVENT = 18;
    public const byte WEAPON_SPAWNED_EVENT = 19;
    public const byte OBJECT_PICKED_UP_EVENT = 20;
    public const byte OBJECT_DROPPED_EVENT = 21;
    public const byte OBJECT_HIT_GROUND_EVENT = 22;
    public const byte WATER_GUN_FIRED_EVENT = 23;
    public const byte HOOK_GUN_FIRED_EVENT = 24;
    public const byte HOOK_HIT_EVENT = 25;
    public const byte HOOK_BLOCKED_EVENT = 26;
    public const byte SUCK_GUN_FIRED_EVENT = 27;
    public const byte SUCK_GUN_SUCK_EVENT = 28;
    public const byte FIST_GUN_FIRED_EVENT = 29;
    public const byte FIST_GUN_HIT_EVENT = 30;
    public const byte FIST_GUN_BLOCKED_EVENT = 31;
    public const byte FIST_GUN_START_CHARGING_EVENT = 32;
    public const byte FIST_GUN_CHARGED_EVENT = 33;
    public const byte BAZOOKA_FIRED_EVENT = 34;
    public const byte BAZOOKA_BOMBED_EVENT = 35;
    public const byte FOOD_DELIVERED_EVENT = 36;
    public const byte BLOCKED_EVENT = 37;

    public NetworkEventManager()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        object[] data = (object[])photonEvent.CustomData;

        if (eventCode == GAME_START_EVENT)
        {
            EventManager.Instance.TriggerEvent(new GameStart());
        }
        else if (eventCode == GAME_END_EVENT)
        {
            // TODO
            int winner = (int)data[0];
            Transform WinnedObjective = PhotonNetwork.GetPhotonView((int)data[1]).transform;
            Vector3 WinnedPosition = (Vector3)data[2];
            GameWinType gamewintype = (GameWinType)data[3];
            // EventManager.Instance.TriggerEvent(new GameEnd(winner, winnedObjective, ))
        }
        else if (eventCode == PLAYER_HIT_EVENT)
        {
            GameObject Hiter = PhotonNetwork.GetPhotonView((int)data[0]).gameObject;
            GameObject Hitted = PhotonNetwork.GetPhotonView((int)data[1]).gameObject;
            Vector3 Force = (Vector3)data[2];
            int HiterPlayerNumber = (int)data[3];
            int HittedPlayerNumber = (int)data[4];
            float MeleeCharge = (float)data[5];
            bool IsABlock = (bool)data[6];

            EventManager.Instance.TriggerEvent(new PlayerHit(Hiter, Hitted, Force, HiterPlayerNumber, HittedPlayerNumber, MeleeCharge, IsABlock));
        }
    }

    public void Destroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
