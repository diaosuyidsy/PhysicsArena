using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessComicPalette : MonoBehaviour
{
    public Material Mat;
    public List<Color> Colors;

    private void Update()
    {
        Mat.SetColorArray("ColorsArray", Colors);
    }
}
