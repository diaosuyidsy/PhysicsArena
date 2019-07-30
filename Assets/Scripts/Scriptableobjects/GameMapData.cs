using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "GameMapData", menuName = "ScriptableObjects/GameMapData", order = 1)]
public class GameMapData : ScriptableObject
{
	public GameMapMode GameMapMode;
	public float TutorialTitleEnterDuration;
	public ScrambleMode TutorialTitleScrambleMode;
	public float TutorialTitleAfterDelay = 0.5f;
}
