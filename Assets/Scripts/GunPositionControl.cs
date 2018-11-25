using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPositionControl : MonoBehaviour
{

    public GameObject Owner;

    // Update is called once per frame
    void Update()
    {
        if (Owner != null)
        {
            GameObject lh = Owner.GetComponent<PlayerController>().LeftHand;
            GameObject rh = Owner.GetComponent<PlayerController>().RightHand;
            transform.position = (lh.transform.position + rh.transform.position) / 2f;
            transform.rotation = Owner.transform.rotation;
        }
    }
}
