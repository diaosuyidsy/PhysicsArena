using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
	public static CameraShake CS;
	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;

	// How long the object should shake for.
	public float shakeDuration = 0f;

	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;

	Vector3 originalPos;

	void Awake()
	{
		CS = this;
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
		}
	}

	void OnEnable()
	{
		originalPos = camTransform.localPosition;
	}

	//void Update()
	//{
	//	if (shakeDuration > 0)
	//	{
	//		camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

	//		shakeDuration -= Time.deltaTime * decreaseFactor;
	//	}
	//	else
	//	{
	//		shakeDuration = 0f;
	//		//camTransform.localPosition = originalPos;
	//	}
	//}

	public void Shake(float _shakeDuration, float _shakeAmount = 0.15f, float _decreaseFactor = 1f)
	{
		StopAllCoroutines();
		StartCoroutine(_shake(_shakeDuration, _shakeAmount, _decreaseFactor));
	}

	IEnumerator _shake(float _sd, float _sa, float _df)
	{
		Vector3 originpos = camTransform.localPosition;
		float elapsedTime = 0f;
		while (elapsedTime < _sd)
		{
			camTransform.localPosition = originpos + Random.insideUnitSphere * shakeAmount;
			elapsedTime += Time.deltaTime * _df;
			yield return new WaitForEndOfFrame();
		}
	}
}