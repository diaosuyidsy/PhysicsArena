using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunPositionControl : MonoBehaviour
{
    [HideInInspector]
    public GameObject Owner;
    [Tooltip ("This Angle is for weapon z axis angle")]
    public float DownAngle = 10f;
    [Tooltip ("This Angle is for weapon y axis angle offset, normally it's 90f")]
    public float FaceAngleOffset = 90f;

    public float VerticalOffset = 0f;

    private GameObject lh;
    private GameObject rh;
    private Vector3 targetposition;

    // Update is called once per frame
    void Update ()
    {
        if (Owner != null)
        {
            lh = Owner.GetComponent<PlayerController> ().LeftHand;
            rh = Owner.GetComponent<PlayerController> ().RightHand;
            targetposition = transform.position;
            targetposition.y = ((lh.transform.position + rh.transform.position) / 2f).y;
            transform.position = targetposition;
            transform.eulerAngles = new Vector3 (0f, 0f, DownAngle);
        }
    }

    public void Drop ()
    {
        Owner = null;
    }
}
