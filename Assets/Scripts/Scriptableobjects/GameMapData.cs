using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "GameMapData", menuName = "ScriptableObjects/GameMapData", order = 1)]
public class GameMapData : ScriptableObject
{
	public GameMapMode GameMapMode;
	[Header("Landing State Setting")]
	public AnimationCurve BirfiaTitleFadeInOutCurve;
	public float BirfiaTitalFadeInOutDuration = 5f;

	public float BirdsFlyDownDuration = 2f;
	public float[] BirdsFlyDownDelay;
	public Ease BirdsFlyDownEase = Ease.OutQuad;

	public AnimationCurve CameraRotateCurve;
	public float CameraMoveDuration;
}
