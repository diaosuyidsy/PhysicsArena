using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureManager
{
	private Camera m_SnapshotCamera;
	private List<Texture2D> m_SnapshotList;
	private SnapshotData m_SnapshotData;

	public CaptureManager(SnapshotData ssd)
	{
		m_SnapshotData = ssd;
		m_SnapshotList = new List<Texture2D>();
		m_SnapshotCamera = GameObject.Find("CaptureCamera").GetComponent<Camera>();
		m_SnapshotCamera.gameObject.SetActive(false);
	}

	private void m_OnPlayerPunch(PlayerHit ev)
	{

	}

	public void Destroy()
	{

	}

	public void LateUpdate()
	{
		if(m_SnapshotCamera.gameObject.activeInHierarchy)
		{
			m_SnapshotCamera.gameObject.SetActive(false);
		}
	}
}
