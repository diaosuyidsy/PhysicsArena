using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bagel : WeaponBase
{
    public VFXData Data;

    public GameObject Entity;
    public GameObject Effect;

    public GameObject RedPointerPrefab;
    public GameObject BluePointerPrefab;


    public float PointerDis;

    public float PointerFarScale;
    public float PointerCloseScale;
    public float PointerFarDis;
    public float PointerCloseDis;
    public float PercentChangeSpeed;

    public float OutScreenClamp;

    public float WorldPointerScale;

    public float GuideHopInterval;
    public float GuideHopDis;
    public float GuideHopTime;

    private GameObject Team1Basket;
    private GameObject Team2Basket;

    private bool Hold;

    private GameObject Guide;
    private GameObject GameUI;
    private GameObject Pointer;
    private GameObject WorldPointer;

    private GameObject HoleEffect;

    private float GuideOffset;

    private float GuideHopTimer;

    private float PointerPercent;
    private float WorldPointerPercent;

    private GameObject LastOwner;


    protected override void Awake()
    {
        base.Awake();
        _hitGroundOnce = true;

        _ammo = 1;

        Team1Basket = BrawlModeReforgedArenaManager.Team1Basket;
        Team2Basket = BrawlModeReforgedArenaManager.Team2Basket;

        GameUI = GameObject.Find("GameUI").gameObject;
    }

    protected override void Update()
    {
        base.Update();
        SetGuide();
        GuideHop();
        SetPointer();

    }

    public bool GetHold()
    {
        return Hold;
    }

    private void SetPointer()
    {
        if (Hold)
        {
            GameObject Basket = Owner.tag.Contains("1") ? Team1Basket : Team2Basket;

            WorldPointer = Basket.transform.Find("ArrowMark").gameObject;

            Vector3 Offset = Basket.transform.position - Owner.transform.position;

            Offset.y = 0;

            float Angle = Vector3.SignedAngle(Vector3.forward, -Offset, Vector3.up);

            Vector3 ContactPoint;

            float Size = Basket.GetComponent<CabelBasket>().Size/2;

            if (Mathf.Abs(Angle) <= 45)
            {
                ContactPoint = Basket.transform.position + Vector3.forward * Size + Vector3.right * Size * Mathf.Tan(Angle * Mathf.Deg2Rad);
            }
            else if(Angle<=135 && Angle > 45)
            {
                ContactPoint = Basket.transform.position + Vector3.right * Size + Vector3.back* Mathf.Tan((Angle-90) * Mathf.Deg2Rad) * Size;
            }
            else if(Angle >= -135 && Angle < -45)
            {
                ContactPoint = Basket.transform.position + Vector3.left * Size + Vector3.forward * Mathf.Tan((Angle+90) * Mathf.Deg2Rad) * Size;
            }
            else
            {
                ContactPoint = Basket.transform.position + Vector3.back * Size + Vector3.left * Size * Mathf.Tan(Angle * Mathf.Deg2Rad);
            }

            Vector3 ScreenPos = Camera.main.WorldToScreenPoint(ContactPoint);

            ScreenPos = Camera.main.WorldToScreenPoint(Basket.transform.position);

            Vector3 OwnerScreenPos = Camera.main.WorldToScreenPoint(Owner.transform.position);

            ScreenPos.z = 0;
            OwnerScreenPos.z = 0;

            if(Pointer == null)
            {
                Pointer = Owner.tag.Contains("1") ? GameObject.Instantiate(RedPointerPrefab,GameUI.transform) : GameObject.Instantiate(BluePointerPrefab, GameUI.transform);
            }



            Pointer.transform.right = (ScreenPos - OwnerScreenPos).normalized;


            float t = Mathf.Clamp((Offset.magnitude - PointerCloseDis) / (PointerFarDis - PointerCloseDis),0,1);


            float Scale = Mathf.Lerp(PointerCloseScale, PointerFarScale, t);

            Pointer.transform.localScale = new Vector3(Scale/GameUI.transform.localScale.x, Scale / GameUI.transform.localScale.y, Scale / GameUI.transform.localScale.z);


            if (ScreenPos.x <= Screen.width && ScreenPos.x >=0  && ScreenPos.y<=Screen.height && ScreenPos.y >=0)
            {

                PointerPercent -= PercentChangeSpeed * Time.deltaTime;
                if (PointerPercent < 0)
                {
                    PointerPercent = 0;
                    WorldPointerPercent += PercentChangeSpeed * Time.deltaTime;

                    if(WorldPointerPercent > 1)
                    {
                        WorldPointerPercent = 1;
                    }

                    WorldPointer.SetActive(true);
                }

                Pointer.transform.localScale *= PointerPercent;

                Pointer.transform.position = ScreenPos;
                Pointer.transform.position -= Pointer.transform.right * PointerDis * Scale * PointerPercent;

                WorldPointer.transform.localScale = WorldPointerScale * Vector3.one * WorldPointerPercent;


                //Pointer.SetActive(false);
            }
            else
            {
                float k = (ScreenPos.y - OwnerScreenPos.y) / (ScreenPos.x - OwnerScreenPos.x);
                float b = ScreenPos.y - k * ScreenPos.x;


                float y =0;
                float x =0;

                if(ScreenPos.y < OwnerScreenPos.y)
                {
                    if(-b/k >=0 && -b/k <= Screen.width)
                    {
                        y = 0;
                        x = -b / k;
                        if (k < 0)
                        {
                            x = Mathf.Clamp(x,Screen.width * (1 - OutScreenClamp), Screen.width);
                        }
                        else
                        {
                            x = Mathf.Clamp(x, 0, Screen.width * OutScreenClamp);
                        }
                    }
                }
                else
                {
                    if ((Screen.height - b) / k >= 0 && (Screen.height - b) / k <= Screen.width)
                    {
                        y = Screen.height;
                        x = (Screen.height - b) / k;

                        if (k < 0)
                        {
                            x = Mathf.Clamp(x, 0, Screen.width * OutScreenClamp);
                        }
                        else
                        {
                            x = Mathf.Clamp(x, Screen.width * (1 - OutScreenClamp), Screen.width);
                        }
                    }
                }

                if(ScreenPos.x < OwnerScreenPos.x)
                {
                    if(b >=0 && b <= Screen.height)
                    {
                        y = b;
                        x = 0;

                        if (k > 0)
                        {
                            y = Mathf.Clamp(y,0, Screen.height * OutScreenClamp);
                        }
                        else
                        {
                            y = Mathf.Clamp(y, Screen.height * (1 - OutScreenClamp), Screen.height);
                        }
                    }
                }
                else
                {
                    if(k*Screen.width+b >= 0 && k*Screen.width+b <= Screen.height)
                    {
                        y = k * Screen.width + b;
                        x = Screen.width;

                        if (k > 0)
                        {
                            y = Mathf.Clamp(y, Screen.height * (1 - OutScreenClamp), Screen.height);
                        }
                        else
                        {
                            y = Mathf.Clamp(y, 0, Screen.height * OutScreenClamp);
                        }
                    }
                }

                WorldPointerPercent -= PercentChangeSpeed * Time.deltaTime;
                if(WorldPointerPercent < 0)
                {
                    WorldPointerPercent = 0;
                    PointerPercent+= PercentChangeSpeed * Time.deltaTime;

                    if(PointerPercent > 1)
                    {
                        PointerPercent = 1;
                    }

                    WorldPointer.SetActive(false);
                }

                Pointer.transform.localScale *= PointerPercent;

                Pointer.transform.position = new Vector3(x, y, 0);

                Pointer.transform.position -= Pointer.transform.right * PointerDis * Scale * PointerPercent;

                WorldPointer.transform.localScale = WorldPointerScale * Vector3.one * WorldPointerPercent;

                //Pointer.SetActive(true);
            }
        }
        else
        {
            if (WorldPointer != null)
            {
                WorldPointer.SetActive(false);
            }
            Destroy(Pointer);
        }
    }

    private void SetGuide()
    {
        if (Hold) // Show guide UI
        {
            if (Guide == null)
            {
                GameObject FoodGuideVFX = Owner.tag.Contains("1") ? Data.ChickenFoodGuideVFX : Data.DuckFoodGuideVFX;

                PlayerController pc = Owner.GetComponent<PlayerController>();
                Guide = GameObject.Instantiate(FoodGuideVFX, pc.PlayerFeet, false);
                pc.FoodTraverseVFXHolder = Guide;
                pc.FoodTraverseVFXHolder.transform.rotation = FoodGuideVFX.transform.rotation;
                pc.FoodTraverseVFXHolder.SetActive(true);

                GuideHopTimer = 0;
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

    private void GuideHop()
    {
        if (Guide == null)
        {
            return;
        }

        GuideHopTimer += Time.deltaTime;

        if(GuideHopTimer <= GuideHopTime / 2)
        {
            GuideOffset = Mathf.Lerp(0, GuideHopDis, GuideHopTimer / (GuideHopTime / 2));
        }
        else if(GuideHopTimer <= GuideHopTime)
        {
            GuideOffset = Mathf.Lerp(GuideHopDis, 0,  (GuideHopTimer - GuideHopTime / 2) / (GuideHopTime / 2));
        }
        else if(GuideHopTimer <= GuideHopTime + GuideHopInterval)
        {
            GuideOffset = 0;
        }
        else
        {
            GuideHopTimer = 0;
        }

        if (Owner == null)
        {
            return;
        }

        Transform Feet = Owner.GetComponent<PlayerController>().PlayerFeet;

        Guide.transform.position = Feet.position + Guide.transform.forward * GuideOffset;
    }

    public override void OnPickUp(GameObject owner)
    {
        base.OnPickUp(owner);

        GameObject Basket = Owner.tag.Contains("1") ? Team1Basket : Team2Basket;
        HoleEffect = Basket.GetComponent<CabelBasket>().HoleEffect;
        HoleEffect.SetActive(true);

        Hold = true;
        LastOwner = owner;
    }

    public override void OnDrop(bool customForce, Vector3 force)
    {
        base.OnDrop(customForce, force);

        Hold = false;
        Destroy(Guide);

        HoleEffect.SetActive(false);
    }

    public override void Fire(bool buttondown)
    {
        if (buttondown)
        {
            Owner.GetComponent<PlayerController>().ForceDropHandObject();
        }
    }

    public void OnSucked()
    {
        gameObject.layer = 2;
        GetComponent<BoxCollider>().enabled = false;

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;

        if (WorldPointer != null)
        {
            WorldPointer.SetActive(false);
        }
        Destroy(Pointer);

        HoleEffect.SetActive(false);

        Effect.SetActive(false);
    }

    protected override void _onWeaponDespawn()
    {
        base._onWeaponDespawn();
        _hitGroundOnce = false;
        EventManager.Instance.TriggerEvent(new BagelDespawn());

        Destroy(gameObject);
    }

    public GameObject GetLastOwner()
    {
        return LastOwner;
    }
}
