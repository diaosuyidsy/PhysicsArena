using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPositionControl : MonoBehaviour
{
    [HideInInspector]
    public GameObject Owner;
    [Tooltip ("This Angle is for weapon z axis angle")]
    public float DownAngle = 10f;
    [Tooltip ("This Angle is for weapon y axis angle offset, normally it's 90f")]
    public float FaceAngleOffset = 90f;

    public float VerticalOffset = 10f;

    // Update is called once per frame
    void Update ()
    {
        if (Owner != null)
        {
            GameObject lh = Owner.GetComponent<PlayerController> ().LeftHand;
            GameObject rh = Owner.GetComponent<PlayerController> ().RightHand;
            //transform.position = (lh.transform.position + rh.transform.position) / 2f;
            Vector3 targetposition = (lh.transform.position + rh.transform.position) / 2f;
            transform.position = new Vector3(targetposition.x, targetposition.y + VerticalOffset, targetposition.z);
            transform.eulerAngles = new Vector3 (0f, Owner.transform.eulerAngles.y + FaceAngleOffset, DownAngle);
        }
    }

    public void Drop ()
    {
        Owner = null;
    }
}
