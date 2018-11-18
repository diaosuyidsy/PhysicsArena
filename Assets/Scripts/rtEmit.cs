using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class rtEmit : MonoBehaviour
{
	public GameObject WaterBall;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		float Fire = Input.GetAxis("XboxRT");
		Fire = Mathf.Approximately(Fire, 0f) || Mathf.Approximately(Fire, -1f) ? 0f : 1f;
		WaterBall.GetComponent<ObiEmitter>().speed = Fire * 8f;
	}
}
