using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGenerator : MonoBehaviour
{
    public float GenerationRadius;
    public float GenerationIntervalMin;
    public float GenerationIntervalMax;

    public Vector3 DetectOffset;
    public float DetectRadius;

    public LayerMask WeaponLayer;


    public List<GameObject> WeaponPrefabs;
    public List<float> ProbabilitySum;

    private float Timer;

    private bool gameStart;


    private const float DetectDis = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnDestroy()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateWeapon()
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


    public bool WeaponOnPlatform()
    {
        RaycastHit[] AllHits = Physics.SphereCastAll(transform.position + DetectOffset, DetectRadius, Vector3.up, DetectDis, WeaponLayer);

        for(int i = 0; i < AllHits.Length; i++)
        {
            if(AllHits[i].collider.gameObject.CompareTag("Weapon_OnChest")|| AllHits[i].collider.gameObject.CompareTag("Weapon_OnHead"))
            {
                return true;
            }
        }

        return false;
    }

}
