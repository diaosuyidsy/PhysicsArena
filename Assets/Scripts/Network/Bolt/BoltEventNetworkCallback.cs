using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[BoltGlobalBehaviour]
public class BoltEventNetworkCallback : GlobalEventListener
{
    public override void OnEvent(PlayerHitEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PlayerHit(ev.Hitter.gameObject,
        ev.Hitted.gameObject,
        ev.Force,
        ev.HiterPN,
        ev.HittedPN, 1f,
        false));
    }

    public override void OnEvent(PunchStartEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PunchStart(ev.Player.gameObject,
        ev.PlayerNumber,
        ev.Player.GetComponent<BoltPlayerController>().RightHand.transform));
    }

    public override void OnEvent(PunchHoldingEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PunchHolding(ev.Player.gameObject,
        ev.PlayerNumber,
        ev.Player.GetComponent<BoltPlayerController>().RightHand.transform));
    }

    public override void OnEvent(PunchDoneEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PunchDone(ev.Player.gameObject,
        ev.PlayerNumber,
        ev.Player.GetComponent<BoltPlayerController>().RightHand.transform));
    }

    public override void OnEvent(PunchReleasedEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PunchReleased(ev.Player.gameObject,
        ev.PlayerNumber));
    }

    public override void OnEvent(PunchInterrupttedEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PunchInterruptted(ev.Player.gameObject,
        ev.PlayerNumber));
    }

    public override void OnEvent(BlockEndEvent ev)
    {
        EventManager.Instance.TriggerEvent(new BlockEnd(ev.Player.gameObject, ev.PN));
    }

    public override void OnEvent(BlockStartEvent ev)
    {
        EventManager.Instance.TriggerEvent(new BlockStart(ev.Player.gameObject, ev.PN));
    }

    public override void OnEvent(PlayerDiedEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PlayerDied(ev.Player.gameObject, ev.PN, ev.Enemy.gameObject,
        ev.HItterIsValid, ev.ImpactTypeIndex));
    }

    public override void OnEvent(PlayerJumpEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PlayerJump(ev.Player.gameObject,
        ev.Player.GetComponent<IBodyConfiguration>().PlayerFeet.gameObject,
        ev.PN, ev.GroundTag));
    }

    public override void OnEvent(PlayerLandEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PlayerLand(ev.Player.gameObject,
        ev.Player.GetComponent<IBodyConfiguration>().PlayerFeet.gameObject,
        ev.PN, ev.GroundTag));
    }

    public override void OnEvent(PlayerRespawnedEvent ev)
    {
        EventManager.Instance.TriggerEvent(new PlayerRespawned(ev.Player.gameObject));
    }
}
