using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

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
    public float PlayTextAfterDelay = 0.5f;
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

    [Header("Map to 1st Menu Transition Setting")]
    public Vector3Ease MapToFirstCameraPositionEase;
    public FloatEase MapToFirstCameraFOVEase;
    public Vector3Ease MapToFirstFirstMenuMove1;
    public Vector3Ease MapToFirstFirstMenuMove2;
    public Vector3Ease MaptoFirstFirstMenuRotate1;
    [Header("Map Selection Related Settings")]
    public Color LeftRightClickColor = Color.white;
    public float LeftRightClickDuration = 0.1f;

    [Header("Map To Character Transition Settings")]
    public float InitialStopDuration = 0.2f;
    public Vector3 CameraStopLocation1 = new Vector3(0.421f, 2.551f, -0.02f);
    public float CameraToCharacterMoveDuration1 = 0.5f;
    public Ease CameraMoveEase1 = Ease.OutQuad;
    public float CameraStopDuration1 = 0.5f;
    public Vector3 CameraStopLocation2 = new Vector3(0.421f, 2.551f, -0.22f);
    public float CameraToCharacterMoveDuration2 = 0.5f;
    public Ease CameraMoveEase2 = Ease.OutQuad;
    [Header("Character To Map Transition Settings")]
    public float CharacterToMapInitialStopDuration = 0.2f;

    public Vector3Ease CharacterToMapCameraLocationEase;
    [Header("Character Selection Settings")]
    public Vector2 MouseClampMaxValue = new Vector2(0.218f, -0.074f);
    public Vector2 MouseClampMinValue = new Vector2(-0.22f, -0.306f);
    public Color EggNormalOutlineColor = Color.black;
    public Color EggCursorOverOutlineColor = Color.white;
    public Color HoleNormalColor = Color.white;
    public Color HoverImageColor = Color.grey;
    public Color[] HoleCursorveHoverColor;
    public Color[] HoleSelectedColor;
    public Vector3 EggActivatedScale;
    public Vector3 EggNormalScale = 0.8f * Vector3.one;
    public Vector2 CursorMoveSpeed = new Vector2(50f, 30f);
    public LayerMask EggLayer;
    public float ETC_EggShakeDuration = 0.2f;
    public float ETC_EggShakeStrength = 90f;
    public int ETC_EggShakeVibrato = 10;
    public Vector3 ETC_EggScaleAmount = new Vector3(1.2f, 1.2f, 1.2f);
    public float ETC_EggScaleDuration = 0.2f;
    public AnimationCurve ETC_EggScaleAnimationCurve;
    public float ETC_EggMoveYAmount = 10f;
    public float ETC_EggMoveYDuration = 0.5f;
    public AnimationCurve ETC_EggMoveYAnimationCurve;
    public float ETC_ChickenMoveYAmount = 10f;
    public float ETC_ChickenMoveYDuration = 0.3f;
    public Ease ETC_ChickenMoveYEase = Ease.OutQuad;
    public float ETC_ChickenMoveYDelay = 0.3f;

    public Color[] BirdColor;
}

[Serializable]
public class Vector3Ease
{
    public Vector3 EndValue;
    public float Duration;
    public bool Relative = false;
    public Ease Ease = Ease.OutQuad;
}

[Serializable]
public class FloatEase
{
    public float EndValue;
    public float Duration;
    public bool Relative = false;
    public Ease Ease = Ease.OutQuad;
}