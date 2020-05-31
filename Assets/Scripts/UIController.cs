using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public LayerMask UILayers;

    private float yDifference;
    private Vector3 _pos;
    private Vector3 _initialScale;
    public float YOffset = 0.1f;

    private void Start()
    {
        yDifference = transform.parent.position.y - transform.position.y;
        _initialScale = transform.localScale;
    }

    private void Update()
    {
        _pos = transform.parent.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.parent.position, Vector3.down, out hit, 10f, UILayers))
        {
            _pos.y = hit.collider.bounds.center.y;
            _pos.y += hit.collider.bounds.extents.y + YOffset;
            float yDiff = Mathf.Abs(_pos.y - transform.parent.position.y);
            Color temp = GetComponent<SpriteRenderer>().color;
            temp.a = (3f - yDiff) / 3f;
            GetComponent<SpriteRenderer>().color = temp;
            transform.localScale = _initialScale * ((3f - yDiff) / 3f);
        }
        else
        {
            _pos.y = -100f;
        }
        transform.position = _pos;
        transform.eulerAngles = new Vector3(90f, transform.parent.eulerAngles.y, 0f);


    }
}
