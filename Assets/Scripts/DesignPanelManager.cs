using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DesignPanelManager : MonoBehaviour
{
    public static DesignPanelManager DPM;

    public Text HookSpeedText;
    public Slider HookSpeedSlider;
    public GameObject HookGunPrefab;
    public Toggle MeleeChargeToggle;
    public Toggle MeleeDoubleArmToggle;
    public Toggle MeleeAlternateSchemaToggle;

    [HideInInspector]
    public float HookSpeed;

    private void Awake()
    {
        DPM = this;
        Setup();
    }

    private void Setup()
    {
        rtHook rth = HookGunPrefab.GetComponent<rtHook>();
        HookSpeed = rth.HookSpeed;
        HookSpeedText.text = HookSpeed.ToString();
        HookSpeedSlider.value = 0.5f;
        rth.HookSpeed = 10f;
    }

}
