using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationEvents : MonoBehaviour
{
	public void DoCameraAnimation(string args)
	{
		Camera.main.GetComponent<DOTweenAnimation>().DORestartAllById(args);
	}
}
