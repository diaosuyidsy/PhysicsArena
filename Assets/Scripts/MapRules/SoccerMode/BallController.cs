using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour, IHittable
{
    public Vector3 InitialPosition;
    public float SoccerForceDamper = 0.1f;
    private Rigidbody _rb;
    private void Awake()
    {
        InitialPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Services.GameStateManager.CameraTargets.Add(transform);
        _rb = GetComponent<Rigidbody>();
        Debug.Assert(_rb != null, "Rigidbody missing on soccer ball");
    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<OnScore>(_onScore);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<OnScore>(_onScore);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Team1Goal"))
        {
            EventManager.Instance.TriggerEvent(new OnScore(1));
        }
        else if (other.name.Contains("Team2Goal"))
        {
            EventManager.Instance.TriggerEvent(new OnScore(2));
        }

        if (other.CompareTag("DeathZone"))
        {
            transform.position = InitialPosition;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    private void _onScore(OnScore ev)
    {
        transform.position = InitialPosition;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public bool CanBeBlockPushed()
    {
        return false;
    }

    public bool CanBlock(Vector3 forwardAngle)
    {
        return false;
    }

    public void OnImpact(Vector3 force, float _meleeCharge, GameObject sender, bool _blockable)
    {

        EventManager.Instance.TriggerEvent(new PlayerHit(sender, gameObject, force, sender.GetComponent<PlayerController>().PlayerNumber, 6, _meleeCharge, false));
        OnImpact(force, ForceMode.Impulse, sender, ImpactType.Melee);

    }

    public void OnImpact(Vector3 force, ForceMode forcemode, GameObject enforcer, ImpactType impactType)
    {
        _rb.AddForce(force * SoccerForceDamper, forcemode);
        OnImpact(enforcer, impactType);
    }

    public void OnImpact(GameObject enforcer, ImpactType impactType)
    {
        return;
    }

    public void OnImpact(Status status)
    {
        return;
    }
}
