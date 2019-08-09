using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    public AudioData AudioDataStore;

    public AudioManager(AudioData _data)
    {
        AudioDataStore = _data;
        OnEnable();
    }

    /// <summary>
    /// Play clip Sound at obj
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="clip"></param>
    /// <param name="oneshot"></param>
    private void _playSound(GameObject obj, AudioClip clip, bool oneshot = true, float volume = 1)
    {
        if (clip == null) return;
        AudioSource objas = obj.GetComponent<AudioSource>();
        if (objas == null) objas = obj.AddComponent<AudioSource>();
        Debug.Assert(objas != null);

        if (oneshot)
            objas.PlayOneShot(clip, volume);
        else
        {
            objas.clip = clip;
            objas.Play();
        }
    }

    /// <summary>
    /// Play sound randomly from given clips
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="clips"></param>
    /// <param name="oneshot"></param>
    private void _playSound(GameObject obj, AudioClip[] clips, bool oneshot = true, float volume = 1)
    {
        if (clips.Length == 0) return;
        int rand = Random.Range(0, clips.Length);
        _playSound(obj, clips[rand], oneshot, volume);
    }

    #region Event Handlers
    private void _onPlayerHit(PlayerHit ph)
    {
        if (ph.IsABlock)
        {
            ///If hiter is null, then it's a block
            _playSound(ph.Hitted, AudioDataStore.BlockAudioClip);
        }
        else
        {
            if (ph.MeleeCharge > 0.1f)
                ///If it's not null, then it's a hit
                _playSound(ph.Hitted, AudioDataStore.PlayerHitAudioClip);
        }
    }

    private void _onHookGunFired(HookGunFired hgf)
    {
        _playSound(hgf.HookGun, AudioDataStore.HookGunFiredAudioClip);
    }

    private void _onHookGunHit(HookHit hh)
    {
        _playSound(hh.Hook, AudioDataStore.HookGunHitAudioClip);
    }

    private void _onSuckGunFired(SuckGunFired sgf)
    {
        _playSound(sgf.SuckGun, AudioDataStore.SuckGunFiredAudioClip);
    }

    private void _onSuckGunSuck(SuckGunSuck sgs)
    {
        _playSound(sgs.SuckBall, AudioDataStore.SuckGunSuckAudioClip);
    }

    private void _onPlayerDied(PlayerDied pd)
    {
        if (pd.Player.tag.Contains("Team1"))
            _playSound(pd.Player, AudioDataStore.ChickenDeathAudioClip);
        else
            _playSound(pd.Player, AudioDataStore.DuckDeathAudioClip);

    }

    private void _onObjectDespawned(ObjectDespawned od)
    {
        if (od.Despawner != null && od.Despawner.GetComponent<ResourceCollector>() != null)
        {
            _playSound(od.Obj, AudioDataStore.WrongFoodAudioClip);
        }
        else
            _playSound(od.Obj, AudioDataStore.ObjectDespawnedAudioClip);
    }

    private void _onPlayerRespawned(PlayerRespawned pr)
    {
        _playSound(pr.Player, AudioDataStore.PlayerRespawnedAudioClip);
    }

    private void _onWeaponSpawned(WeaponSpawned ws)
    {
        _playSound(ws.Weapon, AudioDataStore.WeaponSpawnedAudioClip);
    }

    private void _onPunchStart(PunchStart ph)
    {
        _playSound(ph.Player, AudioDataStore.PunchChargingAudioClip, false);
    }

    private void _onPunchReleased(PunchReleased pr)
    {
        _playSound(pr.Player, AudioDataStore.PunchReleasedAudioClip, false);
    }

    private void _onFootStep(FootStep fs)
    {
        switch (fs.GroundTag)
        {
            case "Ground_Concrete":
                _playSound(fs.PlayerFeet, AudioDataStore.FootstepConcreteAudioClip, true, Random.Range(0.2f, 0.3f));
                break;
            case "Ground_YellowStone":
                _playSound(fs.PlayerFeet, AudioDataStore.FootstepYellowStoneAudioClip, true, Random.Range(0.2f, 0.3f));
                break;
            case "Ground_Grass":
                _playSound(fs.PlayerFeet, AudioDataStore.FootstepGrassAudioClip, true, Random.Range(0.2f, 0.3f));
                break;
            default:
                _playSound(fs.PlayerFeet, AudioDataStore.FootstepConcreteAudioClip, true, Random.Range(0.2f, 0.3f));
                break;
        }
    }

    private void _onPlayerJump(PlayerJump pj)
    {
        _playSound(pj.PlayerFeet, AudioDataStore.JumpAudioClip);
    }

    private void _onWaterGunFired(WaterGunFired wf)
    {
        _playSound(wf.WaterGun, AudioDataStore.WaterGunFiredAudioClip);
    }

    private void _onFistGunFired(FistGunFired ff)
    {
        _playSound(ff.Fist, AudioDataStore.FistGunFiredAudioClip, false);
    }

    private void _onFistGunHit(FistGunHit ff)
    {
        _playSound(ff.Fist, AudioDataStore.FistGunHitAudioClip, false);
    }

    private void _onFistGunBlocked(FistGunBlocked ff)
    {
        _playSound(ff.Fist, AudioDataStore.FistGunBlockedAudioClip, true);
    }

    private void _onFistGunReload(FistGunStartCharging ev)
    {
        _playSound(ev.FistGun, AudioDataStore.FistGunChargeAudioClip, false);
    }

    private void _onHookGunBlocked(HookBlocked bh)
    {
        _playSound(bh.Hook, AudioDataStore.HookGunBlockedAudioClip);
    }

    private void _onBazookaFired(BazookaFired bf)
    {
        _playSound(bf.BazookaGun, AudioDataStore.BazookaFiredAudioClip);

    }

    private void _onObjectPickedUp(ObjectPickedUp opu)
    {
        if (opu.Obj.GetComponent<rtBirdFood>() != null && ((opu.Player.tag.Contains("1") && opu.Obj.tag.Contains("1"))
        || (opu.Player.tag.Contains("2") && opu.Obj.tag.Contains("2"))))
        {
            _playSound(opu.Obj, AudioDataStore.FoodPickedUpCorrectAudioClip);
        }
        else if (opu.Obj.GetComponent<rtBirdFood>() != null && ((opu.Player.tag.Contains("1") && opu.Obj.tag.Contains("2"))
       || (opu.Player.tag.Contains("2") && opu.Obj.tag.Contains("1"))))
        {
            _playSound(opu.Obj, AudioDataStore.FoodPickedUpWrongAudioClip);
        }
        else if (opu.Obj.GetComponent<WeaponBase>() != null)
        {
            _playSound(opu.Obj, AudioDataStore.WeaponPickedUpAudioClip);
        }
    }

    private void _onFoodDelievered(FoodDelivered fs)
    {
        _playSound(fs.Food, AudioDataStore.FoodDeliveredAudioClip);
    }

    private void _onPlayerLand(PlayerLand ev)
    {
        _playSound(ev.PlayerFeet, AudioDataStore.LandAudioClip);
    }

    private void _onObjectHitGround(ObjectHitGround ev)
    {
        if (ev.Obj.GetComponent<rtBazooka>() != null)
        {
            _playSound(ev.Obj, AudioDataStore.BazookaGunHitGroundAudioClip);
            return;
        }
        if (ev.Obj.GetComponent<rtBirdFood>() != null)
        {
            _playSound(ev.Obj, AudioDataStore.FoodHitGroundAudioClip);
            return;
        }
        if (ev.Obj.GetComponent<rtEmit>() != null)
        {
            _playSound(ev.Obj, AudioDataStore.WaterGunHitGroundAudioClip);
            return;
        }
        if (ev.Obj.GetComponent<rtFist>() != null)
        {
            _playSound(ev.Obj, AudioDataStore.FistGunHitGroundAudioClip);
            return;
        }
        if (ev.Obj.GetComponent<rtHook>() != null)
        {
            _playSound(ev.Obj, AudioDataStore.HookGunHitGroundAudioClip);
            return;
        }
        if (ev.Obj.GetComponent<rtSuck>() != null)
        {
            _playSound(ev.Obj, AudioDataStore.SuckGunHitGroundAudioClip);
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
        EventManager.Instance.AddHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelievered);
        EventManager.Instance.AddHandler<PlayerLand>(_onPlayerLand);
        EventManager.Instance.AddHandler<ObjectHitGround>(_onObjectHitGround);
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
        EventManager.Instance.RemoveHandler<HookBlocked>(_onHookGunBlocked);
        EventManager.Instance.RemoveHandler<BazookaFired>(_onBazookaFired);
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelievered);
        EventManager.Instance.RemoveHandler<PlayerLand>(_onPlayerLand);
        EventManager.Instance.RemoveHandler<ObjectHitGround>(_onObjectHitGround);
        EventManager.Instance.RemoveHandler<FistGunStartCharging>(_onFistGunReload);
    }

    public void Destroy()
    {
        OnDisable();
    }
}
