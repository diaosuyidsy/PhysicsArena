using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CountDownVideoPlayer : MonoBehaviour
{
    public RawImage rawImage;
    private VideoPlayer videoplayer;

    private void Start()
    {
        videoplayer = GetComponent<VideoPlayer>();
        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        videoplayer.Prepare();
        WaitForSeconds wfs = new WaitForSeconds(1f);
        while (!videoplayer.isPrepared)
        {
            yield return wfs;
            break;
        }
        rawImage.texture = videoplayer.texture;
        videoplayer.Play();

    }

}
