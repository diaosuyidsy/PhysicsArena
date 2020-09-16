using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;

/// <summary>
/// Game Feel Manager Manages Controller vibration
/// and screen shakes
/// </summary>
public class GameFeelManager
{
    public GameFeelData GameFeelData;
    public GameFeelManager(GameFeelData _gfd)
    {
        GameFeelData = _gfd;
        OnEnable();
    }
    #region Event Handlers
    private void _onPlayerHit(PlayerHit ph)
    {
        CameraShake.CS.Shake(0.1f, 0.1f);
        _vibrateController(ph.HittedPlayerNumber, 1.0f, 0.25f);

        /// If the hiter number is below 0, means it's a block
        /// and blocked attack don't have a hitter
        _vibrateController(ph.HiterPlayerNumber, 1.0f, 0.15f);
        // Also Shake the hitted
        // ph.Hitted.transform.DOShakePosition(GameFeelData.MeleeHitStopInformation.Frames * Time.unscaledDeltaTime,
        // GameFeelData.MeleeHitStopInformation.Viberation,
        // GameFeelData.MeleeHitStopInformation.Vibrato,
        // GameFeelData.MeleeHitStopInformation.Randomness).SetUpdate(true).SetEase(GameFeelData.MeleeHitStopInformation.ViberationEase);
        // EventManager.Instance.TriggerEvent(new HitStopEvent(GameFeelData.MeleeHitStopInformation.Frames, GameFeelData.MeleeHitStopInformation.TimeScale));
    }

    private void _onPlayerDied(PlayerDied pd)
    {
        CameraShake.CS.Shake(0.1f, 0.1f);
        _vibrateController(pd.PlayerNumber, 1.0f, 0.25f);
    }

    private void _onPlayerFireWaterGun(WaterGunFired wf)
    {
        _vibrateController(wf.WaterGunOwnerPlayerNumber);
    }

    private void _onPlayerFireHookGun(HookGunFired hf)
    {
        _vibrateController(hf.HookGunOwnerPlayerNumber, 1.0f, 0.1f);
    }

    private void _onHookHit(HookHit hh)
    {
        _vibrateController(hh.HookedPlayerNumber);
        _vibrateController(hh.HookGunOwnerPlayerNumber);
    }

    private void _onSuckGunFire(SuckGunFired sf)
    {
        _vibrateController(sf.SuckGunOwnerPlayerNumber);
    }

    private void _onSuckGunSuck(SuckGunSuck ss)
    {
        _vibrateController(ss.SuckedPlayersNumber);
        _vibrateController(ss.SuckGunOwnerPlayerNumber);
    }

    private void _onPlayerStune(PlayerStunned ev)
    {

    }

    private void _onPlayerRespawned(PlayerRespawned ev)
    {
        if (ev.Player.GetComponent<PlayerController>() == null) return;
        _vibrateController(ev.Player.GetComponent<PlayerController>().PlayerNumber
        , GameFeelData.PlayerRespawnViberationInformation.MotorLevel
        , GameFeelData.PlayerRespawnViberationInformation.Duration);
    }

    private void _onObjectPickedUp(ObjectPickedUp ev)
    {
        _vibrateController(ev.PlayerNumber
        , GameFeelData.ObjectPickedUpViberationInformation.MotorLevel
        , GameFeelData.ObjectPickedUpViberationInformation.Duration);
    }

    private void _onHookBlocked(HookBlocked ev)
    {
        _vibrateController(ev.HookBlockerPlayerNumber
        , GameFeelData.HookBlockerViberationInformation.MotorLevel
        , GameFeelData.HookBlockerViberationInformation.Duration);
        _vibrateController(ev.HookGunOwnerPlayerNumber
         , GameFeelData.HookBlockedViberationInformation.MotorLevel
         , GameFeelData.HookBlockedViberationInformation.Duration);
    }

    private void _onFistGunFired(FistGunFired ev)
    {
        _vibrateController(ev.FistGunOwnerPlayerNumber
        , GameFeelData.FistGunFireViberationInformation.MotorLevel
        , GameFeelData.FistGunFireViberationInformation.Duration);
    }

    private void _onFistGunHit(FistGunHit ev)
    {
        _vibrateController(ev.FistGunOwnerPlayerNumber
        , GameFeelData.FistGunHitterViberationInformation.MotorLevel
        , GameFeelData.FistGunHitterViberationInformation.Duration);
        _vibrateController(ev.HittedPlayerNumber
        , GameFeelData.FistGunHittedViberationInformation.MotorLevel
        , GameFeelData.FistGunHittedViberationInformation.Duration);
    }

