using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ResourceCollector : MonoBehaviour
{
    public TeamNum Team;
    public GameObject BasketModel;

    //private int TeamTracker;
    private int _resourcePickedUp;
    private Tweener _lightTween;

    private void Awake()
    {
        Transform temp = transform.parent.Find("RespawnPoints");
    }

    private void _onObjectPickedUp(ObjectPickedUp ev)
    {
        _resourcePickedUp++;
        if (_resourcePickedUp - 1 > 0) return;
        if ((Team == TeamNum.Team1 && ev.Obj.tag.Contains("1") && ev.Player.tag.Contains("1") ||
        (Team == TeamNum.Team2 && ev.Obj.tag.Contains("2") && ev.Player.tag.Contains("2"))))
        {
            transform.GetChild(0).gameObject.SetActive(true);
            _lightTween = GetComponentInChildren<Light>().DOIntensity(3.5f, 1f).SetLoops(-1, LoopType.Yoyo);
            BasketModel.GetComponent<Renderer>().material.SetFloat("_Outline", 4f);
        }
    }

    private void _onObjectDropped(ObjectDropped ev)
    {
        _resourcePickedUp--;
        if (_resourcePickedUp != 0) return;
        if ((Team == TeamNum.Team1 && ev.Obj.tag.Contains("1") && ev.Player.tag.Contains("1") ||
        (Team == TeamNum.Team2 && ev.Obj.tag.Contains("2") && ev.Player.tag.Contains("2"))))
        {
            transform.GetChild(0).gameObject.SetActive(false);
            _lightTween.Kill();
            BasketModel.GetComponent<Renderer>().material.SetFloat("_Outline", 1.5f);
        }
    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.AddHandler<ObjectDropped>(_onObjectDropped);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onObjectPickedUp);
        EventManager.Instance.RemoveHandler<ObjectDropped>(_onObjectDropped);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Team == TeamNum.Team1)
        {
            if (other.CompareTag("Team1Resource"))
            {
                //TeamTracker++;
                other.tag = "Untagged";
                other.gameObject.layer = LayerMask.NameToLayer("Default");
                EventManager.Instance.TriggerEvent(new FoodDelivered(other.gameObject, "Team1Resource", other.GetComponent<rtBirdFood>().LastHolder));

            }
        }
        else
        {
            if (other.CompareTag("Team2Resource"))
            {
                //TeamTracker++;
                other.tag = "Untagged";
                other.gameObject.layer = LayerMask.NameToLayer("Default");

                EventManager.Instance.TriggerEvent(new FoodDelivered(other.gameObject, "Team2Resource", other.GetComponent<rtBirdFood>().LastHolder));

            }
        }


    }
}
