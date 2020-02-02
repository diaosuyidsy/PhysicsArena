﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApocalypseArenaWeaponManager : MonoBehaviour
{
    public GameObject LeftTrap;
    public GameObject RightTrap;

    public List<GameObject> WeaponGeneratorList;

    public List<GameObject> AvailableWeaponList;
    public List<int> WeaponQuantityList;

    private int CurrentMaxWeaponNumber;

    private int CurrentWeaponCount;

    private bool gameStart;

    private int BagIndex;
    private List<GameObject> WeaponPrefabBag;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {


        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<WeaponGeneratorSwtich>(OnGeneratorSwtich);
        EventManager.Instance.AddHandler<WeaponHitDeathTrigger>(OnWeaponFall);
        EventManager.Instance.AddHandler<WeaponUsedUp>(OnWeaponUsedUp);

        CurrentMaxWeaponNumber = WeaponGeneratorList.Count;
        InitPrefabBag();
        ShuffleBag();

        TrapSetUp();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<GameStart>(OnGameStart);
        EventManager.Instance.RemoveHandler<WeaponGeneratorSwtich>(OnGeneratorSwtich);
        EventManager.Instance.RemoveHandler<WeaponHitDeathTrigger>(OnWeaponFall);
        EventManager.Instance.RemoveHandler<WeaponUsedUp>(OnWeaponUsedUp);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart && CurrentWeaponCount < CurrentMaxWeaponNumber)
        {

            int Valid = CurrentMaxWeaponNumber - CurrentWeaponCount;

            List<GameObject> AvailableGenerators = new List<GameObject>();

            for(int i = 0; i < WeaponGeneratorList.Count; i++)
            {
                if (!WeaponGeneratorList[i].GetComponent<WeaponGenerator>().WeaponOnPlatform())
                {
                    AvailableGenerators.Add(WeaponGeneratorList[i]);
                }
            }

            while (Valid > 0)
            {


                int Ran = Random.Range(0, AvailableGenerators.Count);

                if(BagIndex >= WeaponPrefabBag.Count)
                {
                    ShuffleBag();
                }

                AvailableGenerators[Ran].GetComponent<WeaponGenerator>().GenerateWeapon(WeaponPrefabBag[BagIndex]);

                AvailableGenerators.Remove(AvailableGenerators[Ran]);

                BagIndex++;
                CurrentWeaponCount++;
                Valid--;
            }

        }
    }

    private void TrapSetUp()
    {
        float r = Random.Range(0.0f, 1.0f);

        if (r < 0.5f)
        {
            LeftTrap.GetComponent<NormalTrap>().SetUp(true);
            RightTrap.GetComponent<NormalTrap>().SetUp(false);
        }
        else
        {
            LeftTrap.GetComponent<NormalTrap>().SetUp(false);
            RightTrap.GetComponent<NormalTrap>().SetUp(true);
        }
    }

    private void InitPrefabBag()
    {
        BagIndex = 0;

        WeaponPrefabBag = new List<GameObject>();

        for(int i = 0; i < WeaponQuantityList.Count; i++)
        {
            for(int j = 0; j < WeaponQuantityList[i]; j++)
            {
                WeaponPrefabBag.Add(AvailableWeaponList[i]);
            }
        }
    }

    private void ShuffleBag()
    {
        BagIndex = 0;

        List<GameObject> Temp = new List<GameObject>();

        while (WeaponPrefabBag.Count > 0)
        {
            int index = Random.Range(0, WeaponPrefabBag.Count);

            Temp.Add(WeaponPrefabBag[index]);
            WeaponPrefabBag.RemoveAt(index);
        }

        for(int i = 0; i < Temp.Count; i++)
        {
            WeaponPrefabBag.Add(Temp[i]);
        }

    }

    private void OnWeaponFall(WeaponHitDeathTrigger e)
    {
        CurrentWeaponCount--;
    }

    private void OnWeaponUsedUp(WeaponUsedUp e)
    {
        CurrentWeaponCount--;
    }

    private void OnGeneratorSwtich(WeaponGeneratorSwtich e)
    {

        if (e.Activated)
        {
            if (!WeaponGeneratorList.Contains(e.Generator))
            {
                CurrentMaxWeaponNumber++;
                WeaponGeneratorList.Add(e.Generator);
            }

        }
        else
        {
            if (WeaponGeneratorList.Contains(e.Generator))
            {
                CurrentMaxWeaponNumber--;
                WeaponGeneratorList.Remove(e.Generator);
            }
        }
    }

    private void OnGameStart(GameStart e)
    {
        gameStart = true;
    }
}