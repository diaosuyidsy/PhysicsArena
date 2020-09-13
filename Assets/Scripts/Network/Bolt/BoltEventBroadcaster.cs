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
        var hitEvent = PlayerHitEvent.Post(GlobalTargets.Everyone,
        ev.Hiter.GetComponent<BoltPlayerController>().entity,
        ev.Hitted.GetComponent<BoltPlayerController>().entity,
        ev.Force,
        ev.HiterPlayerNumber,
        ev.HittedPlayerNumber);
    }

    public void OnPunchStart(PunchStart ev)
    {
        var fireevent = PunchStartEvent.Post(GlobalTargets.Everyone,
        ev.Player.GetComponent<BoltPlayerController>().entity,
        ev.PlayerNumber);
    }

    public void OnPunchHolding(PunchHolding ev)
    {
        var fireevent = PunchHoldingEvent.Post(GlobalTargets.Everyone,
                ev.Player.GetComponent<BoltPlayerController>().entity,
                ev.PlayerNumber);
    }

    public void OnPunchDone(PunchDone ev)
    {
        var fireevent = PunchDoneEvent.Post(GlobalTargets.Everyone,
                ev.Player.GetComponent<BoltPlayerController>().entity,
                ev.PlayerNumber);
    }

    public void OnPunchReleased(PunchReleased ev)
    {
        var fireevent = PunchReleasedEvent.Post(GlobalTargets.Everyone,
                ev.Player.GetComponent<BoltPlayerController>().entity,
                ev.PlayerNumber);
    }

    public void OnPunchInterrepted(PunchInterruptted ev)
    {
        var fireevent = PunchInterrupttedEvent.Post(GlobalTargets.Everyone,
                ev.Player.GetComponent<BoltPlayerController>().entity,
                ev.PlayerNumber);
    }

    public void Destroy()
    {
        // EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);

    }
}
