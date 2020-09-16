using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class BoltEventBroadcaster
{
    public BoltEventBroadcaster()
    {
        // EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
    }

    public void OnPlayerHit(PlayerHit ev)
    {
        PlayerHitEvent.Post(GlobalTargets.Everyone,
        ev.Hiter.GetComponent<BoltEntity>(),
        ev.Hitted.GetComponent<BoltEntity>(),
        ev.Force,
        ev.HiterPlayerNumber,
        ev.HittedPlayerNumber);
    }

    public void OnPunchStart(PunchStart ev)
    {
        PunchStartEvent.Post(GlobalTargets.Everyone,
        ev.Player.GetComponent<BoltEntity>(),
        ev.PlayerNumber);
    }

    public void OnPunchHolding(PunchHolding ev)
    {
        PunchHoldingEvent.Post(GlobalTargets.Everyone,
                ev.Player.GetComponent<BoltEntity>(),
                ev.PlayerNumber);
    }

    public void OnPunchDone(PunchDone ev)
    {
        PunchDoneEvent.Post(GlobalTargets.Everyone,
                ev.Player.GetComponent<BoltEntity>(),
                ev.PlayerNumber);
    }

    public void OnPunchReleased(PunchReleased ev)
    {
        PunchReleasedEvent.Post(GlobalTargets.Everyone,
                ev.Player.GetComponent<BoltEntity>(),
                ev.PlayerNumber);
    }

    public void OnPunchInterrepted(PunchInterruptted ev)
    {
        PunchInterrupttedEvent.Post(GlobalTargets.Everyone,
                ev.Player.GetComponent<BoltEntity>(),
                ev.PlayerNumber);
    }

    public void OnBlockEnd(BlockEnd ev)
    {
        BlockEndEvent.Post(GlobalTargets.Everyone,
        ev.Player.GetComponent<BoltEntity>(),
        ev.PlayerNumber);
    }

    public void OnBlockStart(BlockStart ev)
    {
        BlockStartEvent.Post(GlobalTargets.Everyone,
        ev.Player.GetComponent<BoltEntity>(),
        ev.PlayerNumber);
    }

    public void OnPlayerDied(PlayerDied ev)
    {
        PlayerDiedEvent.Post(GlobalTargets.Everyone,
        ev.Player.GetComponent<BoltEntity>(),
        ev.PlayerNumber,
        ev.PlayerHitter.GetComponent<BoltEntity>(),
        ev.HitterIsValid,
        (int)ev.ImpactType
        );
    }

    public void OnPlayerJump(PlayerJump ev)
    {
        PlayerJumpEvent.Post(GlobalTargets.Everyone,
        ev.Player.GetComponent<BoltEntity>(),
        ev.PlayerNumber,
        ev.GroundTag);
    }

    public void OnPlayerLand(PlayerLand ev)
    {
        PlayerLandEvent.Post(GlobalTargets.Everyone,
        ev.Player.GetComponent<BoltEntity>(),
        ev.PlayerNumber,
        ev.GroundTag);
    }

    public void OnPlayerRespawned(PlayerRespawned ev)
    {
        PlayerRespawnedEvent.Post(GlobalTargets.Everyone,
        ev.Player.GetComponent<BoltEntity>());
    }

    public void Destroy()
    {
        // EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);

    }
}
