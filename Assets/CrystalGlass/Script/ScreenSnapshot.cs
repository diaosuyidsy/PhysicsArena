using UnityEngine;

public class ScreenSnapshot : MonoBehaviour
{
	public LayerMask m_GlassLayers = -1;
	[Range(2f, 4f)] public float m_BlurIntensity = 4f;
	public bool m_RenderAlphaMask = true;
	public bool m_ShowRt = false;
	RenderTexture m_Rt;
	RenderTexture m_RtBlur;
	Camera m_RTCam;
	Shader m_SdrReplace;
	Material m_MatBlur;

	void Start ()
	{
		m_Rt = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
		m_Rt.name = "Scene";
		m_RtBlur = new RenderTexture (Screen.width / 2, Screen.height / 2, 24, RenderTextureFormat.ARGB32);
		m_RtBlur.name = "Scene Blur";
		
		m_SdrReplace = Shader.Find ("Crystal Glass/Refraction/Mesh Alpha");
		
		Shader sdr = Shader.Find ("Crystal Glass/Blur");
		m_MatBlur = new Material (sdr);
	}
	void Update ()
	{
		Camera c = Camera.main;
		if (m_RTCam == null)
		{
			GameObject go = new GameObject ("RtCamera", typeof (Camera), typeof (Skybox));
			m_RTCam = go.GetComponent<Camera> ();
			go.transform.parent = c.transform;
		}
		m_RTCam.CopyFrom (c);
		m_RTCam.targetTexture = m_Rt;
		m_RTCam.cullingMask &= ~m_GlassLayers;
		m_RTCam.enabled = false;
		if (m_RTCam.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox skyCurr = c.GetComponent (typeof (Skybox)) as Skybox;
			Skybox skyRt = m_RTCam.GetComponent (typeof (Skybox)) as Skybox;
			skyRt.enabled = true;
			skyRt.material = skyCurr.material;
		}
		m_RTCam.Render ();   // draw scene without glass layer
		if (m_RenderAlphaMask)
		{
			m_RTCam.cullingMask = 0;
			m_RTCam.cullingMask |= m_GlassLayers;
			m_RTCam.clearFlags = CameraClearFlags.Nothing;
			m_RTCam.SetReplacementShader (m_SdrReplace, "");
			m_RTCam.Render ();   // draw glass layer to alpha channel
		}
		Shader.SetGlobalTexture ("_Global_ScreenTex", m_Rt);
		
		// blur the scene rt
		RenderTexture rt1 = RenderTexture.GetTemporary (m_RtBlur.width, m_RtBlur.height);
		RenderTexture rt2 = RenderTexture.GetTemporary (m_RtBlur.width, m_RtBlur.height);
		Graphics.Blit (m_Rt, rt1);   // down sample

		m_MatBlur.SetVector ("_Offsets", new Vector4 (m_BlurIntensity / Screen.width, 0, 0, 0));
		Graphics.Blit (rt1, rt2, m_MatBlur);
		m_MatBlur.SetVector ("_Offsets", new Vector4 (0, m_BlurIntensity / Screen.height, 0, 0));
		Graphics.Blit (rt2, rt1, m_MatBlur);
		float f = m_BlurIntensity * 2f;
		m_MatBlur.SetVector ("_Offsets", new Vector4 (f / Screen.width, 0, 0, 0));
		Graphics.Blit (rt1, rt2, m_MatBlur);
		m_MatBlur.SetVector ("_Offsets", new Vector4(0, f / Screen.height, 0, 0));
		Graphics.Blit (rt2, m_RtBlur, m_MatBlur);

		Shader.SetGlobalTexture ("_Global_ScreenBlurTex", m_RtBlur);
		RenderTexture.ReleaseTemporary (rt1);
		RenderTexture.ReleaseTemporary (rt2);
	}
	void OnGUI ()
	{
		if (m_ShowRt)
		{
			GUI.DrawTexture (new Rect (10, 10, 128, 128), m_Rt);
			GUI.DrawTexture (new Rect (10, 138, 128, 128), m_RtBlur);
		}
	}
}