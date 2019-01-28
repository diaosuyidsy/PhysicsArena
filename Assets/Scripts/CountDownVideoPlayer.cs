using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CountDownVideoPlayer : MonoBehaviour
{
    public RawImage rawImage;
    private VideoPlayer videoplayer;

    private void Awake()
    {
        videoplayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        videoplayer.Prepare();
        //videoplayer.loopPointReached += _videoEndReach;
    }

    public void PlayTheVideo()
    {
        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        WaitForSeconds wfs = new WaitForSeconds(0f);
        while (!videoplayer.isPrepared)
        {
            yield return wfs;
            break;
        }
        rawImage.texture = videoplayer.texture;
        videoplayer.Play();
    }

    private void _videoEndReach(UnityEngine.Video.VideoPlayer vp)
    {
        MenuController.MC.LoadingEnd();
    }

}
