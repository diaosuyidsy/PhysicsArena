using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveBlink : MonoBehaviour
{
    public float BlinkTime = 3f;
    private bool isBlinking = false;
    private void Awake()
    {
        EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawned);

    }

    private void _onPlayerRespawned(PlayerRespawned ev)
    {

        foreach (Transform child in ev.Player.transform)
        {
            Renderer r = child.gameObject.GetComponent<Renderer>();
            
            if (r!= null && r.material.name.Contains("CustomComic"))
            {
                StartCoroutine(_startBlinking(BlinkTime, r.material));
            }

            
        }

        
        
    }


    IEnumerator _startBlinking(float time, Material mat)
    {
        mat.SetFloat("_IsBlinking", 1);
        yield return new WaitForSeconds(time);
        mat.SetFloat("_IsBlinking", 0);
    }
    
    
    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawned);

    }
}
