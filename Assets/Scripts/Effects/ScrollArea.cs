﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollArea : MonoBehaviour
{
    public float ScrollAmount = 2f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.tag.Contains("Team")) return;
        IHittable pc = other.GetComponent<IHittable>();
        if (pc != null)
        {
            pc.OnImpact(-transform.right * ScrollAmount, ForceMode.VelocityChange, null, ImpactType.Self);
        }
    }
}