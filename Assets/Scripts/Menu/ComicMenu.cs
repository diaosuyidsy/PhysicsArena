using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Rewired;
using TMPro;

public class ComicMenu : MonoBehaviour
{
    public ComicMenuData ComicMenuData;
    public GameObject CoverPage2D;
    private FSM<ComicMenu> ComicMenuFSM;

    private void Awake()
    {
        ComicMenuFSM = new FSM<ComicMenu>(this);

    }


    // Update is called once per frame
    void Update()
    {
        ComicMenuFSM.Update();
    }

    private abstract class MenuState : FSM<ComicMenu>.State
    {
        protected float _VLAxisRaw
        {
            get
            {
                float result = 0f;
                for (int i = 0; i < ReInput.players.playerCount; i++)
                {
                    result = ReInput.players.GetPlayer(i).GetAxisRaw("Move Vertical");
                    if (!Mathf.Approximately(0f, result)) return result;
                }
                return result;
            }
        }
        protected float _HLAxisRaw
        {
            get
            {
                float result = 0f;
                for (int i = 0; i < ReInput.players.playerCount; i++)
                {
                    result = ReInput.players.GetPlayer(i).GetAxisRaw("Move Horizontal");
                    if (!Mathf.Approximately(0f, result)) return result;
                }
                return result;
            }
        }
        protected bool _ADown
        {
            get
            {
                for (int i = 0; i < ReInput.players.playerCount; i++)
                {
                    if (ReInput.players.GetPlayer(i).GetButtonDown("Jump")) return true;
                }
                return false;
            }
        }
        protected bool _BDown
        {
            get
            {
                for (int i = 0; i < ReInput.players.playerCount; i++)
                {
                    if (ReInput.players.GetPlayer(i).GetButtonDown("Block")) return true;
                }
                return false;
            }
        }
        protected bool _vAxisInUse = true;
        protected bool _hAxisInUse = true;
        protected ComicMenuData _MenuData { get { return Context.ComicMenuData; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _vAxisInUse = true;
            _hAxisInUse = true;
        }

        public override void Update()
        {
            base.Update();
            if (_VLAxisRaw == 0f) _vAxisInUse = false;
            if (_HLAxisRaw == 0f) _hAxisInUse = false;
        }

    }

    private class FirstMenuState : MenuState
    {
        private int _index;
        private Transform _menuItem;
        private bool _finishedEnter;
        private int _maxIndex;
        public override void OnEnter()
        {
            base.OnEnter();
            for (int i = 0; i < ReInput.players.playerCount; i++)
            {
                ReInput.players.GetPlayer(i).controllers.maps.SetMapsEnabled(true, "Assignment");
            }
            _maxIndex = Context.CoverPage2D.transform.childCount - 1;
            _index = 0;
            _menuItem = Context.CoverPage2D.transform.GetChild(_index);
            _finishedEnter = false;
            ActivateMenuItem(_menuItem);
        }

        public override void Update()
        {
            base.Update();
            if (_VLAxisRaw > 0.2f && !_vAxisInUse && _finishedEnter && _index < _maxIndex)
            {
                _index++;
                _menuItem = Context.CoverPage2D.transform.GetChild(_index);
                ActivateMenuItem(_menuItem);
            }
        }

        private void ActivateMenuItem(Transform menuItem)
        {
            // 1. Set fill color to red
            // 2. Set Text Color to White
            // 3. Enlarge Menu Item
            menuItem.GetChild(0).GetComponent<SpriteRenderer>().color = _MenuData.SelectedFillColor;
            menuItem.GetChild(1).GetComponent<TextMeshPro>().color = _MenuData.SelectedTextColor;
            menuItem.DOScale(_MenuData.SelectedMenuItemScale, _MenuData.SelectedMenuItemDuration).SetEase(_MenuData.SelectedMenuItemEase)
            .OnComplete(() =>
            {
                _finishedEnter = true;
            });
        }
    }
}