    private void _onFistGunBlocked(FistGunBlocked ev)
    {
        _vibrateController(ev.FistGunOwnerPlayerNumber
        , GameFeelData.FistGunBlockedViberationInformation.MotorLevel
        , GameFeelData.FistGunBlockedViberationInformation.Duration);
        _vibrateController(ev.BlockerPlayerNumber
        , GameFeelData.FistGunBlockerViberationInformation.MotorLevel
        , GameFeelData.FistGunBlockerViberationInformation.Duration);
    }

    private void _onFistGunCharged(FistGunCharged ev)
    {
        _vibrateController(ev.FistGunOwnerPlayerNumber
        , GameFeelData.FistGunChargedViberationInformation.MotorLevel
        , GameFeelData.FistGunChargedViberationInformation.Duration);
    }

    private void _onBazookaFired(BazookaFired ev)
    {
        _vibrateController(ev.PlayerNumber
        , GameFeelData.BazookaFiredViberationInformation.MotorLevel
        , GameFeelData.BazookaFiredViberationInformation.Duration);
    }

    private void _onBazookaBombed(BazookaBombed ev)
    {
        _vibrateController(ev.PlayerNumber
        , GameFeelData.BazookaBomberViberationInformation.MotorLevel
        , GameFeelData.BazookaBomberViberationInformation.Duration);

        _vibrateController(ev.HitPlayersNumber
        , GameFeelData.BazookaBombedViberationInformation.MotorLevel
        , GameFeelData.BazookaBombedViberationInformation.Duration);
    }

    private void _onFoodDelieverd(FoodDelivered ev)
    {
        _vibrateController(ev.DeliverPlayerNumber
        , GameFeelData.FoodDeliveredViberationInformation.MotorLevel
        , GameFeelData.FoodDeliveredViberationInformation.Duration);
    }

    #endregion

    public void ViberateController(int playernumber, float motorlevel = 1.0f, float duration = 0.15f)
    {
        _vibrateController(playernumber, motorlevel, duration);
    }

    private void _vibrateController(int playernumber, float motorlevel = 1.0f, float duration = 0.15f)
    {
        if (playernumber < 0 || playernumber >= ReInput.players.playerCount) return;
        Player player = ReInput.players.GetPlayer(playernumber);
        player.SetVibration(0, motorlevel, duration);
        player.SetVibration(1, motorlevel, duration);
    }

    private void _vibrateController(List<int> playernumbers, float motorlevel = 1.0f, float duration = 0.15f)
    {
        foreach (int playernumber in playernumbers)
        {
            _vibrateController(playernumber, motorlevel, duration);
        }
    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<WaterGunFired>(_onPlayerFireWaterGun);
        EventManager.Instance.AddHandler<HookGunFired>(_onPlayerFireHookGun);
        EventManager.Instance.AddHandler<HookHit>(_onHookHit);
        EventManager.Instance.AddHandler<PlayerStunned>(_onPlayerStune);
        EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawned);
        EventManager.Instance.AddHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.AddHandler<HookBlocked>(_onHookBlocked);
        EventManager.Instance.AddHandler<FistGunFired>(_onFistGunFired);
        EventManager.Instance.AddHandler<FistGunHit>(_onFistGunHit);
        EventManager.Instance.AddHandler<FistGunBlocked>(_onFistGunBlocked);
        EventManager.Instance.AddHandler<FistGunCharged>(_onFistGunCharged);
        EventManager.Instance.AddHandler<BazookaFired>(_onBazookaFired);
        EventManager.Instance.AddHandler<BazookaBombed>(_onBazookaBombed);
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelieverd);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<WaterGunFired>(_onPlayerFireWaterGun);
        EventManager.Instance.RemoveHandler<HookGunFired>(_onPlayerFireHookGun);
        EventManager.Instance.RemoveHandler<HookHit>(_onHookHit);
        EventManager.Instance.RemoveHandler<PlayerStunned>(_onPlayerStune);
        EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawned);
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.RemoveHandler<HookBlocked>(_onHookBlocked);
        EventManager.Instance.RemoveHandler<FistGunFired>(_onFistGunFired);
        EventManager.Instance.RemoveHandler<FistGunHit>(_onFistGunHit);
        EventManager.Instance.RemoveHandler<FistGunBlocked>(_onFistGunBlocked);
        EventManager.Instance.RemoveHandler<FistGunCharged>(_onFistGunCharged);
        EventManager.Instance.RemoveHandler<BazookaFired>(_onBazookaFired);
        EventManager.Instance.RemoveHandler<BazookaBombed>(_onBazookaBombed);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelieverd);
    }

    public void Destory()
    {
        OnDisable();
    }
}
