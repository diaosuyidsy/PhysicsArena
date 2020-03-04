using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RechoosePoint : MonoBehaviour
{
    public ComicMenu ComicMenu;
    private int _colorIndex { get { return transform.GetSiblingIndex(); } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            if (other.transform.GetSiblingIndex() == _colorIndex)
            {
                other.GetComponent<Rigidbody>().isKinematic = true;
                ComicMenu.OnPlayerRechoose(other.GetComponent<PlayerController>().PlayerNumber);
            }
        }
    }
}
