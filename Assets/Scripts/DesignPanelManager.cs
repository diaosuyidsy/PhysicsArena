using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System;

public class DesignPanelManager : MonoBehaviour
{
    public static DesignPanelManager DPM;

    public GameObject HookGunPrefab;
    public Toggle MeleeChargeToggle;
    public Toggle MeleeDoubleArmToggle;
    public Toggle MeleeAlternateSchemaToggle;
    public Slider HookGunAuxillaryAimSlider;
    public Slider BlockAngleSlider;
    public Toggle HookAlternateSchemaToggle;

    private void Awake()
    {
        DPM = this;
    }
    private void OnApplicationQuit()
    {
        string path = "Assets/Resources/DesignNumber.txt";
        StreamWriter sw = new StreamWriter(path, false);
        sw.WriteLine(System.DateTime.Now);
        sw.WriteLine("Melee Charge: " + (MeleeChargeToggle.isOn ? "ON" : "OFF"));
        sw.WriteLine("Melee Double Arm Mode: " + (MeleeDoubleArmToggle.isOn ? "ON" : "OFF"));
        sw.WriteLine("Melee Alternate Schema: " + (MeleeAlternateSchemaToggle.isOn ? "ON" : "OFF"));
        sw.WriteLine("Hook Gunxillary Aim Angle: " + HookGunAuxillaryAimSlider.value);
        sw.WriteLine("Block Angle: " + BlockAngleSlider.value);

        sw.Close();
    }
}
