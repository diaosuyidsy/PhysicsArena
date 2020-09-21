using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectID))]
public class CritterObject : MonoBehaviour, IHittable
{
    public float ExplosionForceMultiplier = 0.01f;
    public float SuckForceMultiplier = 0.05f;
    public float OtherForceMultiplier = 0.05f;
    private Rigidbody _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    public bool CanBeBlockPushed()
    {
        return false;
    }

    public bool CanBlock(Vector3 forwardAngle)
    {
        return false;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void OnImpact(Vector3 force, ForceMode forcemode, GameObject enforcer, ImpactType impactType)
    {
        if (impactType == ImpactType.BazookaGun)
            _rb.AddForce(force * ExplosionForceMultiplier, forcemode);
        else if (impactType == ImpactType.SuckGun)
            _rb.AddForce(force * SuckForceMultiplier, forcemode);

    }

    public void OnImpact(GameObject enforcer, ImpactType impactType)
    {
        return;
    }

    public void OnImpact(Status status)
    {
        return;
    }

    public void SetVelocity(Vector3 vel)
    {
        _rb.velocity = vel;
    }

    public bool CanDefend(Vector3 forwardAngle)
    {
        throw new System.NotImplementedException();
    }
}
