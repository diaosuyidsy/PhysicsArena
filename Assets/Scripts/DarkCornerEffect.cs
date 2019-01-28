using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DarkCornerEffect : MonoBehaviour
{

    public float Length;
    public Vector2 CenterPosition;

    private Material _material;

    private void Awake()
    {
        _material = new Material(Shader.Find("Hidden/DCEffect"));
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetFloat("_dcLength", Length);
        _material.SetVector("_CenterPoint", CenterPosition);
        Graphics.Blit(source, destination, _material);

    }
}
