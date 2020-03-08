using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public Animator Animator;
    public Renderer[] ShieldRenderers;
    public GameObject Shield;

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
    }
}
