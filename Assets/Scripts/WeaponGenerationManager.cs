using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGenerationManager : MonoBehaviour
{
    public static WeaponGenerationManager WGM;
    public int MaxWeaponsNum;
    public GameObject[] Weapons;
    private bool[] _weaponTrackers;

    private void Awake ()
    {
        WGM = this;
    }

    private void Start ()
    {

    }
}
