using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistHitGroundSound : MonoBehaviour
{
    private bool _hitGround;
    private void OnCollisionEnter(Collision other)
    {
        if (!_hitGround && other.gameObject.tag.Contains("Ground"))
        {
            _hitGround = true;
            // GetComponent<AudioSource>().PlayOneShot(Services.AudioManager.AudioDataStore.FistHitGroundAudioClip);
        }
    }
}
