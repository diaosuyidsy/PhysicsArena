using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "ComicMenuData", menuName = "ScriptableObjects/ComicMenuData", order = 1)]
public class ComicMenuData : MonoBehaviour
{
    [Header("Cover Page Related Settings")]
    public Color SelectedFillColor = Color.red;
    public Color SelectedTextColor = Color.white;
    public Color UnselectedFillColor = Color.white;
    public Color UnselectedTextColor = Color.black;
    public Vector3 SelectedMenuItemScale = new Vector3(1.2f, 1.2f, 1.2f);
    public Vector3 UnselectedMenuItemScale = new Vector3(0.968f, 0.968f, 0.968f);
    public float SelectedMenuItemDuration = 0.2f;
    public float UnselectedMenuItemDuartion = 0f;
    public Ease SelectedMenuItemEase;
    public Ease UnSelectedMenuItemEase;
}
