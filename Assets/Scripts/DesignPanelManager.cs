using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DesignPanelManager : MonoBehaviour
{
    public static DesignPanelManager DPM;

    public GameObject HookGunPrefab;
    public Toggle MeleeChargeToggle;
    public Toggle MeleeDoubleArmToggle;
    public Toggle MeleeAlternateSchemaToggle;
    public Slider HookGunAuxillaryAimSlider;
    public Slider BlockAngleSlider;

    private void Awake()
    {
        DPM = this;
        Setup();
    }

    private void Setup()
    {
    }

}
