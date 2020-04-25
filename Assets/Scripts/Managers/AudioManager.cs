using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager
{
    public AudioManager()
    {
        OnEnable();
    }

    public void PlaySound(string path, Vector3 position = default(Vector3))
    {
        _playSound(path, position);
    }

    /// FMOD play sound
    private void _playSound(string path, Vector3 position = default(Vector3))
    {
        RuntimeManager.PlayOneShot(path, position);
    }

    #region Event Handlers
    private void _onPlayerHit(PlayerHit ev)
    {
        if (ev.IsABlock)
        {
            ///If hiter is null, then it's a block
            _playSound("event:/SFX/Gameplay/Melee/PlayerPunchBlocked", ev.Hitted.transform.position);
        }
        else
        {
            ///If it's not null, then it's a hit
            _playSound("event:/SFX/Gameplay/Melee/PlayerPunched", ev.Hiter.transform.position);
        }
    }

    private void _onHookGunFired(HookGunFired ev)
    {
        _playSound("event:/SFX/Gameplay/Object/HookGun/HookGunFired", ev.HookGun.transform.position);
    }

    private void _onHookGunHit(HookHit ev)
    {
        _playSound("event:/SFX/Gameplay/Object/HookGun/HookGunHit", ev.HookGun.transform.position);
    }

    private void _onSuckGunFired(SuckGunFired ev)
    {
        _playSound("event:/SFX/Gameplay/Object/SuckGun/SuckGunFired", ev.SuckGun.transform.position);
    }

    private void _onSuckGunSuck(SuckGunSuck ev)
    {
        _playSound("event:/SFX/Gameplay/Object/SuckGun/SuckGunSuck", ev.SuckGun.transform.position);
    }

    private void _onPlayerDied(PlayerDied ev)
    {
        if (ev.Player.tag.Contains("Team1"))
            _playSound("event:/SFX/Gameplay/Characters/ChickenDied", ev.Player.transform.position);
        else
            _playSound("event:/SFX/Gameplay/Characters/DuckDied", ev.Player.transform.position);

    }

    private void _onObjectDespawned(ObjectDespawned ev)
    {
        _playSound("event:/SFX/Gameplay/Object/Other/ObjectDespawned", ev.Obj.transform.position);
    }

    private void _onPlayerRespawned(PlayerRespawned ev)
    {
        _playSound("event:/SFX/Gameplay/Characters/PlayerRespawned", ev.Player.transform.position);
    }

    private void _onWeaponSpawned(WeaponSpawned ev)
    {
        _playSound("event:/SFX/Gameplay/Object/Other/WeaponSpawned", ev.Weapon.transform.position);
    }

    private void _onPunchStart(PunchStart ev)
    {
        _playSound("event:/SFX/Gameplay/Melee/PlayerPunchStart", ev.Player.transform.position);
    }

    private void _onPunchReleased(PunchReleased ev)
    {
        _playSound("event:/SFX/Gameplay/Melee/PlayerPunchReleased", ev.Player.transform.position);
    }

    private void _onFootStep(FootStep ev)
    {
        _playSound("event:/SFX/Gameplay/Characters/FootStepConcrete", ev.PlayerFeet.transform.position);
    }

    private void _onPlayerJump(PlayerJump ev)
    {
        _playSound("event:/SFX/Gameplay/Characters/PlayerJump", ev.PlayerFeet.transform.position);
    }

    private void _onWaterGunFired(WaterGunFired ev)
    {
        _playSound("event:/SFX/Gameplay/Object/WaterGun/WaterGunFired", ev.WaterGun.transform.position);
    }

    private void _onFistGunFired(FistGunFired ev)
    {
        _playSound("event:/SFX/Gameplay/Object/FistGun/FistGunFired", ev.FistGun.transform.position);
    }

    private void _onFistGunHit(FistGunHit ev)
    {
        _playSound("event:/SFX/Gameplay/Object/FistGun/FistGunHit", ev.FistGun.transform.position);
    }

    private void _onFistGunBlocked(FistGunBlocked ev)
    {
        _playSound("event:/SFX/Gameplay/Object/FistGun/FistGunBlocked", ev.FistGun.transform.position);
    }

    private void _onFistGunReload(FistGunStartCharging ev)
    {
        _playSound("event:/SFX/Gameplay/Object/FistGun/FistGunReload", ev.FistGun.transform.position);
    }

    private void _onHookGunBlocked(HookBlocked ev)
    {
        _playSound("event:/SFX/Gameplay/Object/HookGun/HookGunBlocked", ev.HookGun.transform.position);
    }

    private void _onBazookaFired(BazookaFired ev)
    {
        _playSound("event:/SFX/Gameplay/Object/Bazooka/BazookaFired", ev.BazookaGun.transform.position);
    }

    private void _onBazookaBombed(BazookaBombed ev)
    {
        _playSound("event:/SFX/Gameplay/Object/Bazooka/BazookaBombed", ev.BazookaGun.transform.position);
    }

    private void _onBagelExplode(AmmoExplode ev)
    {
        _playSound("event:/SFX/Gameplay/Object/Other/BagelExplode", ev.Pos);
    }

    private void _onObjectPickedUp(ObjectPickedUp ev)
    {
        if (ev.Obj.GetComponent<WeaponBase>() != null)
        {
            _playSound("event:/SFX/Gameplay/Object/Other/WeaponPickedup", ev.Obj.transform.position);
        }
    }

    private void _onFoodDelievered(BagelSent ev)
    {
        _playSound("event:/SFX/Gameplay/Object/Other/BagelDelivered", ev.Basket.transform.position);
    }

    private void _onPlayerLand(PlayerLand ev)
    {
        _playSound("event:/SFX/Gameplay/Characters/PlayerLand", ev.PlayerFeet.transform.position);
    }

    private void _onObjectHitGround(ObjectHitGround ev)
    {
        if (ev.Obj.GetComponent<rtBazooka>() != null)
        {
            _playSound("event:/SFX/Gameplay/Object/Bazooka/BazookaHitGround", ev.Obj.transform.position);
            return;
        }
        if (ev.Obj.GetComponent<rtEmit>() != null)
        {
            _playSound("event:/SFX/Gameplay/Object/WaterGun/WaterGunHitGround", ev.Obj.transform.position);
            return;
        }
        if (ev.Obj.GetComponent<rtFist>() != null)
        {
            _playSound("event:/SFX/Gameplay/Object/FistGun/FistGunHitGround", ev.Obj.transform.position);
            return;
        }
        if (ev.Obj.GetComponent<rtHook>() != null)
        {
            _playSound("event:/SFX/Gameplay/Object/HookGun/HookGunHitGround", ev.Obj.transform.position);
            return;
        }
        if (ev.Obj.GetComponent<rtSuck>() != null)
        {
            _playSound("event:/SFX/Gameplay/Object/SuckGun/SuckGunHitGround", ev.Obj.transform.position);
            return;
        }
    }
    #endregion

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.AddHandler<WaterGunFired>(_onWaterGunFired);
        EventManager.Instance.AddHandler<HookGunFired>(_onHookGunFired);
        EventManager.Instance.AddHandler<HookHit>(_onHookGunHit);
        EventManager.Instance.AddHandler<SuckGunFired>(_onSuckGunFired);
        EventManager.Instance.AddHandler<SuckGunSuck>(_onSuckGunSuck);
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<ObjectDespawned>(_onObjectDespawned);
        EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawned);
        EventManager.Instance.AddHandler<WeaponSpawned>(_onWeaponSpawned);
        EventManager.Instance.AddHandler<PunchStart>(_onPunchStart);
        EventManager.Instance.AddHandler<PunchReleased>(_onPunchReleased);
        EventManager.Instance.AddHandler<FootStep>(_onFootStep);
        EventManager.Instance.AddHandler<PlayerJump>(_onPlayerJump);
        EventManager.Instance.AddHandler<FistGunFired>(_onFistGunFired);
        EventManager.Instance.AddHandler<FistGunHit>(_onFistGunHit);
        EventManager.Instance.AddHandler<FistGunBlocked>(_onFistGunBlocked);
        EventManager.Instance.AddHandler<HookBlocked>(_onHookGunBlocked);
        EventManager.Instance.AddHandler<BazookaFired>(_onBazookaFired);
        EventManager.Instance.AddHandler<BazookaBombed>(_onBazookaBombed);
        EventManager.Instance.AddHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.AddHandler<BagelSent>(_onFoodDelievered);
        EventManager.Instance.AddHandler<PlayerLand>(_onPlayerLand);
        EventManager.Instance.AddHandler<ObjectHitGround>(_onObjectHitGround);
        EventManager.Instance.AddHandler<AmmoExplode>(_onBagelExplode);
        EventManager.Instance.AddHandler<FistGunStartCharging>(_onFistGunReload);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.RemoveHandler<WaterGunFired>(_onWaterGunFired);
        EventManager.Instance.RemoveHandler<HookGunFired>(_onHookGunFired);
        EventManager.Instance.RemoveHandler<HookHit>(_onHookGunHit);
        EventManager.Instance.RemoveHandler<SuckGunFired>(_onSuckGunFired);
        EventManager.Instance.RemoveHandler<SuckGunSuck>(_onSuckGunSuck);
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<ObjectDespawned>(_onObjectDespawned);
        EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawned);
        EventManager.Instance.RemoveHandler<WeaponSpawned>(_onWeaponSpawned);
        EventManager.Instance.RemoveHandler<PunchStart>(_onPunchStart);
        EventManager.Instance.RemoveHandler<PunchReleased>(_onPunchReleased);
        EventManager.Instance.RemoveHandler<FootStep>(_onFootStep);
        EventManager.Instance.RemoveHandler<PlayerJump>(_onPlayerJump);
        EventManager.Instance.RemoveHandler<FistGunFired>(_onFistGunFired);
        EventManager.Instance.RemoveHandler<FistGunFired>(_onFistGunFired);
        EventManager.Instance.RemoveHandler<FistGunHit>(_onFistGunHit);
        EventManager.Instance.RemoveHandler<FistGunBlocked>(_onFistGunBlocked);
        EventManager.Instance.RemoveHandler<BazookaBombed>(_onBazookaBombed);
        EventManager.Instance.RemoveHandler<HookBlocked>(_onHookGunBlocked);
        EventManager.Instance.RemoveHandler<BazookaFired>(_onBazookaFired);
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.RemoveHandler<BagelSent>(_onFoodDelievered);
        EventManager.Instance.RemoveHandler<PlayerLand>(_onPlayerLand);
        EventManager.Instance.RemoveHandler<ObjectHitGround>(_onObjectHitGround);
        EventManager.Instance.RemoveHandler<AmmoExplode>(_onBagelExplode);
        EventManager.Instance.RemoveHandler<FistGunStartCharging>(_onFistGunReload);
    }

    public void Destroy()
    {
        OnDisable();
    }
}
