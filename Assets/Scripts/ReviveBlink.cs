using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveBlink : MonoBehaviour
{
    private float BlinkTime = 3f;
    private float BlinkSpeed = 10f;
    private bool isBlinking = false;
    private void Awake()
    {
        EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawned);
        BlinkTime = Services.Config.GameMapData.InvincibleTime;
    }

    private void _onPlayerRespawned(PlayerRespawned ev)
    {

        foreach (Transform child in ev.Player.transform)
        {
            Renderer r = child.gameObject.GetComponent<Renderer>();
            
            if (r!= null && r.material.name.Contains("CustomComic"))
            {
                StartCoroutine(_startBlinking(BlinkTime, r));
            }

            
        }

        
        
    }


    IEnumerator _startBlinking(float time, Renderer r)
    {
        float curTime = 0;
        float deltaTime = BlinkSpeed * Time.deltaTime;
        while (curTime < time)
        {
            if (r.enabled)
            {
                r.enabled = false;
            }
            else
            {
                r.enabled = true;
            }

            curTime += deltaTime;
            yield return new WaitForSeconds(deltaTime);
        }

        r.enabled = true;
    }
    
    
    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawned);

    }
}
