using UnityEngine;

public class Demo : MonoBehaviour
{
	public enum EModel { Teapot = 0, Bunny };
	[Header("Glass Object")]
	public EModel m_ShowModel = EModel.Teapot;
	public GameObject m_GoTeapot;
	public GameObject m_GoBunny;
	[Header("Material Parameters")]
	[Range(-0.95f, -0.8f)] public float m_IOR = -0.9f;
	[Range(-0.05f, 0.05f)] public float m_IOROffset = 0.02f;
	[Range(0.1f, 8f)] public float m_FresnelPower = 1.55f;
	[Range(0f, 2f)]	public float m_FresnelAlpha = 1f;
	public Color m_FresnelColor1 = Color.white;
	public Color m_FresnelColor2 = Color.white;
	[Range(0f, 1f)] public float m_Transparency = 1f;
	public Cubemap m_Env;
	public Texture2D m_Normal;
	[Range(1f, 8f)] public float m_NormalUvScale = 3f;
	public Texture2D m_Rainbow;
	public bool m_UseCubeLod = false;
	[Range(0f, 7f)]	public float m_CubeLod = 1f;
	public Texture2D m_Sparkle;
	[Header("Internal")]
	public Material[] m_Mats;
	public Shader[] m_Sdrs;
	bool m_ToggleBump = false;
	bool m_ToggleSparkle = false;
	int m_UseShaderIndex = 0;

	void Start ()
	{
		QualitySettings.antiAliasing = 8;
		
		m_Sdrs = new Shader[4];
		m_Sdrs[0] = Shader.Find ("Crystal Glass/Opacity");
		m_Sdrs[1] = Shader.Find ("Crystal Glass/Single Pass Transparency");
		m_Sdrs[2] = Shader.Find ("Crystal Glass/Double Pass Transparency");
		m_Sdrs[3] = Shader.Find ("Crystal Glass/Rainbow");
		
		Renderer rd1 = m_GoTeapot.GetComponent<Renderer> ();
		Renderer rd2 = m_GoBunny.GetComponent<Renderer> ();
		int n = rd1.materials.Length + rd2.materials.Length;
		m_Mats = new Material[n];
		m_Mats[0] = rd1.materials[0];
		m_Mats[1] = rd2.materials[0];
		m_Mats[2] = rd2.materials[1];
		Shader.DisableKeyword ("CRYSTAL_GLASS_BUMP");
	}
	void Update ()
	{
		if (m_ShowModel == EModel.Teapot)
		{
			m_GoTeapot.SetActive (true);
			m_GoBunny.SetActive (false);
		}
		else
		{
			m_GoTeapot.SetActive (false);
			m_GoBunny.SetActive (true);
		}
		
		for (int i = 0; i < m_Mats.Length; i++)
		{
			m_Mats[i].shader = m_Sdrs[m_UseShaderIndex];
			m_Mats[i].SetTexture ("_EnvTex", m_Env);
			m_Mats[i].SetTexture ("_NormalTex", m_Normal);
			m_Mats[i].SetTextureScale ("_NormalTex", new Vector2 (m_NormalUvScale, m_NormalUvScale));
			m_Mats[i].SetTexture ("_RainbowTex", m_Rainbow);
			m_Mats[i].SetFloat ("_IOR", m_IOR);
			m_Mats[i].SetFloat ("_IOROffset", m_IOROffset);
			m_Mats[i].SetFloat ("_FresnelPower", m_FresnelPower);
			m_Mats[i].SetFloat ("_FresnelAlpha", m_FresnelAlpha);
			m_Mats[i].SetColor ("_FresnelColor1", m_FresnelColor1);
			m_Mats[i].SetColor ("_FresnelColor2", m_FresnelColor2);
			m_Mats[i].SetFloat ("_Transparency", m_Transparency);
			m_Mats[i].SetFloat ("_Lod", m_CubeLod);
			m_Mats[i].SetTexture ("_SparkleNoiseTex", m_Sparkle);
			
			if (m_ToggleBump)
				m_Mats[i].EnableKeyword ("CRYSTAL_GLASS_BUMP");
			else
				m_Mats[i].DisableKeyword ("CRYSTAL_GLASS_BUMP");
			
			if (m_UseCubeLod)
				m_Mats[i].EnableKeyword ("CRYSTAL_GLASS_LOD");
			else
				m_Mats[i].DisableKeyword ("CRYSTAL_GLASS_LOD");
				
			if (m_ToggleSparkle)
				m_Mats[i].EnableKeyword ("CRYSTAL_GLASS_SPARKLE");
			else
				m_Mats[i].DisableKeyword ("CRYSTAL_GLASS_SPARKLE");
		}
	}
	void OnGUI()
	{
		int btnW = 180;
		int btnH = 30;
		GUI.Label (new Rect (10, 10, 250, 30), "Crystal Glass Demo");
		if (GUI.Button (new Rect (10, btnH * 0 + 40, btnW, btnH), "Opacity"))
			m_UseShaderIndex = 0;
		if (GUI.Button (new Rect (10, btnH * 1 + 40, btnW, btnH), "Single Pass Transparency"))
			m_UseShaderIndex = 1;
		if (GUI.Button (new Rect (10, btnH * 2 + 40, btnW, btnH), "Double Pass Transparency"))
			m_UseShaderIndex = 2;
		m_ToggleBump = GUI.Toggle (new Rect(10, btnH * 3 + 50, 120, btnH), m_ToggleBump, "Bump Mapping");
		if (GUI.Button (new Rect (10, btnH * 4 + 50, btnW, btnH), "Rainbow Crystal Glass"))
			m_UseShaderIndex = 3;
		m_ToggleSparkle = GUI.Toggle (new Rect(10, btnH * 5 + 60, 120, btnH), m_ToggleSparkle, "Sparkle");
	}
}