using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGenerator : MonoBehaviour
{
    public float GenerationRadius;
    public float GenerationIntervalMin;
    public float GenerationIntervalMax;

    public int MaxWeaponNumber;

    public List<GameObject> WeaponPrefabs;
    public List<float> ProbabilitySum;

    private float Timer;

    private bool gameStart;
    

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddHandler<GameStart>(OnGameStart);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<GameStart>(OnGameStart);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart)
        {
            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                Timer = Random.Range(GenerationIntervalMin, GenerationIntervalMax);
                GenerateWeapon();
            }
        }
    }

    private void OnGameStart(GameStart e)
    {
        gameStart = true;
    }

    private void GenerateWeapon()
    {
        float Ran = Random.Range(0.0f, 1.0f);

        int index = 0;

        while (Ran > ProbabilitySum[index]&&index<ProbabilitySum.Count)
        {
            index++;
        }

        GameObject Weapon = Instantiate(WeaponPrefabs[index]);
        if (Weapon.name.Contains("Water"))
        {
            Camera.main.GetComponent<Obi.ObiBaseFluidRenderer>().particleRenderers.Add(Weapon.GetComponent<rtEmit>().ParticleRenderer);
        }

        Vector2 XZOffset = Random.insideUnitCircle * GenerationRadius;

        Weapon.transform.position = transform.position + new Vector3(XZOffset.x, 6.5f, XZOffset.y);

        Weapon.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Weapon.GetComponent<WeaponBase>().OnSpawn();

    }
}
