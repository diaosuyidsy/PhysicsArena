using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    private AudioData AudioDataStore;

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
        int rand = Random.Range(0, clips.Length);
        _playSound(obj, clips[rand], oneshot, volume);
    }

    #region Event Handlers
    private void _onPlayerHit(PlayerHit ph)
    {
        if (ph.Hiter == null)
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
        _playSound(pd.Player, AudioDataStore.DeathAudioClip);
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
        AudioClip ac = AudioDataStore.FootstepGrassAudioClip;
        switch (fs.GroundTag)
        {
            case "Ground_Concrete":
                ac = AudioDataStore.FootstepConcreteAudioClip;
                break;
            case "Ground_YellowStone":
                ac = AudioDataStore.FootstepYellowStoneAudioClip;
                break;
        }
        _playSound(fs.PlayerFeet, ac, true, Random.Range(0.2f, 0.3f));
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
        _playSound(ff.Fist, AudioDataStore.FistGunBlockedAudioClip, false);
    }

    private void _onHookGunBlocked(HookBlocked bh)
    {
        _playSound(bh.Hook, AudioDataStore.HookGunBlockedAudioClip);
    }

    private void _onBazookaFired(BazookaFired bf)
    {
        _playSound(bf.BazookaGun, AudioDataStore.BazookaFiredAudioClip);

    }

    private void _onBazookaBombed(BazookaBombed bb)
    {
        _playSound(bb.BazookaGun, AudioDataStore.BazookaBombedAudioClip);

    }

    private void _onObjectPickedUp(ObjectPickedUp opu)
    {
        if (opu.Obj.GetComponent<rtBirdFood>() != null && (opu.Player.tag.Contains("1") && opu.Player.tag.Contains("1")
        || (opu.Player.tag.Contains("2") && opu.Player.tag.Contains("2"))))
        {
            _playSound(opu.Obj, AudioDataStore.FoodPickedUpCorrectAudioClip);
        }
        else if (opu.Obj.GetComponent<rtBirdFood>() != null && (opu.Player.tag.Contains("1") && opu.Player.tag.Contains("2")
       || (opu.Player.tag.Contains("2") && opu.Player.tag.Contains("1"))))
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
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelievered);
        EventManager.Instance.AddHandler<PlayerLand>(_onPlayerLand);
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
        EventManager.Instance.RemoveHandler<BazookaBombed>(_onBazookaBombed);
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelievered);
        EventManager.Instance.RemoveHandler<PlayerLand>(_onPlayerLand);

    }

    public void Destroy()
    {
        OnDisable();
    }
}
