using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "GameMapData", menuName = "ScriptableObjects/GameMapData", order = 1)]
public class GameMapData : ScriptableObject
{
	public GameMapMode GameMapMode;

	[Header("General Map Settings")]
	public Vector3[] Team1RespawnPoints;
	public Vector3[] Team2RespawnPoints;

	[Header("Tutorial State Setting")]
	public float TutorialImageMoveInDuration = 0.2f;
	public Ease TutorialImageMoveInEase;

	public float HoldAMoveInDuration = 0.2f;
	public float HoldAMoveInDelay = 0.2f;

	public float FillASpeed = 2f;

	[Header("Landing State Setting")]

	public Vector3[] ChickenLandingPosition;
	public Vector3[] DuckLandingPostion;

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
	public Ease TitleTextInCurve;
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
	public float WeaponSpawnCD = 1f;
	public int MaxAmountWeaponAtOnetime = 3;
	public WeaponInformation[] WeaponsInformation;

	public Vector3 WeaponSpawnerSize;

	// Need to manually set up world center until we figure out a way to automatically do it.
	public Vector3 WorldCenter;
	public Vector3 WorldSize;
}
