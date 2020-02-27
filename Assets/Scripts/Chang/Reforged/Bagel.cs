using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bagel : WeaponBase
{
    public VFXData Data;

    public GameObject Entity;
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

        Team1Basket = BrawlModeReforgedArenaManager.Team1Basket;
        Team2Basket = BrawlModeReforgedArenaManager.Team2Basket;
    }

    protected override void Update()
    {
        base.Update();
        if (Hold)
        {
            if (Guide== null)
            {
                GameObject FoodGuideVFX = Owner.tag.Contains("1") ? Data.ChickenFoodGuideVFX : Data.DuckFoodGuideVFX;

                PlayerController pc = Owner.GetComponent<PlayerController>();
                Guide = GameObject.Instantiate(FoodGuideVFX, pc.PlayerFeet, false);
                pc.FoodTraverseVFXHolder = Guide;
                pc.FoodTraverseVFXHolder.transform.rotation = FoodGuideVFX.transform.rotation;
                pc.FoodTraverseVFXHolder.SetActive(true);
            }
            else
            {
                Vector3 Team1BasketOffset = Team1Basket.transform.position - Guide.transform.position;
                Vector3 Team2BasketOffset = Team2Basket.transform.position - Guide.transform.position;
                Team1BasketOffset.y = 0;
                Team2BasketOffset.y = 0;

                Guide.transform.forward = Owner.tag.Contains("1") ? Team1BasketOffset : Team2BasketOffset;
            }


        }
    }

    public override void OnPickUp(GameObject owner)
    {
        base.OnPickUp(owner);

        Material mat = Entity.GetComponent<Renderer>().material;
        mat.EnableKeyword("_EMISSION");
        

        if (owner.tag.Contains("1"))
        {
            //mat.SetColor("_Color", Red);
            //mat.SetColor("_EmissionColor", Red * 0.5f);
        }
        else
        {
            //mat.SetColor("_Color", Blue);
            //mat.SetColor("_EmissionColor", Blue * 0.5f);
        }

        Hold = true;
    }

    public override void OnDrop()
    {
        base.OnDrop();

        Material mat = Entity.GetComponent<Renderer>().material;
        mat.EnableKeyword("_EMISSION");

        //mat.SetColor("_Color", Default);
        //mat.SetColor("_EmissionColor", Default * 0.5f);

        Hold = false;
        Destroy(Guide);
        
    }

    public override void Fire(bool buttondown)
    {
        if (buttondown)
        {
            Owner.GetComponent<PlayerController>().FireBirdFood();
            GetComponent<Rigidbody>().AddForce(Owner.transform.forward * 2 + Owner.transform.up * 2,ForceMode.VelocityChange);
        }
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
        if (other.CompareTag("DeathZone"))
        {
            _hitGroundOnce = false;
            EventManager.Instance.TriggerEvent(new ObjectDespawned(gameObject));
            EventManager.Instance.TriggerEvent(new BagelDespawn());
            return;
        }
        /*if (other.tag.Contains("Collector"))
        {
            EventManager.Instance.TriggerEvent(new BagelSent(other.transform.parent.gameObject));
            Destroy(gameObject);
        }*/
    }
}
