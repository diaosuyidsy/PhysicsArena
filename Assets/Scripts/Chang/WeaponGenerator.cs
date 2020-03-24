using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGenerator : MonoBehaviour
{
    public float GenerationRadius;

    public Vector3 DetectOffset;
    public float DetectRadius;

    public LayerMask WeaponLayer;

    private const float DetectDis = 8;

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

    public GameObject GenerateWeapon(GameObject WeaponPrefab)
    {

        GameObject Weapon = Instantiate(WeaponPrefab);
        if (Weapon.name.Contains("Water"))
        {
            Camera.main.GetComponent<Obi.ObiBaseFluidRenderer>().particleRenderers.Add(Weapon.GetComponent<rtEmit>().ParticleRenderer);
        }

        Vector2 XZOffset = Random.insideUnitCircle * GenerationRadius;

        Weapon.transform.position = transform.position + new Vector3(XZOffset.x, 6.5f, XZOffset.y);

        Weapon.GetComponent<Rigidbody>().velocity = Vector3.zero;
        if (Weapon.GetComponent<WeaponBase>())
        {
            Weapon.GetComponent<WeaponBase>().OnSpawn();
        }
        if (Weapon.GetComponent<NetworkWeaponBase>())
        {
            Weapon.GetComponent<NetworkWeaponBase>().OnSpawn();
        }

        return Weapon;

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
