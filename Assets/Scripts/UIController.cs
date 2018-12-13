using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public Transform UI;
    public LayerMask UILayers;

    private float yDifference;
    private Vector3 _pos;
    private Quaternion _rot;

    private void Start ()
    {
        yDifference = transform.position.y - UI.position.y;
        _pos = transform.position;
        _rot = transform.rotation;
    }

    private void Update ()
    {
        _pos = transform.position;
        _pos.y = transform.position.y - yDifference;
        UI.position = _pos;
        UI.eulerAngles = new Vector3 (90f, transform.eulerAngles.y, 0f);
        //RaycastHit hit;
        //if (Physics.Raycast (transform.position, Vector3.down, out hit, UILayers))
        //{
        //    //_pos.y =
        //}

    }
}
