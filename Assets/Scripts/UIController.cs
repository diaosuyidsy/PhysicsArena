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
    public float YOffset = 0.1f;

    private void Start ()
    {
        yDifference = transform.position.y - UI.position.y;
        _pos = transform.position;
        _rot = transform.rotation;
    }

    private void Update ()
    {
        _pos = transform.position;
        RaycastHit hit;
        if (Physics.Raycast (transform.position, Vector3.down, out hit, 100f, UILayers))
        {
            _pos.y = hit.transform.position.y;
            _pos.y += hit.collider.bounds.extents.y + YOffset;
            float yDiff = Mathf.Abs (_pos.y - transform.position.y);
            Color temp = UI.GetComponent<SpriteRenderer> ().color;
            temp.a = (-180f * yDiff + 318.75f) / 255f;
            UI.gameObject.GetComponent<SpriteRenderer> ().color = temp;
        }
        else
        {
            _pos.y = -100f;
        }
        UI.position = _pos;
        UI.eulerAngles = new Vector3 (90f, transform.eulerAngles.y, 0f);


    }
}
