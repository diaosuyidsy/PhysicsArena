using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleFieldControl : MonoBehaviour
{
    public float VisibleOnHitTime = 0.5f;
    private readonly int _visibleRenderQueue = 1998;
    private void Awake()
    {
        EventManager.Instance.AddHandler<PunchHolding>(_onPlayerHoldPunch);
        EventManager.Instance.AddHandler<PunchDone>(_onPunchDone);
        EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<BlockStart>(_onBlockStart);
        EventManager.Instance.AddHandler<BlockEnd>(_onBlockEnd);
    }

    private void _onPlayerHoldPunch(PunchHolding ev)
    {
        ev.Player.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue = _visibleRenderQueue;
    }

    private void _onPlayerReleasedPunch(PunchReleased ev)
    {
        ev.Player.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue = 2000;
    }

    private void _onPunchDone(PunchDone ev)
    {
        ev.Player.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue = 2000;
    }

    private void _onPlayerHit(PlayerHit ev)
    {
        if (ev.Hitted.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue == 2000)
        {
            StartCoroutine(_startInvisible(VisibleOnHitTime, ev.Hitted));
        }
    }

    private void _onPlayerDied(PlayerDied ev)
    {
        ev.Player.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue = 2000;
    }

    private void _onBlockStart(BlockStart ev)
    {
        ev.Player.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue = _visibleRenderQueue;

    }

    private void _onBlockEnd(BlockEnd ev)
    {
        ev.Player.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue = 2000;
    }

    IEnumerator _startInvisible(float time, GameObject Player)
    {
        Player.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue = _visibleRenderQueue;
        yield return new WaitForSeconds(time);
        Player.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue = 2000;
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<PunchHolding>(_onPlayerHoldPunch);
        EventManager.Instance.RemoveHandler<PunchDone>(_onPunchDone);
        EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<BlockStart>(_onBlockStart);
        EventManager.Instance.RemoveHandler<BlockEnd>(_onBlockEnd);
    }
}
