using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathModeWeaponManager : MonoBehaviour
{
    public List<GameObject> WeaponGeneratorList;

    private int CurrentMaxWeaponNumber;

    private int CurrentWeaponCount;

    private bool gameStart;
    // Start is called before the first frame update
    void Start()
    {
        CurrentMaxWeaponNumber = WeaponGeneratorList.Count;

        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
        EventManager.Instance.AddHandler<WeaponGeneratorSwtich>(OnGeneratorActivaed);
        EventManager.Instance.AddHandler<WeaponHitDeathTrigger>(OnWeaponFall);
        EventManager.Instance.AddHandler<WeaponUsedUp>(OnWeaponUsedUp);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<GameStart>(OnGameStart);
        EventManager.Instance.RemoveHandler<WeaponGeneratorSwtich>(OnGeneratorActivaed);
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

                AvailableGenerators[Ran].GetComponent<WeaponGenerator>().GenerateWeapon();

                AvailableGenerators.Remove(AvailableGenerators[Ran]);

                CurrentWeaponCount++;
                Valid--;
            }

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

    private void OnGeneratorActivaed(WeaponGeneratorSwtich e)
    {
        CurrentMaxWeaponNumber++;
        WeaponGeneratorList.Add(e.Generator);
    }

    private void OnGameStart(GameStart e)
    {
        gameStart = true;
    }
}
