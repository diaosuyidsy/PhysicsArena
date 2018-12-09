using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunStickPositionControl : MonoBehaviour
{
    public Transform MachineGun;

    private Vector3 scale;

    private void Update ()
    {
        scale = transform.localScale;
        scale.y = MachineGun.transform.localPosition.y * 1.94f - 0.28f;
        transform.localScale = scale;
    }

}
