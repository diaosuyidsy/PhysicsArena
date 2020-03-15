using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingShotControl : MonoBehaviour
{
    public Transform LeftAnchorPoint;
    public Transform RightAnchorPoint;
    public Vector2 LeftAnchorPointYClamp;
    public Vector2 RightAnchorPointYClamp;

    private void LateUpdate()
    {
        Vector3 leftRotation = LeftAnchorPoint.eulerAngles;
        leftRotation.y = Mathf.Clamp(leftRotation.y, LeftAnchorPointYClamp.x, LeftAnchorPointYClamp.y);
        Vector3 rightRotation = RightAnchorPoint.eulerAngles;
        leftRotation.y = Mathf.Clamp(rightRotation.y, RightAnchorPointYClamp.x, RightAnchorPointYClamp.y);
        LeftAnchorPoint.eulerAngles = leftRotation;
        RightAnchorPoint.eulerAngles = rightRotation;
    }
}
