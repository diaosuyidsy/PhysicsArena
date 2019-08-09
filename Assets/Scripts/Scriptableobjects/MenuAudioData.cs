using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuAudioData", menuName = "ScriptableObjects/MenuAudioData", order = 1)]
public class MenuAudioData : ScriptableObject
{
    public AudioClip BrowseAudioClip;
    public AudioClip SelectionAudioClip;
    public AudioClip SecondMenuModeDropDownAudioClip;
    public AudioClip SecondMenuModeDropUpAudioClip;
    public AudioClip SecondMenuMapBrowseAudioClip;
    public AudioClip BackAudioClip;
    public AudioClip ThirdMenuSlotInAudioClip;
    public AudioClip ThirdMenuSlotOutAudioClip;
    public AudioClip ThirdMenuJoinGameAudioClip;
    public AudioClip ThirdMenuEggJiggleAudioClip;
    public AudioClip ThirdMenuUnJoinGameAudioClip;
    public AudioClip ThirdMenuEggSelectedAudioClip;
    public AudioClip ThirdMenuEggUpAudioClip;
    public AudioClip ThirdMenuChickenLandAudioClip;
    public AudioClip ThirdMenuGroundMoveBackAudioClip;
    public AudioClip ThirdMenuEggDisappearAudioClip;
    public AudioClip ThirdMenuChickenToEggAudioClip;
    public AudioClip ThirdMenuChickenToAbyssAudioClip;

}
