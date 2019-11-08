using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class DepthOfFieldEffect : MonoBehaviour
{
    [Range(-1f, 1f)]
    public float redChannelMove = 0.408f;
    [Range(-1f, 1f)]
    public float blueChannelMove = -0.368f;
    [Range(-1f, 1f)]
    public float greenChannelMove = 0f;
    [Range(0.1f, 100f)]
    public float focusDistance = 10f;
    [Range(0.1f, 15f)]
    public float focusRange = 7.6f;
    [Range(1f, 10f)]
    public float bokehRadius = 10f;
    [HideInInspector]
    public Shader dofShader;

    [NonSerialized]
    Material dofMaterial;
    
    const int circleOfConfusionPass = 0;
    const int preFilterPass = 1;
    const int bokehPass = 2;
    const int postFilterPass = 3;
    const int combinePass = 4;

    private Camera camera;

    private void OnEnable()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        focusDistance = camera.fieldOfView + 150f / camera.fieldOfView;
    }

    void OnRenderImage (RenderTexture source, RenderTexture destination) {
        if (dofMaterial == null) {
            dofMaterial = new Material(dofShader);
            dofMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        dofMaterial.SetFloat("_FocusDistance", focusDistance);
        dofMaterial.SetFloat("_FocusRange", focusRange);
        dofMaterial.SetFloat("_BokehRadius", bokehRadius);
        dofMaterial.SetFloat("_RedChannelMove", redChannelMove);
        dofMaterial.SetFloat("_BlueChannelMove", blueChannelMove);
        dofMaterial.SetFloat("_GreenChannelMove", greenChannelMove);
        
        RenderTexture coc = RenderTexture.GetTemporary(
            source.width, source.height, 0,
            RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear
        );

        int width = source.width / 2;
        int height = source.height / 2;
        RenderTextureFormat format = source.format;
        RenderTexture dof0 = RenderTexture.GetTemporary(width, height, 0, format);
        RenderTexture dof1 = RenderTexture.GetTemporary(width, height, 0, format);

        dofMaterial.SetTexture("_CoCTex", coc);
        dofMaterial.SetTexture("_DoFTex", dof0);

        Graphics.Blit(source, coc, dofMaterial, circleOfConfusionPass);
        Graphics.Blit(source, dof0, dofMaterial, preFilterPass);
        Graphics.Blit(dof0, dof1, dofMaterial, bokehPass);
        Graphics.Blit(dof1, dof0, dofMaterial, postFilterPass);
        Graphics.Blit(source, destination, dofMaterial, combinePass);


        RenderTexture.ReleaseTemporary(coc);
        RenderTexture.ReleaseTemporary(dof0);
        RenderTexture.ReleaseTemporary(dof1);
    }
}
