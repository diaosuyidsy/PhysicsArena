using UnityEngine;

public class Glass1 : MonoBehaviour
{
	public Texture2D m_Bump;
	[Range(0f, 0.1f)] public float m_BumpScale = 0.15f;
	public Texture2D m_Tint;
	public Color m_TintColor = Color.white;
	[Range(0f, 1f)]	public float m_TintAmount = 0.1f;
	public bool m_Blur = false;
	Renderer[] m_Rds;
	
	void Start ()
	{
		m_Rds = GetComponentsInChildren<Renderer> ();
		Shader sdr = Shader.Find ("Crystal Glass/Refraction/Mesh");
		for (int i = 0; i < m_Rds.Length; i++)
			m_Rds[i].material.shader = sdr;
	}
	void Update ()
	{
		for (int i = 0; i < m_Rds.Length; i++)
		{
			Renderer rd = m_Rds[i];
			if (m_Blur)
				rd.material.EnableKeyword ("CRYSTAL_GLASS_BLUR");
			else
				rd.material.DisableKeyword ("CRYSTAL_GLASS_BLUR");
			rd.material.SetTexture ("_BumpTex", m_Bump);
			rd.material.SetTexture ("_TintTex", m_Tint);
			rd.material.SetFloat ("_BumpScale", m_BumpScale);
			rd.material.SetFloat ("_TintAmount", m_TintAmount);
			rd.material.SetColor ("_TintColor", m_TintColor);
		}
	}
}