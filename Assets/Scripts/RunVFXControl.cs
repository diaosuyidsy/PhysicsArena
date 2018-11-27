using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunVFXControl : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            Instantiate(GameManager.GM.RunSandVFX, transform.position, Quaternion.identity);
        }
    }

}
