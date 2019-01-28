using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DarkCornerEffect : MonoBehaviour
{

    public float Length;
    public Vector2 CenterPosition;

    public Material Material;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Material.SetFloat("_dcLength", Length);
        Material.SetVector("_CenterPoint", CenterPosition);
        Graphics.Blit(source, destination, Material);

    }
}
