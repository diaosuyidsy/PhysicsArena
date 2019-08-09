using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameFeelData", menuName = "ScriptableObjects/GameFeelData", order = 1)]
public class GameFeelData : ScriptableObject
{
    public ViberationInformation PlayerRespawnViberationInformation;
    public ViberationInformation ObjectPickedUpViberationInformation;
    public ViberationInformation HookBlockerViberationInformation;
    public ViberationInformation HookBlockedViberationInformation;
    public ViberationInformation FistGunFireViberationInformation;
    public ViberationInformation FistGunHitterViberationInformation;
    public ViberationInformation FistGunHittedViberationInformation;
    public ViberationInformation FistGunBlockerViberationInformation;
    public ViberationInformation FistGunBlockedViberationInformation;
    public ViberationInformation FistGunChargedViberationInformation;
    public ViberationInformation BazookaFiredViberationInformation;
    public ViberationInformation BazookaBomberViberationInformation;
    public ViberationInformation BazookaBombedViberationInformation;
    public ViberationInformation FoodDeliveredViberationInformation;
}
