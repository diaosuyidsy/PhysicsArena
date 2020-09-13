using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[BoltGlobalBehaviour]
public class BoltNetworkCallback : GlobalEventListener
{
    public override void SceneLoadLocalDone(string scene)
    {
        var spawnPosition = new Vector3(14.5f, 0.5f, -17f);
        BoltNetwork.Instantiate(BoltPrefabs.Bolt_Player_Phoenix_Yellow, spawnPosition, Quaternion.identity);

    }

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
}
