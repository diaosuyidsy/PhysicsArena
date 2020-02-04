using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class JiffycrewPPLine : MonoBehaviour
{
    private Camera cam;
    private Camera utilCam;
    private GameObject utilCamObject;

    [Header("Background")]
    public bool AugmentSceneColor = false;
    public Color BGColor = new Color(0.75f,0.75f,0.75f,1);

    [Header("Silhouette")]
    public bool SILineEnable = true;
    [Range(0, 5)] public float SILineThickness = 2;
    [Range(0, 0.5f)] public float SILineThreshold= 0.1f;
    public Color SILineColor = new Color(0.5f, 0.5f, 0.5f, 1);

    [Header("Crease")]
    public bool CRLineEnable = true;
    [Range(0, 5)] public float CRLineThickness = 2;
    [Range(0, 0.5f)] public float CRLineThreshold = 0.1f;
    public Color CRLineColor = new Color(0.5f, 0.5f, 0.5f, 1);

    [Header("Suggestive Contour")]
    public bool SCLineEnable = true;
    [Range(0, 5)] public float SCLineThickness = 2;
    [Range(0, 0.5f)] public float SCLineThreshold = 0.05f;
    public Color SCLineColor = new Color(0.5f, 0.5f, 0.5f, 1);

    [Header("Suggestive Highlight")]
    public bool SHLineEnable = true;
    [Range(0,5)]public float SHLineThickness = 2;
    [Range(0,0.5f)]public float SHLineThreshold = 0.05f;
    public Color SHLineColor = new Color(0.125f, 0.125f, 0.125f, 1);

    [Header("Materials")]
    public Texture2D defaultNormalTex;
    public Material depthNormalMaterial;
    public Material lineMaterial;
    public Material compositeMaterial;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.DepthNormals;

        if (utilCamObject == null)
        {
            utilCamObject = GameObject.Find("JiffycrewUtilCam");
            if (utilCamObject == null)
            {
                utilCamObject = new GameObject("JiffycrewUtilCam", typeof(Camera));
            }                
            utilCam = utilCamObject.GetComponent<Camera>();
            utilCam.enabled = false;
            utilCam.transform.parent = this.transform;
        }

#if UNITY_EDITOR
        defaultNormalTex = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/JiffycrewPPLine/Textures/DefaultNormal1x1.png", typeof(Texture2D));
        depthNormalMaterial = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/JiffycrewPPLine/Materials/JiffycrewDepthNormalMaterial.mat", typeof(Material));
        lineMaterial = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/JiffycrewPPLine/Materials/JiffycrewPPLineMaterial.mat", typeof(Material));
        compositeMaterial = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/JiffycrewPPLine/Materials/JiffycrewPPLineComposite.mat", typeof(Material));
#endif
    }

    private void OnDisable()
    {
        DestroyImmediate(utilCamObject);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //Depth and Normal
        RenderTexture depthNormalTexture = RenderTexture.GetTemporary(Screen.width*2, Screen.height*2, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1);
        depthNormalTexture.filterMode = FilterMode.Point;

        utilCam.targetTexture = dest;

        if (!enabled || depthNormalMaterial == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        Shader.SetGlobalTexture(Shader.PropertyToID("_BumpMap"), defaultNormalTex);

        utilCam.CopyFrom(cam);
        utilCam.renderingPath = RenderingPath.Forward; // force forward
        utilCam.clearFlags = CameraClearFlags.Color;
        utilCam.depthTextureMode = DepthTextureMode.DepthNormals;
        utilCam.backgroundColor = Color.clear;

        utilCam.targetTexture = depthNormalTexture;
        utilCam.RenderWithShader(depthNormalMaterial.shader, "RenderType");

        //Line Drawing
        RenderTexture lineTexture = RenderTexture.GetTemporary(Screen.width*2, Screen.height*2, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear, 1);
        lineTexture.filterMode = FilterMode.Bilinear;

        utilCam.targetTexture = lineTexture;

        lineMaterial.SetTexture("_DepthNormalTexture", depthNormalTexture);

        lineMaterial.SetFloat("_ScreenWidth", Screen.width);
        lineMaterial.SetFloat("_ScreenHeight", Screen.height);

        lineMaterial.SetFloat("_AugmentSceneColor", System.Convert.ToSingle(AugmentSceneColor));
        lineMaterial.SetColor("_BGColor", BGColor);

        lineMaterial.SetFloat("_SILineEnable", System.Convert.ToSingle(SILineEnable));
        lineMaterial.SetFloat("_SILineThickness", SILineThickness);
        lineMaterial.SetFloat("_SILineThreshold", SILineThreshold);
        lineMaterial.SetColor("_SILineColor", SILineColor);

        lineMaterial.SetFloat("_CRLineEnable", System.Convert.ToSingle(CRLineEnable));
        lineMaterial.SetFloat("_CRLineThickness", CRLineThickness);
        lineMaterial.SetFloat("_CRLineThreshold", CRLineThreshold);
        lineMaterial.SetColor("_CRLineColor", CRLineColor);

        lineMaterial.SetFloat("_SCLineEnable", System.Convert.ToSingle(SCLineEnable));
        lineMaterial.SetFloat("_SCLineThickness", SCLineThickness);
        lineMaterial.SetFloat("_SCLineThreshold", SCLineThreshold);
        lineMaterial.SetColor("_SCLineColor", SCLineColor);

        lineMaterial.SetFloat("_SHLineEnable", System.Convert.ToSingle(SHLineEnable));
        lineMaterial.SetFloat("_SHLineThickness", SHLineThickness);
        lineMaterial.SetFloat("_SHLineThreshold", SHLineThreshold);
        lineMaterial.SetColor("_SHLineColor", SHLineColor);

        Vector4 sceneTexelSize = new Vector4(1.0f / Screen.width, 1.0f / Screen.height);

        lineMaterial.SetVector("_SceneTexelSize", sceneTexelSize);

        float CAMERA_NEAR = GetComponent<Camera>().nearClipPlane;
        float CAMERA_FAR = GetComponent<Camera>().farClipPlane;
        float CAMERA_FOV = GetComponent<Camera>().fieldOfView;
        float CAMERA_ASPECT_RATIO = GetComponent<Camera>().aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovWHalf = CAMERA_FOV * 0.5f;

        Vector3 toRight = GetComponent<Camera>().transform.right * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * CAMERA_ASPECT_RATIO;
        Vector3 toTop = GetComponent<Camera>().transform.up * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 topLeft = (GetComponent<Camera>().transform.forward * CAMERA_NEAR - toRight + toTop);
        float CAMERA_SCALE = topLeft.magnitude * CAMERA_FAR / CAMERA_NEAR;

        topLeft.Normalize();
        topLeft *= CAMERA_SCALE;

        Vector3 topRight = (GetComponent<Camera>().transform.forward * CAMERA_NEAR + toRight + toTop);
        topRight.Normalize();
        topRight *= CAMERA_SCALE;

        Vector3 bottomRight = (GetComponent<Camera>().transform.forward * CAMERA_NEAR + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= CAMERA_SCALE;

        Vector3 bottomLeft = (GetComponent<Camera>().transform.forward * CAMERA_NEAR - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= CAMERA_SCALE;

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        lineMaterial.SetMatrix("_FrustumCornersWS", frustumCorners);
        lineMaterial.SetVector("_CameraWS", GetComponent<Camera>().transform.position);

        CustomGraphicsBlit(src, lineTexture, lineMaterial);
        CustomGraphicsBlit(lineTexture, dest, compositeMaterial);

        RenderTexture.ReleaseTemporary(depthNormalTexture);
        RenderTexture.ReleaseTemporary(lineTexture);
    }

    void CustomGraphicsBlit(RenderTexture src, RenderTexture dest, Material material)
    {
        Graphics.SetRenderTarget(dest);

        RenderTexture.active = dest;

        material.SetTexture("_MainTex", src);

        GL.PushMatrix();
            GL.LoadOrtho();
            material.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.MultiTexCoord2(0, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.1f); // BL
            GL.MultiTexCoord2(0, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.1f); // BR
            GL.MultiTexCoord2(0, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 0.1f); // TR
            GL.MultiTexCoord2(0, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.1f); // TL
            GL.End();
        GL.PopMatrix();
    }

}
