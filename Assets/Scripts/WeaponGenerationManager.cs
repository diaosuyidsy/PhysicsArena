using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class WeaponGenerationManager : MonoBehaviour
{
    public static WeaponGenerationManager WGM;

    public GameObject[] Weapons;
    public float WeaponSpawnCD = 10f;
    public int NextSpawnAmount;

    private int _activatedWeaponNum = 0;
    private int _nextspawnamount;

    private void Awake()
    {
        WGM = this;
    }

    private void Start()
    {
        _nextspawnamount = NextSpawnAmount;
        foreach (GameObject weapon in Weapons)
        {
            weapon.SetActive(false);
        }

        // We need to invoke weapon generation from the start
        InvokeRepeating("GenerateWeapon", 5f, WeaponSpawnCD);
    }

    private void GenerateWeapon()
    {
        // If next weapon in array is deactivated
        // Then move it to the current random spawn location
        // Then activate it
        //while (_nextspawnamount > 0)
        //{
        //    System.Random rand = new System.Random();
        //    int rannum = rand.Next(0, Weapons.Length);
        //    GameObject weapon = Weapons[rannum];
        //    if (!weapon.activeSelf)
        //    {
        //        GameManager.GM.MoveWeaponToSpawnArea(weapon);
        //        weapon.SetActive(true);
        //        _nextspawnamount--;
        //    }
        //}
        foreach (GameObject weapon in Weapons)
        {
            if (!weapon.activeSelf && _nextspawnamount > 0)
            {
                GameManager.GM.MoveWeaponToSpawnArea(weapon);
                weapon.SetActive(true);
                _nextspawnamount--;
            }
        }

        // If we want to change next spawn amount accordingly, do it here
        _nextspawnamount = NextSpawnAmount;
    }
}
