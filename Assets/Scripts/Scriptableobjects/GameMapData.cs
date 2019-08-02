using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "GameMapData", menuName = "ScriptableObjects/GameMapData", order = 1)]
public class GameMapData : ScriptableObject
{
	public GameMapMode GameMapMode;
	[Header("Tutorial State Setting")]
	public float TutorialImageMoveInDuration = 0.2f;
	public Ease TutorialImageMoveInEase;

	public float HoldAMoveInDuration = 0.2f;
	public float HoldAMoveInDelay = 0.2f;

	public float FillASpeed = 2f;

	[Header("Landing State Setting")]
	public float BirdsFlyDownDuration = 2f;
	public float[] BirdsFlyDownDelay;
	public Ease BirdsFlyDownEase = Ease.OutQuad;

	public Vector3 CameraMoveToPosition = new Vector3(17.13f, 8.2f, -25.6f);
	public float CameraMoveDuration = 1f;
	public float CameraMoveDelay = 0f;
	public float CameraTargetFOV = 15f;
	public Ease CameraMoveEase;

	public float CountDownStartDelay;
	public Ease FightEase = Ease.OutElastic;

	public float FightDelay = 0.5f;
	public float FightScale;
	public float FightDuration;
	public float FightStayOnScreenDuration;

	[Header("Win State Setting")]
	[Range(0f, 1f)]
	public float DarkCornerMiddlePercentage = 0.75f;
	[Range(0f, 1f)]
	public float DarkCornerFinalPercentage = 0.45f;
	public float DarkCornerToMiddleDuration = 2f;
	public float DarkCornerToFinalDuration = 1f;
	public float DarkCornerMiddleStayDuration = 3f;
	public float TitleTextInDuration = 1.5f;
	public float TitleStayDuration = 1f;
	public float TitleTextOutDuration = 1f;
	public float TitleTextInDelay = 1f;

	[Header("Statistic Setting")]
	public string[] StatisticsNames;
	public string[] StatisticsIntro1;
	public string[] StatisticsIntro2;
	public float StatisticStayTime = 2f;

	[Header("Pause Menu Setting")]
	public AudioMixer BackgroundMusicMixer;

	[Header("Weapon Generation Setting")]
	public int MaxAmountWeaponAtOnetime = 3;
	[Range(0, 8)]
	public int WaterGunSetAmount = 4;
	public int SuckGunSetAmount = 1;
	public int HookGunSetAmount = 1;
	public int BazookaSetAmount = 1;
	public int FistGunSetAmount = 1;
}
