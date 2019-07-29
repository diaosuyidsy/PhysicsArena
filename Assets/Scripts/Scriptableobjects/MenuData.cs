using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "MenuData", menuName = "ScriptableObjects/MenuData", order = 1)]
public class MenuData : ScriptableObject
{
	[Header("First Menu Setting")]
	public Color SelectingFontColor;
	public Color NormalFontColor;
	public Color SelectedFontColor;
	public Ease FirstMenuSelectionEase;
	public float FirstMenuSelectionTransitionDuration = 0.05f;

	[Header("First Menu to Second Menu Setting")]
	public float FirstMenuSelectedBlinkDurition = 0.1f;
	public float FirstMenuSelectedBlinkTime = 5f;
	[Tooltip("Indicates the power in time of the ease, and must be between -1 and 1. 0 is balanced, 1 fully weakens the ease in time, -1 starts the ease fully weakened and gives it power towards the end.")]
	public float FirstMenuSelectedBlinkPeriod = 0f;

	public float FirstMenuTitleMoveOutPosition = -1500f;
	public float FirstMenuTitleMoveOutDuration = 1f;
	public float FirstMenuTitleMoveOutDelay = 0f;
	public Ease FirstMenuTitleMoveOutEase;

	public float FirstMenuPlayMoveOutPosition = -1500f;
	public float FirstMenuPlayMoveOutDuration = 1f;
	public float FirstMenuPlayMoveOutDelay = 0f;
	public Ease FirstMenuPlayMoveOutEase;

	public float FirstMenuSettingMoveOutPosition = -1500f;
	public float FirstMenuSettingMoveOutDuration = 1f;
	public float FirstMenuSettingMoveOutDelay = 0f;
	public Ease FirstMenuSettingMoveOutEase;

	public float FirstMenuQuitMoveOutPosition = -1500f;
	public float FirstMenuQuitMoveOutDuration = 1f;
	public float FirstMenuQuitMoveOutDelay = 0f;
	public Ease FirstMenuQuitMoveOutEase;

	public float FirstMenuToSecondCameraMoveTime = 1f;
	public float FirstMenuToSecondCameraMoveDelay = 0.4f;
	public Ease FirstMenuToSecondCameraEase;

	public float SecondMenuCarModeMoveTime = 0.5f;
	public float SecondMenuCarModeMoveDelay = 0.4f;
	public Ease SecondMenuCarModeEase;

	public float SecondMenuBrawlModeMoveTime = 0.5f;
	public float SecondMenuBrawlModeMoveDelay = 0.4f;
	public Ease SecondMenuBrawlModeEase;

	public float SecondMenuTitleMoveTime = 0.5f;
	public float SecondMenuTitleMoveDelay = 0.4f;
	public string SecondMenuTitleString = "Press A To Select";

	[Header("Second Menu to First Menu Setting")]

	public float SecondMenuTitleMoveOutTime = 0.5f;
	public float SecondMenuTitleMoveOutDelay = 0f;

	public float SecondMenuCarModeMoveOutTime = 0.5f;
	public float SecondMenuCarModeMoveOutDelay = 0.4f;

	public float SecondMenuBrawlModeMoveOutTime = 0.5f;
	public float SecondMenuBrawlModeMoveOutDelay = 0.4f;

	public float SecondMenuToFirstCameraMoveTime = 1f;
	public float SecondMenuToFirstCameraMoveDelay = 0.3f;
	public Ease SecondMenuToFirstCameraEase;

	public float FirstMenuTitleMoveInDuration = 1f;
	public float FirstMenuTitleMoveInDelay = 0f;
	public Ease FirstMenuTitleMoveInEase;

	public float FirstMenuPlayMoveInDuration = 1f;
	public float FirstMenuPlayMoveInDelay = 0f;
	public Ease FirstMenuPlayMoveInEase;

	public float FirstMenuSettingMoveInDuration = 1f;
	public float FirstMenuSettingMoveInDelay = 0f;
	public Ease FirstMenuSettingMoveInEase;

	public float FirstMenuQuitMoveInDuration = 1f;
	public float FirstMenuQuitMoveInDelay = 0f;
	public Ease FirstMenuQuitMoveInEase;

