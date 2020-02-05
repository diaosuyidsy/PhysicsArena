using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureManager
{
	private Transform m_SnapshotCameraHolder;
	private List<Texture2D> m_SnapshotList;
	private SnapshotData m_SnapshotData;
	private bool m_PlayerPunchSnapshot;
	private Transform m_SnapshotCanvas;
	private int m_MaxShots;
	private int m_resWidth;
	private int m_resHeight;
	private List<int> m_ShotsDelay;
	private List<Camera> m_SnapshotCameras;

	public CaptureManager(SnapshotData ssd)
	{
		m_SnapshotData = ssd;
		m_SnapshotList = new List<Texture2D>();
		m_SnapshotCameraHolder = GameObject.Find("SnapshotCamera").transform;
		m_SnapshotCameras = new List<Camera>();
		m_ShotsDelay = new List<int>();
		for(int i = 0; i < m_SnapshotCameraHolder.childCount; i++)
		{
			m_SnapshotCameras.Add(m_SnapshotCameraHolder.GetChild(i).GetComponent<Camera>());
		}
		m_SnapshotCanvas = GameObject.Find("SnapShotCanvas").transform;
		m_MaxShots = m_SnapshotData.SnapshotList;
		m_resWidth = m_SnapshotCameraHolder.GetChild(0).GetComponent<Camera>().targetTexture.width;
		m_resHeight = m_SnapshotCameraHolder.GetChild(0).GetComponent<Camera>().targetTexture.height;
		EventManager.Instance.AddHandler<PlayerHit>(m_OnPlayerPunch);
	}

	private void m_OnPlayerPunch(PlayerHit ev)
	{
		m_PlayerPunchSnapshot = true;
		m_TurnCameras(true);

		/// Position Cameras
		/// 1. Relative to the puncher's head
		/// 2. Relative to the puncher's hand
		/// 3. Relative to the punchee's head
		m_SnapshotCameras[0].transform.position = ev.Hiter.transform.position
			+ ev.Hiter.transform.right * m_SnapshotData.PunchRenderRelativePosition1.x
			+ ev.Hiter.transform.up * m_SnapshotData.PunchRenderRelativePosition1.y
			+ ev.Hiter.transform.forward * m_SnapshotData.PunchRenderRelativePosition1.z;
		m_SnapshotCameras[0].transform.LookAt(ev.Hiter.transform);
		m_SnapshotCameras[1].transform.position = ev.Hiter.transform.position
			+ ev.Hiter.transform.right * m_SnapshotData.PunchRenderRelativePosition2.x
			+ ev.Hiter.transform.up * m_SnapshotData.PunchRenderRelativePosition2.y
			+ ev.Hiter.transform.forward * m_SnapshotData.PunchRenderRelativePosition2.z;
		m_SnapshotCameras[1].transform.LookAt(ev.Hiter.transform);
		m_SnapshotCameras[2].transform.position = ev.Hiter.transform.position
			+ ev.Hiter.transform.right * m_SnapshotData.PunchRenderRelativePosition3.x
			+ ev.Hiter.transform.up * m_SnapshotData.PunchRenderRelativePosition3.y
			+ ev.Hiter.transform.forward * m_SnapshotData.PunchRenderRelativePosition3.z;
		m_SnapshotCameras[2].transform.LookAt(ev.Hiter.transform);
		m_ShotsDelay.Clear();
		for(int i = 0; i < m_SnapshotData.PunchRenderDelayFrame.Count; i++)
		{
			m_ShotsDelay.Add(m_SnapshotData.PunchRenderDelayFrame[i]);
		}
	}

	private void m_TurnCameras(bool on)
	{
		for(int i = 0; i < m_SnapshotCameras.Count; i++)
		{
			m_SnapshotCameras[i].gameObject.SetActive(on);
		}
	}

	public void Destroy()
	{
		EventManager.Instance.RemoveHandler<PlayerHit>(m_OnPlayerPunch);
	}

	public void LateUpdate()
	{
		if(	m_PlayerPunchSnapshot)
		{
			bool done = true;

			for(int i = 0; i < m_SnapshotCameras.Count; i++)
			{
				if(!m_CameraCapture(i))
				{
					done = false;
				}
			}
			if (!done) return;
			m_PlayerPunchSnapshot = false;
		}
	}

	private bool m_CameraCapture(int index)
	{
		m_ShotsDelay[index]--;
		if (m_ShotsDelay[index] > 0) return false;
		if (!m_SnapshotCameras[index].gameObject.activeInHierarchy) return true;
		Texture2D snapshot = new Texture2D(m_resWidth, m_resHeight, TextureFormat.RGB24, false);
		m_SnapshotCameras[index].Render();
		m_SnapshotCameras[index].gameObject.SetActive(false);
		return true;
	}
}
