using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSteps : MonoBehaviour
{
    public void FootStep(int foot = 0)
    {
        GetComponentInParent<BoltPlayerView>().FootStep(foot);
    }
}
