using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public LayerMask UILayers;

    private float yDifference;
    private Vector3 _pos;
    public float YOffset = 0.1f;

    private void Start()
    {
        yDifference = transform.parent.position.y - transform.position.y;
    }

    private void Update()
    {
        _pos = transform.parent.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.parent.position, Vector3.down, out hit, 10f, UILayers))
        {
            _pos.y = hit.transform.position.y;
            _pos.y += hit.collider.bounds.extents.y + YOffset;
            float yDiff = Mathf.Abs(_pos.y - transform.position.y);
            Color temp = GetComponent<SpriteRenderer>().color;
            temp.a = (-180f * yDiff + 318.75f) / 255f;
            GetComponent<SpriteRenderer>().color = temp;
        }
        else
        {
            _pos.y = -100f;
        }
        transform.position = _pos;
        transform.eulerAngles = new Vector3(90f, transform.parent.eulerAngles.y, 0f);


    }
}
