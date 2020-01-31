using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTrap : MonoBehaviour
{
    public GameObject FillPlatform;
    public GameObject FillPlatformColliders;
    public GameObject Fence;
    public GameObject FenceColliders;

    public GameObject WeaponGenerator;

    public GameObject ConnectedTrap;

    public float SwtichTime;

    public float FenceFilledScaleY;
    public float FenceUnfilledScaleY;
    public float FillPlatformFilledScaleY;
    public float FillPlatformUnfilledScaleY;

    public float FenceCollidersFilledScaleY;
    public float FenceCollidersUnfilledScaleY;
    public float FillPlatformCollidersFilledScaleY;
    public float FillPlatformCollidersUnfilledScaleY;

    private bool filled;


    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if (e.ImpactObject == gameObject)
        {
            StartCoroutine(Swtich(true));
            ConnectedTrap.GetComponent<NormalTrap>().StartCoroutine(ConnectedTrap.GetComponent<NormalTrap>().Swtich(false));
        }
        
    }

    public void SetUp(bool fill)
    {

        Vector3 FillPlatformScale = FillPlatform.transform.localScale;
        Vector3 FillPlatformCollidersScale = FillPlatformColliders.transform.localScale;
        Vector3 FenceScale = Fence.transform.localScale;
        Vector3 FenceCollidersScale = FenceColliders.transform.localScale;

        if (fill)
        {
            FillPlatform.GetComponent<MeshRenderer>().enabled = true;
            Fence.GetComponent<MeshRenderer>().enabled = false;

            FenceColliders.SetActive(false);
            FillPlatformColliders.SetActive(true);

            FillPlatform.transform.localScale = new Vector3(FillPlatformScale.x, FillPlatformFilledScaleY, FillPlatformScale.z);
            FillPlatformColliders.transform.localScale = new Vector3(FillPlatformCollidersScale.x, FillPlatformCollidersFilledScaleY, FillPlatformCollidersScale.z);
            Fence.transform.localScale = new Vector3(FenceScale.x, FenceFilledScaleY, FenceScale.z);
            FenceColliders.transform.localScale = new Vector3(FenceCollidersScale.x, FenceCollidersFilledScaleY, FenceCollidersScale.z);

        }
        else
        {
            FillPlatform.GetComponent<MeshRenderer>().enabled = false;
            Fence.GetComponent<MeshRenderer>().enabled = true;

            FenceColliders.SetActive(true);
            FillPlatformColliders.SetActive(false);

            FillPlatform.transform.localScale = new Vector3(FillPlatformScale.x, FillPlatformUnfilledScaleY, FillPlatformScale.z);
            FillPlatformColliders.transform.localScale = new Vector3(FillPlatformCollidersScale.x, FillPlatformCollidersUnfilledScaleY, FillPlatformCollidersScale.z);
            Fence.transform.localScale = new Vector3(FenceScale.x, FenceUnfilledScaleY, FenceScale.z);
            FenceColliders.transform.localScale = new Vector3(FenceCollidersScale.x, FenceCollidersUnfilledScaleY, FenceCollidersScale.z);
        }
    }

    public IEnumerator Swtich(bool fill)
    {
        GetComponent<MeshCollider>().enabled = false;

        Vector3 FillPlatformScale = FillPlatform.transform.localScale;
        Vector3 FillPlatformCollidersScale = FillPlatformColliders.transform.localScale;
        Vector3 FenceScale = Fence.transform.localScale;
        Vector3 FenceCollidersScale = FenceColliders.transform.localScale;

        if (fill)
        {
            FillPlatform.GetComponent<MeshRenderer>().enabled = true;
            FillPlatform.transform.localScale = new Vector3(FillPlatformScale.x, FillPlatformUnfilledScaleY, FillPlatformScale.z);

            FillPlatformColliders.SetActive(true);
            FillPlatformColliders.transform.localScale = new Vector3(FillPlatformCollidersScale.x, FillPlatformCollidersUnfilledScaleY, FillPlatformCollidersScale.z);

            Fence.transform.localScale = new Vector3(FenceScale.x, FenceUnfilledScaleY, FenceScale.z);

            FenceColliders.transform.localScale = new Vector3(FenceCollidersScale.x, FenceCollidersUnfilledScaleY, FenceCollidersScale.z);
        }
        else
        {
            Fence.GetComponent<MeshRenderer>().enabled = true;

            FillPlatform.transform.localScale = new Vector3(FillPlatformScale.x, FillPlatformFilledScaleY, FillPlatformScale.z);
            FillPlatformColliders.transform.localScale = new Vector3(FillPlatformCollidersScale.x, FillPlatformCollidersFilledScaleY, FillPlatformCollidersScale.z);

            Fence.transform.localScale = new Vector3(FenceScale.x, FenceFilledScaleY, FenceScale.z);
            FenceColliders.transform.localScale = new Vector3(FenceCollidersScale.x, FenceCollidersFilledScaleY, FenceCollidersScale.z);
            FenceColliders.SetActive(true);

            WeaponGenerator.GetComponent<WeaponGenerator>().enabled = false;
            EventManager.Instance.TriggerEvent(new WeaponGeneratorSwtich(WeaponGenerator, false));

        }

        float Timer = 0;

        while(Timer < SwtichTime)
        {
            Timer += Time.deltaTime;

            if (fill)
            {
                FillPlatform.transform.localScale = new Vector3(FillPlatformScale.x, Mathf.Lerp(FillPlatformUnfilledScaleY, FillPlatformFilledScaleY, Timer / SwtichTime), FillPlatformScale.z);
                FillPlatformColliders.transform.localScale = new Vector3(FillPlatformCollidersScale.x, Mathf.Lerp(FillPlatformCollidersUnfilledScaleY, FillPlatformCollidersFilledScaleY, Timer / SwtichTime), FillPlatformCollidersScale.z);

                Fence.transform.localScale = new Vector3(FenceScale.x, Mathf.Lerp(FenceUnfilledScaleY, FenceFilledScaleY, Timer / SwtichTime), FenceScale.z);
                FenceColliders.transform.localScale = new Vector3(FenceCollidersScale.x, Mathf.Lerp(FenceCollidersUnfilledScaleY, FenceCollidersFilledScaleY, Timer / SwtichTime), FenceCollidersScale.z);

            }
            else
            {
                FillPlatform.transform.localScale = new Vector3(FillPlatformScale.x, Mathf.Lerp(FillPlatformFilledScaleY, FillPlatformUnfilledScaleY, Timer / SwtichTime), FillPlatformScale.z);
                FillPlatformColliders.transform.localScale = new Vector3(FillPlatformCollidersScale.x, Mathf.Lerp(FillPlatformCollidersFilledScaleY, FillPlatformCollidersUnfilledScaleY, Timer / SwtichTime), FillPlatformCollidersScale.z);

                Fence.transform.localScale = new Vector3(FenceScale.x, Mathf.Lerp(FenceFilledScaleY, FenceUnfilledScaleY, Timer / SwtichTime), FenceScale.z);
                FenceColliders.transform.localScale = new Vector3(FenceCollidersScale.x, Mathf.Lerp(FenceCollidersFilledScaleY, FenceCollidersUnfilledScaleY, Timer / SwtichTime), FenceCollidersScale.z);
            }

            yield return null;
        }

        if (fill)
        {
            Fence.GetComponent<MeshRenderer>().enabled = false;
            FenceColliders.SetActive(false);

            WeaponGenerator.GetComponent<WeaponGenerator>().enabled = true;
            EventManager.Instance.TriggerEvent(new WeaponGeneratorSwtich(WeaponGenerator, true));
        }
        else
        {
            FillPlatformColliders.SetActive(false);
        }

        filled = fill;
    }
}
