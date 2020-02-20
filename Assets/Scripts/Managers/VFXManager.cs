﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager
{
    public VFXData VFXDataStore;
    private float _blinkTime = 3f;
    private float _blinkDeltaTime = 10f;
    private Transform _mainCameraTransform;

    public VFXManager(VFXData _vfxdata)
    {
        VFXDataStore = _vfxdata;
        _mainCameraTransform = Camera.main.transform;
        OnEnable();
    }

    #region Event Handlers
    private void _onPlayerHit(PlayerHit ph)
    {
        if (Utility.PlayerWillDieOnHit(ph, Services.Config.CharacterData, VFXDataStore.HitBlockedLayer))
        {
            GameObject[] CameraVFX = ph.Hiter.CompareTag("Team1") ? VFXDataStore.ChickenHittedCameraVFX : VFXDataStore.DuckHittedCameraVFX;
            _instantiateVFX(CameraVFX, _mainCameraTransform);
        }
        if (VFXDataStore.HitVFX != null)
        {
            for (int i = 0; i < VFXDataStore.HitVFX.Length; i++)
            {
                GameObject VFX = VFXDataStore.HitVFX[i];
                Vector3 hittedPos = ph.Hiter.transform.position +
                                    ph.Hiter.transform.forward * VFX.transform.position.z +
                                    ph.Hiter.transform.right * VFX.transform.position.x +
                                    ph.Hiter.transform.up * VFX.transform.position.y;
                Vector3 force = ph.Force;
                _instantiateVFX(VFXDataStore.HitVFX[i],
                                hittedPos,
                                Quaternion.Euler(0f,
                                                VFXDataStore.HitVFX[i].transform.eulerAngles.y + Vector3.SignedAngle(Vector3.forward, new Vector3(force.x, 0f, force.z), Vector3.up),
                                                0f));
            }
        }
        GameObject[] HittedFeetVFX = ph.Hiter.CompareTag("Team1") ? VFXDataStore.ChickenHittedFeetVFX : VFXDataStore.DuckHittedFeetVFX;
        _instantiateVFX(HittedFeetVFX, ph.Hitted.GetComponent<PlayerController>().PlayerUITransform);
        GameObject[] HittedBodyVFX = ph.Hiter.CompareTag("Team1") ? VFXDataStore.ChickenHittedBodyVFX : VFXDataStore.DuckHittedBodyVFX;
        if (HittedBodyVFX != null)
        {
            for (int i = 0; i < HittedBodyVFX.Length; i++)
            {
                GameObject VFX = HittedBodyVFX[i];
                Vector3 hittedPos = ph.Hitted.transform.position +
                                    ph.Hitted.transform.forward * VFX.transform.position.z +
                                    ph.Hitted.transform.right * VFX.transform.position.x +
                                    ph.Hitted.transform.up * VFX.transform.position.y;
                Vector3 force = ph.Force;
                _instantiateVFX(VFX,
                                hittedPos,
                                Quaternion.Euler(0f,
                                                VFX.transform.eulerAngles.y + Vector3.SignedAngle(Vector3.forward, new Vector3(force.x, 0f, force.z), Vector3.up),
                                                0f));
            }
        }

    }

    private void _onPlayerDied(PlayerDied pd)
    {
        if (pd.Player.layer < 13)
            _instantiateVFX(VFXDataStore.DeathVFX[pd.Player.layer - 9], pd.Player.transform.position, VFXDataStore.DeathVFX[pd.Player.layer - 9].transform.rotation);
        else
            _instantiateVFX(VFXDataStore.DeathVFX[pd.Player.layer - 11], pd.Player.transform.position, VFXDataStore.DeathVFX[pd.Player.layer - 11].transform.rotation);

        if (pd.Player.layer < 13)
            _instantiateVFX(VFXDataStore.HugeDeathVFX[pd.Player.layer - 9], pd.Player.transform.position, VFXDataStore.HugeDeathVFX[pd.Player.layer - 9].transform.rotation);
        else
            _instantiateVFX(VFXDataStore.HugeDeathVFX[pd.Player.layer - 11], pd.Player.transform.position, VFXDataStore.HugeDeathVFX[pd.Player.layer - 11].transform.rotation);

    }

    private void _onObjectDespawned(ObjectDespawned od)
    {
        _instantiateVFX(VFXDataStore.VanishVFX, od.Obj.transform.position, VFXDataStore.VanishVFX.transform.rotation);
    }

    private void _onFoodDelivered(FoodDelivered fd)
    {
        _instantiateVFX(VFXDataStore.DeliverFoodVFX, fd.Food.transform.position, VFXDataStore.DeliverFoodVFX.transform.rotation);
    }

    private void _onFootStep(FootStep ev)
    {
        GameObject VFX = ev.Player.CompareTag("Team1") ?
        (ev.PlayerFootLeftRight == 0 ? VFXDataStore.ChickenRightFootStepVFX : VFXDataStore.ChickenLeftFootStepVFX) :
        (ev.PlayerFootLeftRight == 0 ? VFXDataStore.DuckRightFootStepVFX : VFXDataStore.DuckLeftFootStepVFX);
        _instantiateVFX(VFX, ev.PlayerActualFoot.transform.position, Quaternion.Euler(VFX.transform.eulerAngles.x, ev.Player.transform.eulerAngles.y, ev.Player.transform.eulerAngles.z));
    }

    private void _onPlayerJump(PlayerJump pj)
    {
        var VFX = VFXDataStore.JumpGrassVFX;
        switch (pj.GroundTag)
        {
            case "Ground_Concrete":
                VFX = VFXDataStore.JumpConcreteVFX;
                break;
            case "Ground_YellowStone":
                VFX = VFXDataStore.JumpYellowStoneVFX;
                break;
        }
        _instantiateVFX(VFX, pj.PlayerFeet.transform.position, VFX.transform.rotation);
    }

    private void _onPlayerLand(PlayerLand pl)
    {
        var VFX = VFXDataStore.LandGrassVFX;
        switch (pl.GroundTag)
        {
            case "Ground_Concrete":
                VFX = VFXDataStore.LandConcreteVFX;
                break;
            case "Ground_YellowStone":
                VFX = VFXDataStore.LandYellowStoneVFX;
                break;
        }
        _instantiateVFX(VFX, pl.PlayerFeet.transform.position, VFX.transform.rotation);
    }

    private void _onBlockStart(BlockStart bs)
    {
        GameObject VFX = bs.Player.CompareTag("Team1") ? VFXDataStore.ChickenBlockVFX : VFXDataStore.DuckBlockVFX;
        // GameObject UIVFX = bs.Player.CompareTag("Team1") ? VFXDataStore.ChickenBlockUIVFX : VFXDataStore.DuckBlockUIVFX;
        GameObject BlockVFXHolder = bs.Player.GetComponent<PlayerController>().BlockVFXHolder;
        if (BlockVFXHolder == null)
        {
            bs.Player.GetComponent<PlayerController>().BlockVFXHolder = GameObject.Instantiate(VFX, bs.Player.transform);
        }
        // if (bs.Player.GetComponent<PlayerController>().BlockUIVFXHolder == null)
        // {
        //     bs.Player.GetComponent<PlayerController>().BlockUIVFXHolder = GameObject.Instantiate(UIVFX, bs.Player.transform);
        // }
        bs.Player.GetComponent<PlayerController>().BlockVFXHolder.SetActive(true);
        // bs.Player.GetComponent<PlayerController>().BlockUIVFXHolder.SetActive(true);
    }

    private void _onBlockEnd(BlockEnd be)
    {
        be.Player.GetComponent<PlayerController>().BlockVFXHolder.SetActive(false);
        // be.Player.GetComponent<PlayerController>().BlockUIVFXHolder.SetActive(false);
    }

    private void _onPunchStart(PunchStart ps)
    {
        GameObject VFX = ps.Player.CompareTag("Team1") ? VFXDataStore.ChickenMeleeChargingVFX : VFXDataStore.DuckMeleeChargingVFX;
        GameObject MeleeVFXHolder = ps.Player.GetComponent<PlayerController>().MeleeVFXHolder2;
        if (MeleeVFXHolder != null) GameObject.Destroy(MeleeVFXHolder);
        ps.Player.GetComponent<PlayerController>().MeleeVFXHolder2 = GameObject.Instantiate(VFX, ps.PlayerRightHand, false);

    }

    private void _onPunchHolding(PunchHolding ph)
    {
        GameObject VFX = ph.Player.CompareTag("Team1") ? VFXDataStore.ChickenUltimateVFX : VFXDataStore.DuckUltimateVFX;
        GameObject MeleeVFXHolder = ph.Player.GetComponent<PlayerController>().MeleeVFXHolder;
        if (MeleeVFXHolder != null) GameObject.Destroy(MeleeVFXHolder);
        ph.Player.GetComponent<PlayerController>().MeleeVFXHolder = GameObject.Instantiate(VFX, ph.PlayerRightHand, false);
    }

    private void _onPunchReleased(PunchReleased pr)
    {
        GameObject MeleeVFXHolder2 = pr.Player.GetComponent<PlayerController>().MeleeVFXHolder2;
        if (MeleeVFXHolder2 != null) GameObject.Destroy(MeleeVFXHolder2);
        if (pr.Player.CompareTag("Team1"))
        {
            _instantiateVFX(VFXDataStore.ChickenReleasePunchHandVFX, pr.Player.GetComponent<PlayerController>().RightHand.transform);
            _instantiateVFX(VFXDataStore.ChickenReleasePunchFootVFX, pr.Player.GetComponent<PlayerController>().PlayerFeet);
        }
        else
        {
            _instantiateVFX(VFXDataStore.DuckReleasePunchHandVFX, pr.Player.GetComponent<PlayerController>().RightHand.transform);
            _instantiateVFX(VFXDataStore.DuckReleasePunchFootVFX, pr.Player.GetComponent<PlayerController>().PlayerFeet);
        }
        GameObject[] ReleaseBodyVFX = pr.Player.CompareTag("Team1") ? VFXDataStore.ChickenReleaseBodyVFX : VFXDataStore.DuckReleaseBodyVFX;
        for (int i = 0; i < ReleaseBodyVFX.Length; i++)
        {
            GameObject VFX = ReleaseBodyVFX[i];
            Vector3 hittedPos = pr.Player.transform.position +
                                   pr.Player.transform.forward * VFX.transform.position.z +
                                   pr.Player.transform.right * VFX.transform.position.x +
                                   pr.Player.transform.up * VFX.transform.position.y;
            _instantiateVFX(VFX,
                    hittedPos,
                    Quaternion.Euler(0f,
                                    VFX.transform.eulerAngles.y + pr.Player.transform.eulerAngles.y,
                                    0f));
        }
    }

    private void _onPunchDone(PunchDone pd)
    {
        GameObject MeleeVFXHolder = pd.Player.GetComponent<PlayerController>().MeleeVFXHolder;
        GameObject MeleeVFXHolder2 = pd.Player.GetComponent<PlayerController>().MeleeVFXHolder2;
        if (MeleeVFXHolder != null) GameObject.Destroy(MeleeVFXHolder);
        if (MeleeVFXHolder2 != null) GameObject.Destroy(MeleeVFXHolder2);
    }

    private void _onBazookaBombed(BazookaBombed bb)
    {
        _instantiateVFX(VFXDataStore.BazookaExplosionVFX, bb.BazookaGun.transform.position, VFXDataStore.BazookaExplosionVFX.transform.rotation);
        if (bb.BazookaGun.GetComponent<rtBazooka>() != null &&
            bb.BazookaGun.GetComponent<rtBazooka>().BazookaTrailVFXHolder != null)
            bb.BazookaGun.GetComponent<rtBazooka>().BazookaTrailVFXHolder.SetActive(false);
    }

    private void _onPlayerStunned(PlayerStunned ps)
    {
        GameObject VFX = ps.Player.CompareTag("Team1") ? VFXDataStore.ChickenStunnedVFX : VFXDataStore.DuckStunnedVFX;
        if (ps.Player.GetComponent<PlayerController>().StunVFXHolder == null)
            ps.Player.GetComponent<PlayerController>().StunVFXHolder = GameObject.Instantiate(VFX, ps.PlayerHead, false);
        ps.Player.GetComponent<PlayerController>().StunVFXHolder.SetActive(true);
    }

    private void _onPlayerUnStunned(PlayerUnStunned ps)
    {
        if (ps.Player.GetComponent<PlayerController>().StunVFXHolder != null)
            ps.Player.GetComponent<PlayerController>().StunVFXHolder.SetActive(false);
    }

    private void _onPlayerSlowed(PlayerSlowed ps)
    {
        GameObject VFX = ps.Player.CompareTag("Team1") ? VFXDataStore.ChickenSlowedVFX : VFXDataStore.DuckSlowedVFX;
        if (ps.Player.GetComponent<PlayerController>().SlowVFXHolder == null)
        {
            ps.Player.GetComponent<PlayerController>().SlowVFXHolder = GameObject.Instantiate(VFX, ps.PlayerFeet.transform, false);
            ps.Player.GetComponent<PlayerController>().SlowVFXHolder.transform.rotation = VFX.transform.rotation;
        }
        ps.Player.GetComponent<PlayerController>().SlowVFXHolder.SetActive(true);
    }

    private void _onPlayerUnslowed(PlayerUnslowed pu)
    {
        pu.Player.GetComponent<PlayerController>().SlowVFXHolder.SetActive(false);
    }
    private void _onGameEnd(GameEnd ge)
    {
        if (ge.GameWinType == GameWinType.CartWin)
        {
            _instantiateVFX(VFXDataStore.CartExplosionVFX, ge.WinnedObjective.position, VFXDataStore.CartExplosionVFX.transform.rotation);
        }
    }

    private void _onObjectPickedUp(ObjectPickedUp opu)
    {
        rtBirdFood bf = opu.Obj.GetComponent<rtBirdFood>();
        if (bf != null)
        {
            if ((opu.Obj.tag.Contains("Team1") && opu.Player.tag.Contains("Team2")) || (opu.Obj.tag.Contains("Team2") && opu.Player.tag.Contains("Team1")))
                return;
            GameObject VFX = opu.Obj.tag.Contains("Team1") ? VFXDataStore.ChickenFoodVFX : VFXDataStore.DuckFoodVFX;
            if (bf.PickUpVFXHolder == null)
            {
                bf.PickUpVFXHolder = GameObject.Instantiate(VFX, opu.Obj.transform, false);
                bf.PickUpVFXHolder.transform.rotation = VFX.transform.rotation;
            }
            bf.PickUpVFXHolder.SetActive(true);
            GameObject FoodGuideVFX = opu.Obj.tag.Contains("Team1") ? VFXDataStore.ChickenFoodGuideVFX : VFXDataStore.DuckFoodGuideVFX;
            PlayerController pc = opu.Player.GetComponent<PlayerController>();
            if (pc != null && pc.FoodTraverseVFXHolder == null)
            {
                pc.FoodTraverseVFXHolder = GameObject.Instantiate(FoodGuideVFX, pc.PlayerFeet, false);
                pc.FoodTraverseVFXHolder.transform.rotation = FoodGuideVFX.transform.rotation;
            }
            pc.FoodTraverseVFXHolder.SetActive(true);
        }
    }

    private void _onObjectDropped(ObjectDropped od)
    {
        rtBirdFood bf = od.Obj.GetComponent<rtBirdFood>();
        if (bf != null && bf.PickUpVFXHolder != null)
        {
            bf.PickUpVFXHolder.SetActive(false);
        }
        PlayerController pc = od.Player.GetComponent<PlayerController>();
        if (pc != null && pc.FoodTraverseVFXHolder != null)
        {
            pc.FoodTraverseVFXHolder.SetActive(false);
        }
    }

    private void _onFistGunFire(FistGunFired ev)
    {
        GameObject.Instantiate(VFXDataStore.FistGunFistTrailVFX, ev.Fist.transform, false);
    }

    private void _onFistGunRecharged(FistGunCharged ev)
    {
        GameObject.Instantiate(VFXDataStore.VanishVFX, ev.FistPos, VFXDataStore.VanishVFX.transform.rotation);
    }
    private void _onBazookaLaunched(BazookaFired ev)
    {
        // GameObject.Instantiate(VFXDataStore.BazookaStartVFX, ev.FistPos, VFXDataStore.BazookaStartVFX.transform.rotation);
        rtBazooka rb = ev.BazookaGun.GetComponent<rtBazooka>();
        if (rb.BazookaTrailVFXHolder == null)
        {
            rb.BazookaTrailVFXHolder = GameObject.Instantiate(VFXDataStore.BazookaTrailVFX, ev.BazookaGun.transform, false);
            rb.BazookaTrailVFXHolder.transform.rotation = VFXDataStore.BazookaTrailVFX.transform.rotation;
        }
        if (rb.BazookaTrailVFXHolder != null)
            rb.BazookaTrailVFXHolder.SetActive(true);
    }

    private void _onBlocked(Blocked ev)
    {
        GameObject parryvfx = ev.Blocker.tag.Contains("Team1") ? VFXDataStore.ChickenBlockParryVFX : VFXDataStore.DuckBlockParryVFX;
        GameObject startvfx = ev.Blocker.tag.Contains("Team1") ? VFXDataStore.ChickenBlockShieldStarVFX : VFXDataStore.DuckBlockShieldStarVFX;
        PlayerController pc = ev.Blocker.GetComponent<PlayerController>();
        GameObject.Instantiate(parryvfx, pc.transform);
        GameObject.Instantiate(parryvfx, pc.BlockVFXHolder.transform);
    }

    private void _onPlayerRespawned(PlayerRespawned ev)
    {
        foreach (Transform child in ev.Player.transform)
        {
            Renderer r = child.gameObject.GetComponent<Renderer>();

            if (r != null && r.material.name.Contains("CustomComic"))
            {
                ev.Player.GetComponent<PlayerController>().StartCoroutine(_startBlinking(_blinkTime, r));
            }
        }
    }
    IEnumerator _startBlinking(float time, Renderer r)
    {
        float curTime = 0;
        float deltaTime = _blinkDeltaTime * Time.deltaTime;
        while (curTime < time)
        {
            if (r.enabled)
            {
                r.enabled = false;
            }
            else
            {
                r.enabled = true;
            }

            curTime += deltaTime;
            yield return new WaitForSeconds(deltaTime);
        }

        r.enabled = true;
    }

    #endregion

    private GameObject _instantiateVFX(GameObject _vfx, Vector3 _pos, Quaternion _rot)
    {
        if (_vfx == null) return null;
        return GameObject.Instantiate(_vfx, _pos, _rot);
    }
    private GameObject[] _instantiateVFX(GameObject[] _vfx, Vector3 _pos, Quaternion _rot)
    {
        if (_vfx.Length == 0) return null;
        GameObject[] result = new GameObject[_vfx.Length];
        for (int i = 0; i < _vfx.Length; i++)
        {
            result[i] = GameObject.Instantiate(_vfx[i], _pos, _rot);
        }
        return result;
    }

    private GameObject[] _instantiateVFX(GameObject[] _vfx, Transform parent)
    {
        if (_vfx.Length == 0) return null;
        GameObject[] result = new GameObject[_vfx.Length];
        for (int i = 0; i < _vfx.Length; i++)
        {
            result[i] = GameObject.Instantiate(_vfx[i], parent);
        }
        return result;
    }
    private void OnEnable()
    {
        EventManager.Instance.AddHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<ObjectDespawned>(_onObjectDespawned);
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelivered);
        EventManager.Instance.AddHandler<PlayerJump>(_onPlayerJump);
        EventManager.Instance.AddHandler<PlayerLand>(_onPlayerLand);
        EventManager.Instance.AddHandler<PunchStart>(_onPunchStart);
        EventManager.Instance.AddHandler<PunchHolding>(_onPunchHolding);
        EventManager.Instance.AddHandler<PunchReleased>(_onPunchReleased);
        EventManager.Instance.AddHandler<PunchDone>(_onPunchDone);
        EventManager.Instance.AddHandler<BazookaBombed>(_onBazookaBombed);
        EventManager.Instance.AddHandler<BlockStart>(_onBlockStart);
        EventManager.Instance.AddHandler<BlockEnd>(_onBlockEnd);
        EventManager.Instance.AddHandler<PlayerStunned>(_onPlayerStunned);
        EventManager.Instance.AddHandler<PlayerUnStunned>(_onPlayerUnStunned);
        EventManager.Instance.AddHandler<PlayerSlowed>(_onPlayerSlowed);
        EventManager.Instance.AddHandler<PlayerUnslowed>(_onPlayerUnslowed);
        EventManager.Instance.AddHandler<GameEnd>(_onGameEnd);
        EventManager.Instance.AddHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.AddHandler<ObjectDropped>(_onObjectDropped);
        EventManager.Instance.AddHandler<FistGunFired>(_onFistGunFire);
        EventManager.Instance.AddHandler<FistGunCharged>(_onFistGunRecharged);
        EventManager.Instance.AddHandler<BazookaFired>(_onBazookaLaunched);
        EventManager.Instance.AddHandler<Blocked>(_onBlocked);
        EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawned);
        EventManager.Instance.AddHandler<FootStep>(_onFootStep);
        _blinkTime = Services.Config.GameMapData.InvincibleTime;
    }


    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<PlayerHit>(_onPlayerHit);
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<ObjectDespawned>(_onObjectDespawned);
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
        EventManager.Instance.RemoveHandler<PlayerJump>(_onPlayerJump);
        EventManager.Instance.RemoveHandler<PlayerLand>(_onPlayerLand);
        EventManager.Instance.RemoveHandler<PunchStart>(_onPunchStart);
        EventManager.Instance.RemoveHandler<PunchHolding>(_onPunchHolding);
        EventManager.Instance.RemoveHandler<PunchReleased>(_onPunchReleased);
        EventManager.Instance.RemoveHandler<PunchDone>(_onPunchDone);
        EventManager.Instance.RemoveHandler<BazookaBombed>(_onBazookaBombed);
        EventManager.Instance.RemoveHandler<BlockStart>(_onBlockStart);
        EventManager.Instance.RemoveHandler<BlockEnd>(_onBlockEnd);
        EventManager.Instance.RemoveHandler<PlayerStunned>(_onPlayerStunned);
        EventManager.Instance.RemoveHandler<PlayerUnStunned>(_onPlayerUnStunned);
        EventManager.Instance.RemoveHandler<PlayerSlowed>(_onPlayerSlowed);
        EventManager.Instance.RemoveHandler<PlayerUnslowed>(_onPlayerUnslowed);
        EventManager.Instance.RemoveHandler<GameEnd>(_onGameEnd);
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.RemoveHandler<ObjectDropped>(_onObjectDropped);
        EventManager.Instance.RemoveHandler<FistGunFired>(_onFistGunFire);
        EventManager.Instance.RemoveHandler<FistGunCharged>(_onFistGunRecharged);
        EventManager.Instance.RemoveHandler<BazookaFired>(_onBazookaLaunched);
        EventManager.Instance.RemoveHandler<Blocked>(_onBlocked);
        EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawned);
        EventManager.Instance.RemoveHandler<FootStep>(_onFootStep);
    }

    public void Destory()
    {
        OnDisable();
    }
}
