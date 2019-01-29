using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsatingGlow : MonoBehaviour
{

	public float glowAmount = 0.2f;
	//private float brightnessMin;
	//private float brightnessMax;

	private float amp;
	public float glowSpeed = 5f;
	
	public MeshRenderer mRend;
	
	private float startingBrightness;

	public bool glow = true;
	
	// Use this for initialization
	void Start () {
		float yPosHigh = mRend.material.GetFloat("_yPosHigh");
		//brightnessMin = yPosHigh * (1f - glowAmount);
		//brightnessMax = yPosHigh * (1f + glowAmount);

		startingBrightness = yPosHigh;
		amp = glowAmount * yPosHigh;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (glow)
		{
			float theta = Time.timeSinceLevelLoad * glowSpeed;
			float brightnessChange = amp * Mathf.Sin(theta);
		
			mRend.material.SetFloat("_yPosHigh", startingBrightness + brightnessChange);
		}

	}

	public void StopGlowing()
	{
		mRend.material.SetFloat("_yPosHigh", startingBrightness);
		glow = false;
	}

	public void StartGlowing()
	{
		glow = true;
	}
}
