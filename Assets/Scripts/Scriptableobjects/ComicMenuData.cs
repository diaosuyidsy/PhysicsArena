using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "ComicMenuData", menuName = "ScriptableObjects/ComicMenuData", order = 1)]
public class ComicMenuData : ScriptableObject
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
    [Header("1st Menu To 2nd Menu Transition Settings")]
    public Color PlayTextBlinkColor = Color.black;
    public float PlayTextBlinkDuration = 0.4f;
    public float PlayTextBlinkTime = 0.2f;
    public float PlayTextBlinkPeriod = 0.2f;
    public Vector3 CoverPageLocalMovement = new Vector3(-0.8f, 0f, 0f);
    public float CoverPageMovementDuration = 0.35f;
    public Ease CoverPageMovementEase = Ease.OutQuad;
    public Vector3 CoverPageLocalRotation = new Vector3(0f, -30f, 0f);
    public float CoverPageRotateDuration = 0.4f;
    public Ease CoverPageRotateEase = Ease.OutQuad;
    public Vector3 CoverPageReturnLocalMovement = new Vector3(0.8f, 0f, 0f);
    public float CoverpageReturnDuration = 0.35f;
    public Ease CoverPageReturnEase = Ease.OutQuad;
    public Vector3 CoverPageReturnLocalRotation = new Vector3(0f, -15f, 0f);
    public float CoverPageReturnRotateDuration = 0.2f;
    public Ease CoverPageReturnRotateEase = Ease.InQuad;
    public float AfterCoverPageWaitDuration = 0.5f;
    public Vector3 CameraMapSelectionWorldLocation = new Vector3(0.421f, 2.551f, 0.166f);
    public float CameraMapSelectionFOV = 10.5f;
    public float CameraToMapSelectionDuration = 0.5f;
    public Ease CameraToMapSelectionEase = Ease.OutQuad;

    [Header("Map Selection Related Settings")]
    public Color LeftRightClickColor = Color.white;
    public float LeftRightClickDuration = 0.1f;
}
