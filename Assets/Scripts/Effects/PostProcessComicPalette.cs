using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessComicPalette : MonoBehaviour
{
    public Color[] Colors;
    
    private MaterialPropertyBlock materialPropertyBlock;
    private Renderer _screenRender;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        _screenRender = GetComponent<Renderer>();
        Vector4[] colorArray = new Vector4[Colors.Length];
        for (int i = 0; i < Colors.Length; i++)
        {
            colorArray[i] = Colors[i];
        }

        materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetVectorArray("ColorsArray",colorArray);
        _screenRender.SetPropertyBlock(materialPropertyBlock);

    }
}
