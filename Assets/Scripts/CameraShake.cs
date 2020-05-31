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

    private Vector3 originalPos;

    void Awake()
    {
        CS = this;
        if (camTransform == null)
        {
            camTransform = transform.parent.transform;
        }
        originalPos = camTransform.localPosition;
    }

    public void Shake(float _shakeDuration, float _shakeAmount = 0.15f, float _decreaseFactor = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(_shake(_shakeDuration, _shakeAmount, _decreaseFactor));
    }

    IEnumerator _shake(float _sd, float _sa, float _df)
    {
        float elapsedTime = 0f;
        while (elapsedTime < _sd)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            elapsedTime += Time.deltaTime * _df;
            yield return new WaitForEndOfFrame();
        }
    }
}