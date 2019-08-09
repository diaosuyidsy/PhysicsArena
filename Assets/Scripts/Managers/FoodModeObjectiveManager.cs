using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodModeObjectiveManager : ObjectiveManager
{
    private int Team1Score;
    private int Team2Score;
    private int _correctFoodPickedUp;

    public FoodModeObjectiveManager()
    {
        EventManager.Instance.AddHandler<FoodDelivered>(_onFoodDelivered);
        EventManager.Instance.AddHandler<ObjectPickedUp>(_onFoodPickedUp);
        EventManager.Instance.AddHandler<ObjectDropped>(_onFoodDropped);
    }

    public override void Destroy()
    {
        EventManager.Instance.RemoveHandler<FoodDelivered>(_onFoodDelivered);
        EventManager.Instance.RemoveHandler<ObjectPickedUp>(_onFoodPickedUp);
        EventManager.Instance.RemoveHandler<ObjectDropped>(_onFoodDropped);
    }

    private void _onFoodPickedUp(ObjectPickedUp ev)
    {
        if (ev.Obj.GetComponent<rtBirdFood>() != null && ((ev.Player.tag.Contains("1") && ev.Obj.tag.Contains("1"))
        || (ev.Player.tag.Contains("2") && ev.Obj.tag.Contains("2"))))
        {
            _correctFoodPickedUp++;
            GameObject CameraParent = Camera.main.transform.parent.gameObject;
            if (CameraParent.GetComponent<AudioSource>() == null)
            {
                AudioSource ass = CameraParent.AddComponent<AudioSource>();
                ass.loop = true;
            }
            if (!CameraParent.GetComponent<AudioSource>().isPlaying)
            {
                CameraParent.GetComponent<AudioSource>().clip = Services.AudioManager.AudioDataStore.DrumLoopAudioClip;
                CameraParent.GetComponent<AudioSource>().Play();
            }
        }
    }

    private void _onFoodDropped(ObjectDropped ev)
    {
        if (_correctFoodPickedUp <= 0) return;
        if (ev.Obj.GetComponent<rtBirdFood>() != null && ((ev.Player.tag.Contains("1") && ev.Obj.tag.Contains("1"))
        || (ev.Player.tag.Contains("2") && ev.Obj.tag.Contains("2"))))
        {
            _correctFoodPickedUp--;
            if (_correctFoodPickedUp <= 0)
            {
                GameObject CameraParent = Camera.main.transform.parent.gameObject;
                CameraParent.GetComponent<AudioSource>().Stop();
            }
        }
    }

    private void _onFoodDelivered(FoodDelivered fd)
    {
        if (fd.FoodTag == "Team1Resource")
        {
            Team1Score++;
            if (Team1Score >= 2)
            {
                EventManager.Instance.TriggerEvent(new GameEnd(1, fd.Food.transform, GameWinType.FoodWin));
            }
        }
        else
        {
            Team2Score++;
            if (Team2Score >= 2)
            {
                EventManager.Instance.TriggerEvent(new GameEnd(2, fd.Food.transform, GameWinType.FoodWin));
            }
        }
    }
}
