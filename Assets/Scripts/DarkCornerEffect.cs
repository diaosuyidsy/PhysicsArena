using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DarkCornerEffect : MonoBehaviour
{

    public float length;
    private Material material;

    private void Awake()
    {
        material = new Material(Shader.Find("Hidden/DCEffect"));
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("_dcLength", length);
        Graphics.Blit(source, destination, material);

    }
}
