using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SnapshotData", menuName = "ScriptableObjects/SnapshotData", order = 1)]
public class SnapshotData : ScriptableObject
{
	public int SnapshotList = 3;

	public List<int> PunchRenderDelayFrame;
	public Vector3 PunchRenderRelativePosition1;
	public Vector3 PunchRenderRelativePosition2;
	public Vector3 PunchRenderRelativePosition3;

}
