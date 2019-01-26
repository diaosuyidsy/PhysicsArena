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
        // First we need to shuffle the array
        System.Random rng = new System.Random();
        for (int i = Weapons.Length - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);

            GameObject temp = Weapons[i];
            Weapons[i] = Weapons[j];
            Weapons[j] = temp;
        }
        // Then search through it
        for (int i = 0; i < Weapons.Length; i++)
        {
            GameObject weapon = Weapons[i];
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
