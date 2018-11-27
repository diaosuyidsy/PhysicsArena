using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPositionControl : MonoBehaviour
{
    [HideInInspector]
    public GameObject Owner;

    // Update is called once per frame
    void Update()
    {
        if (Owner != null)
        {
            GameObject lh = Owner.GetComponent<PlayerController>().LeftHand;
            GameObject rh = Owner.GetComponent<PlayerController>().RightHand;
            transform.position = (lh.transform.position + rh.transform.position) / 2f;
            transform.eulerAngles = new Vector3(0f, Owner.transform.eulerAngles.y + 90f, 10f);
        }
    }

    public void Drop()
    {
        Owner = null;
    }
}
