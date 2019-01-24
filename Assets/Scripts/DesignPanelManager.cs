using UnityEngine.UI;
using UnityEngine;
using System.IO;

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
    }
    private void OnApplicationQuit()
    {
        string path = "Assets/Resources/DesignNumber.txt";
        StreamWriter sw = new StreamWriter(path, false);
        sw.WriteLine("Melee Charge: " + (MeleeChargeToggle.isOn ? "ON" : "OFF"));
        sw.WriteLine("Melee Double Arm Mode: " + (MeleeChargeToggle.isOn ? "ON" : "OFF"));
        sw.WriteLine("Melee Alternate Schema: " + (MeleeChargeToggle.isOn ? "ON" : "OFF"));
        sw.WriteLine("Melee Charge: " + (MeleeChargeToggle.isOn ? "ON" : "OFF"));

        sw.Close();
    }
}
