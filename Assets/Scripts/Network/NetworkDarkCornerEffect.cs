﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkDarkCornerEffect : NetworkBehaviour
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
