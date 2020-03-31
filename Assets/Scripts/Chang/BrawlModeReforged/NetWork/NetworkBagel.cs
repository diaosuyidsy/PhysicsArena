using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkBagel : NetworkWeaponBase
{
    public VFXData Data;

    //public GameObject Entity;
    public Color Default;
    public Color Blue;
    public Color Red;

    private GameObject Team1Basket;
    private GameObject Team2Basket;

    public bool Hold;

    private GameObject Guide;

    protected override void Awake()
    {
        base.Awake();
        _hitGroundOnce = true;

        Team1Basket = NetworkBrawlModeReforgedArenaManager.Team1Basket;
        Team2Basket = NetworkBrawlModeReforgedArenaManager.Team2Basket;
    }

    protected override void Update()
    {

        base.Update();
        if (isServer)
        {
            SetGuide();
        }
        
    }

    private void SetGuide()
    {
        if (Hold) // Show guide UI
        {
            if (Guide == null)
            {
                GameObject FoodGuideVFX = Owner.tag.Contains("1") ? Data.ChickenFoodGuideVFX : Data.DuckFoodGuideVFX;

                PlayerControllerMirror pc = Owner.GetComponent<PlayerControllerMirror>();

                Guide = GameObject.Instantiate(FoodGuideVFX, pc.PlayerFeet, false);

                pc.FoodTraverseVFXHolder = Guide;
                pc.FoodTraverseVFXHolder.transform.rotation = FoodGuideVFX.transform.rotation;
                pc.FoodTraverseVFXHolder.SetActive(true);

                NetworkServer.Spawn(Guide);

                RpcActivateVFXHolder(Owner,Guide);
            }
            else
            {
                Vector3 Team1BasketOffset = Team1Basket.transform.position - Guide.transform.position;
                Vector3 Team2BasketOffset = Team2Basket.transform.position - Guide.transform.position;
                Team1BasketOffset.y = 0;
                Team2BasketOffset.y = 0;

                Guide.transform.forward = Owner.tag.Contains("1") ? Team1BasketOffset : Team2BasketOffset;

                RpcSetGuideTransform();


            }
        }
    }


    [ClientRpc]
    private void RpcActivateVFXHolder(GameObject Owner,GameObject Guide)
    {
        GameObject FoodGuideVFX = Owner.tag.Contains("1") ? Data.ChickenFoodGuideVFX : Data.DuckFoodGuideVFX;

        PlayerControllerMirror pc = Owner.GetComponent<PlayerControllerMirror>();

        Guide.transform.parent = pc.PlayerFeet;
        Guide.transform.localPosition = Vector3.zero;
        Guide.transform.localScale = Vector3.one * 15;


        pc.FoodTraverseVFXHolder = Guide;
        pc.FoodTraverseVFXHolder.transform.rotation = FoodGuideVFX.transform.rotation;
        pc.FoodTraverseVFXHolder.SetActive(true);
    }

    [ClientRpc]
    private void RpcSetGuideTransform()
    {
        if (Guide != null)
        {
            Vector3 Team1BasketOffset = Team1Basket.transform.position - Guide.transform.position;
            Vector3 Team2BasketOffset = Team2Basket.transform.position - Guide.transform.position;
            Team1BasketOffset.y = 0;
            Team2BasketOffset.y = 0;

            Guide.transform.forward = Owner.tag.Contains("1") ? Team1BasketOffset : Team2BasketOffset;
        }
    }

    public override void OnPickUp(GameObject owner)
    {
        base.OnPickUp(owner);

        Hold = true;

        RpcSetHold(true);
    }

    public override void OnDrop()
    {
        base.OnDrop();

        Hold = false;

        NetworkServer.UnSpawn(Guide);
        Destroy(Guide);

        RpcSetHold(false);

    }


    [ClientRpc]
    private void RpcSetHold(bool b)
    {
        Hold = b;
    }

    public override void Fire(bool buttondown)
    {
        if (buttondown)
        {
            NetworkServer.UnSpawn(Guide);
            Destroy(Guide);

            Owner.GetComponent<PlayerControllerMirror>().ForceDropHandObject();
            RpcFire();


        }
    }

    [ClientRpc]
    private void RpcFire()
    {
        Owner.GetComponent<PlayerControllerMirror>().ForceDropHandObject();
    }

    public void OnSucked()
    {
        gameObject.layer = 2;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    protected override void _onWeaponDespawn()
    {

    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!isServer)
        {
            return;
        }

        if (other.CompareTag("DeathZone"))
        {
            _hitGroundOnce = false;
            EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
            EventManager.Instance.TriggerEvent(new BagelDespawn());

            NetworkServer.UnSpawn(gameObject);
            Destroy(gameObject);


            return;
        }
    }
}
