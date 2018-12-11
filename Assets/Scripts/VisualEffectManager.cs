using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffectManager : MonoBehaviour
{
    public static VisualEffectManager VEM;

    [Header ("All Kinds of Visual Effects Holder")]
    public GameObject DeliverFoodVFX;
    public GameObject DeathVFX;
    public GameObject VanishVFX;
    public GameObject HitVFX;

    private void Awake ()
    {
        VEM = this;
    }

}
