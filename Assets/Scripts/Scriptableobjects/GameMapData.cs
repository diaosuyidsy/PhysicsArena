using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
	public Ease CameraMoveEase;

	public float CountDownStartDelay;
	public float[] CountDownDuration;
	public float[] CountDownDelay;
	public float[] CountDownScale;
	public Ease CountDownEase = Ease.OutElastic;

	public float FightScale;
	public float FightDuration;
	public float FightStayOnScreenDuration;
}
