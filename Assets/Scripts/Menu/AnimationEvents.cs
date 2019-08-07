using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationEvents : MonoBehaviour
{
    public void DoCameraAnimation(string cameraTag)
    {
        GameObject.FindGameObjectWithTag(cameraTag).GetComponent<DOTweenAnimation>().DORestartAllById("Sit");
    }
}