	[Header("Mode Select Setting")]
	public Color ModeNormalColor;
	public Color ModeSelectedBlinkColor;
	public float ModeSelectedBlinkDurition = 0.1f;
	public float ModeSelectedBlinkTime = 5f;
	[Tooltip("Indicates the power in time of the ease, and must be between -1 and 1. 0 is balanced, 1 fully weakens the ease in time, -1 starts the ease fully weakened and gives it power towards the end.")]
	public float ModeSelectedBlinkPeriod = 0f;

	public float ModeImageMoveDuration = 0.3f;
	public float ModeImageMoveDelay = 0f;
	public Ease ModeImageMoveEase;

	public float ModeMapMoveInDuration = 0.3f;
	public float ModeMapMoveInDelay = 0f;
	public Ease ModeMapMoveInEase;

	public float ModePanelSelectedZoomDuration = 0.2f;
	public Ease ModePanelSelectedZoomEase;

	public float MapMoveDuration = 0.2f;
	public Ease MapMoveEase;

	public Color SelectedMapColor;
	public Color UnselectedMapColor;

	[Header("Mode Select To Character Select Setting")]
	public float PanelMoveOutDuration = 0.2f;
	public Ease PanelMoveOutEase;

	public float BrawlPanelMoveOutDelay = 0.4f;
	public float CartPanelMoveOutDelay = 0.2f;
	public float TextMoveOutDelay = 0.2f;

	public float CameraToCharacterSelectionMoveDuration = 0.5f;
	public float CameraToCharacterSelectionMoveDelay = 0.4f;
	public Ease CameraToCharacterSelectionMoveEase;

	public float ThirdMenuTitleMoveInDuration = 0.2f;
	public float ThirdMenuTitleMoveInDelay = 1f;

	public float[] ThirdMenuHolderMoveInDuration;
	public float[] ThirdMenuHolderMoveInDelay;
	public Ease ThirdMenuHolderMoveInEase;

	public Vector3 InstantiatedMapFinalScale;
	public float InstantiatedMapScaleDuration = 1f;
	public Ease InstantiatedMapScaleEase;

	public Color InstantiaedMapColor;
	public float InstantiatedMapColorDuration = 1f;
	public Ease InstantiatedMapColorEase;


	[Header("Character Select To Mode Select Setting")]
	public float[] ThirdMenuHolderMoveOutDuration;
	public float[] ThirdMenuHolderMoveOutDelay;
	public Ease ThirdMenuHolderMoveOutEase;

	public float ThirdMenuTitleMoveOutDuration;
	public float ThirdMenuTitleMoveOutDelay;

	public float CameraFromCharacterSelectionToModeSelectMoveDuration = 1f;
	public float CameraFromCharacterSelectionToModeSelectMoveDelay = 1f;
	public Ease CameraFromCharacterSelectionToModeSelectMoveEase;

	[Header("Character Select Setting")]
	public float CursorMoveSpeed = 10f;

	public LayerMask EggLayer;

	public Color EggNormalOutlineColor;
	public Color EggCursorOverOutlineColor;
	public Vector3 EggActivatedScale;

	public Color HoverImageColor;
	public Color[] HoleCursorveHoverColor;
	public Color HoleNormalColor;
	public Color[] HoleSelectedColor;

	[Header("Egg To Chicken Transition Setting")]
	public float ETC_EggShakeDuration = 0.5f;
	public Vector3 ETC_EggShakeStrength;
	public int ETC_EggShakeVibrato = 30;

	public float ETC_EggMoveYAmount = 15f;
	public float ETC_EggMoveYDuration = 0.35f;
	public AnimationCurve ETC_EggMoveYAnimationCurve;

	public Vector3 ETC_EggScaleAmount;
	public float ETC_EggScaleDuration;
	public AnimationCurve ETC_EggScaleAnimationCurve;

	public float ETC_ChickenMoveYDuration = 0.5f;
	public Ease ETC_ChickenMoveYEase;
	public float ETC_ChickenMoveYDelay = 0.3f;
	public GameObject ETC_ChickenLandVFX;
	public Vector3 ETC_ChickenLandVFXOffset;

	public GameObject ETC_ChickenDisappearVFX;
	public Vector3 ETC_ChickenDisapperavFXOffset;
}
