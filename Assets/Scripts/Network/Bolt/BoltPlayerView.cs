using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltPlayerView : MonoBehaviour
{
    private BoltPlayerController _controllerRef;
    private Transform _controllerTransform;
    private CharacterData _characterData;
    private float _interpolationMultiplication = 1f;
    private Rigidbody _rb;
    private bool _interpolate = true;

    private void Awake()
    {
        _controllerRef = GetComponentInParent<BoltPlayerController>();
        _characterData = _controllerRef.CharacterDataStore;
        _controllerTransform = _controllerRef.transform;
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!_interpolate) return;
        transform.position = Vector3.Lerp(transform.position, _controllerTransform.position, _characterData.MovementInterpolation * _interpolationMultiplication);
        transform.rotation = Quaternion.Lerp(transform.rotation, _controllerTransform.rotation, _characterData.MovementInterpolation * _interpolationMultiplication);
    }

    public void FootStep(int foot = 0)
    {
        _controllerRef.FootStep(foot);
    }

    public void OnForceImpact(float time, Vector3 force, ForceMode fm = ForceMode.Impulse)
    {
        _rb.isKinematic = false;
        // _interpolate = false;
        _rb.AddForce(force, fm);
        SetInterpolation(time, 0f);
    }

    public void SetInterpolation(float time, float mul)
    {
        StartCoroutine(_setinte(time, mul));
    }

    IEnumerator _setinte(float time, float mul)
    {
        // _interpolate = false;
        _rb.isKinematic = false;
        _interpolationMultiplication = mul;
        yield return new WaitForSeconds(time);
        _rb.isKinematic = true;
        float elapsedTime = 0f;
        float ping = 0f;
        if (BoltNetwork.IsConnected && BoltNetwork.IsClient)
            ping = BoltNetwork.Server.PingAliased / 1000f;
        while (elapsedTime < ping)
        {
            elapsedTime += Time.deltaTime;
            _interpolationMultiplication = elapsedTime / ping;
            yield return null;
        }
        _interpolationMultiplication = 1f;
        // _interpolate = true;
    }
}
