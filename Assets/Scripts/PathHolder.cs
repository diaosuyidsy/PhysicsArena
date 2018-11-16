using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathHolder : MonoBehaviour {

	[SerializeField] public List<Waypoint> Waypoints;

	[SerializeField] public float Speed;

	[SerializeField] public float MaxTurnRate;

	private void OnDrawGizmosSelected()
	{
		if (Waypoints == null) return;

		for (int i = 1; i < Waypoints.Count; i++)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(Waypoints[i].Position + transform.position, Waypoints[i-1].Position + transform.position);
		}
			
	}
	
}

[System.Serializable]
public class Waypoint
{
	[SerializeField] public Vector3 Position;
}
