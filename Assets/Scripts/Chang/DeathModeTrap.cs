using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathModeTrap : MonoBehaviour
{
    public GameObject FillPlatform;
    public GameObject FillPlatformColliders;
    public GameObject Fence;
    public GameObject FenceColliders;

    public GameObject WeaponGenerator;

    public float SwtichTime;

    public float FenceStartScaleY;
    public float FenceEndScaleY;
    public float FillPlatformStartScaleY;
    public float FillPlatformEndScaleY;

    public float FenceCollidersStartScaleY;
    public float FenceCollidersEndScaleY;
    public float FillPlatformCollidersStartScaleY;
    public float FillPlatformCollidersEndScaleY;


    private bool Swtiched;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddHandler<PlayerDiedInDeathMode>(OnPlayerDied);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<PlayerDiedInDeathMode>(OnPlayerDied);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPlayerDied(PlayerDiedInDeathMode e)
    {
        if (e.TrapZone == gameObject&&!Swtiched)
        {
            StartCoroutine(Swtiching());
        }
    }

    private IEnumerator Swtiching()
    {
        Swtiched = true;

        GetComponent<MeshCollider>().enabled = false;

        Vector3 FillPlatformScale = FillPlatform.transform.localScale;
        Vector3 FillPlatformCollidersScale = FillPlatformColliders.transform.localScale;
        Vector3 FenceScale = Fence.transform.localScale;
        Vector3 FenceCollidersScale = FenceColliders.transform.localScale;

        FillPlatform.GetComponent<MeshRenderer>().enabled = true;
        FillPlatform.transform.localScale = new Vector3(FillPlatformScale.x, FillPlatformStartScaleY, FillPlatformScale.z);

        FillPlatformColliders.SetActive(true);
        FillPlatformColliders.transform.localScale = new Vector3(FillPlatformCollidersScale.x, FillPlatformCollidersStartScaleY, FillPlatformCollidersScale.z);


        Fence.transform.localScale = new Vector3(FenceScale.x, FenceStartScaleY, FenceScale.z);

        FenceColliders.transform.localScale = new Vector3(FenceCollidersScale.x, FenceCollidersStartScaleY, FenceCollidersScale.z);



        float Timer = 0;

        while(Timer < SwtichTime)
        {
            Timer += Time.deltaTime;

            FillPlatform.transform.localScale = new Vector3(FillPlatformScale.x, Mathf.Lerp(FillPlatformStartScaleY, FillPlatformEndScaleY, Timer / SwtichTime), FillPlatformScale.z);
            FillPlatformColliders.transform.localScale = new Vector3(FillPlatformCollidersScale.x, Mathf.Lerp(FillPlatformCollidersStartScaleY, FillPlatformCollidersEndScaleY, Timer / SwtichTime), FillPlatformCollidersScale.z);

            Fence.transform.localScale = new Vector3(FenceScale.x, Mathf.Lerp(FenceStartScaleY, FenceEndScaleY, Timer / SwtichTime), FenceScale.z);
            FenceColliders.transform.localScale = new Vector3(FenceCollidersScale.x, Mathf.Lerp(FenceCollidersStartScaleY, FenceCollidersEndScaleY, Timer / SwtichTime), FenceCollidersScale.z);

            yield return null;
        }


        Fence.GetComponent<MeshRenderer>().enabled = false;
        FenceColliders.SetActive(false);

        WeaponGenerator.GetComponent<WeaponGenerator>().enabled = true;
        EventManager.Instance.TriggerEvent(new WeaponGeneratorActivated(WeaponGenerator));
    }
}
