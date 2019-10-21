using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Vector3 InitialPosition;
    private void Awake()
    {
        InitialPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Services.GameStateManager.CameraTargets.Add(transform);
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
}
