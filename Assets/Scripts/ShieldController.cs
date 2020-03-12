using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public Animator Animator;
    public Renderer[] ShieldRenderers;
    public GameObject Shield;
    public GameObject ShieldCrack;

    private GameObject _instantiatedShieldCrack;

    public void SetShield(bool open)
    {
        Animator.SetBool("ShieldOpen", open);
        Shield.SetActive(open);
    }

    public void SetEnergy(float Energy)
    {
        foreach (Renderer r in ShieldRenderers)
        {
            r.material.SetFloat("_Energy", Energy);
        }
        if (Energy == 0f)
        {
            Shield.SetActive(false);
            _instantiatedShieldCrack = Instantiate(ShieldCrack, transform.parent, true);
            _instantiatedShieldCrack.SetActive(true);
            _instantiatedShieldCrack.transform.parent = null;
            StartCoroutine(increaseShieldCraftDistance(0.5f));
        }
    }

    IEnumerator increaseShieldCraftDistance(float time)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            _instantiatedShieldCrack.GetComponent<Renderer>().material.SetFloat("_Distance", elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(_instantiatedShieldCrack);
        _instantiatedShieldCrack = null;
    }
}
