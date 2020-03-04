using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Rewired;
using TMPro;

public class ComicMenu : MonoBehaviour
{
    public ComicMenuData ComicMenuData;
    public GameObject CoverPage;
    public GameObject SelectionPage;
    public GameObject LoadingPage;
    public GameObject CoverPage2D;
    public GameObject MapSelectionLeft;
    public GameObject MapSelectionRight;
    public GameObject Maps;
    public GameObject EggHolder;
    public GameObject PlayerHolder;
    public GameObject CharacterImageHolder;
    public Camera SceneCamera;
    public GameObject HitBar;
    [HideInInspector]
    public string SelectedMapName = "BrawlModeReforged";
    private PlayerInformation _finalPlayerInformation;
    private Transform[] _eggs;
    private Transform[] _chickens;
    private Transform[] _hoverEggCharacterImages;
    public Transform[] _selectionCursors;
    public Transform[] _characterSelectionHolders;
    public Transform[] _3rdMenuHoleImages;
    public Transform[] _3rdMenuIndicators;
    public Transform[] _3rdMenuPrompts;
    public TextMeshPro _hintText;
    private Vector3[] _3rdMenuCursorsOriginalLocalPosition;
    private Vector3[] _chickenOriginalLocalPosition;
    private Vector3[] _eggsOriginalLocalPosition;
    private Vector3[] _eggsOriginalLocalScale;

    private FSM<ComicMenu> ComicMenuFSM;

    private void Awake()
    {
        ComicMenuFSM = new FSM<ComicMenu>(this);
        ComicMenuFSM.TransitionTo<FirstMenuState>();
        _eggs = new Transform[6];
        _chickens = new Transform[6];
        _hoverEggCharacterImages = new Transform[6];
        _3rdMenuCursorsOriginalLocalPosition = new Vector3[6];
        _chickenOriginalLocalPosition = new Vector3[6];
        _eggsOriginalLocalPosition = new Vector3[6];
        _eggsOriginalLocalScale = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            _eggs[i] = EggHolder.transform.GetChild(i);
            _chickens[i] = PlayerHolder.transform.GetChild(i);
            _hoverEggCharacterImages[i] = CharacterImageHolder.transform.GetChild(i);
            _3rdMenuCursorsOriginalLocalPosition[i] = new Vector3(_selectionCursors[i].localPosition.x, _selectionCursors[i].localPosition.y, _selectionCursors[i].localPosition.z);
            _chickenOriginalLocalPosition[i] = new Vector3(_chickens[i].localPosition.x, _chickens[i].localPosition.y, _chickens[i].localPosition.z);
            _eggsOriginalLocalPosition[i] = new Vector3(_eggs[i].localPosition.x, _eggs[i].localPosition.y, _eggs[i].localPosition.z);
            _eggsOriginalLocalScale[i] = new Vector3(_eggs[i].localScale.x, _eggs[i].localScale.y, _eggs[i].localScale.z);
        }
    }


    // Update is called once per frame
    void Update()
    {
        ComicMenuFSM.Update();
    }

    public void OnPlayerRechoose(int rewiredId)
    {
        ((CharacterSelectionState)ComicMenuFSM.CurrentState).OnPlayerRechoose(rewiredId);
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
        protected Camera _MainCamera;
        public override void OnEnter()
        {
            base.OnEnter();
            _MainCamera = Camera.main;
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
                DeactivateMenuItem(_menuItem);
                _index++;
                _vAxisInUse = true;
                _finishedEnter = false;
                _menuItem = Context.CoverPage2D.transform.GetChild(_index);
                ActivateMenuItem(_menuItem);
                return;
            }

            if (_VLAxisRaw < -0.2f && !_vAxisInUse && _finishedEnter && _index > 0)
            {
                DeactivateMenuItem(_menuItem);
                _index--;
                _vAxisInUse = true;
                _finishedEnter = false;
                _menuItem = Context.CoverPage2D.transform.GetChild(_index);
                ActivateMenuItem(_menuItem);
                return;
            }
            if (_ADown && !_vAxisInUse && _finishedEnter)
            {
                switch (_index)
                {
                    case 0:
                        TransitionTo<FirstMenuToSecondMenuTransition>();
                        break;
                    case 1:
                        break;
                    case 2:
                        Application.Quit();
                        break;
                }
                return;
            }

        }

        private void DeactivateMenuItem(Transform menuItem)
        {
            menuItem.GetChild(0).GetComponent<SpriteRenderer>().color = _MenuData.UnselectedFillColor;
            menuItem.GetChild(1).GetComponent<TextMeshPro>().color = _MenuData.UnselectedTextColor;
            menuItem.DOScale(_MenuData.UnselectedMenuItemScale, _MenuData.UnselectedMenuItemDuartion).SetEase(_MenuData.UnSelectedMenuItemEase);
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

    private class FirstMenuToSecondMenuTransition : MenuState
    {
        private Transform _playMenuItem;
        public override void OnEnter()
        {
            base.OnEnter();
            _playMenuItem = Context.CoverPage2D.transform.GetChild(0);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_playMenuItem.GetChild(1).GetComponent<TextMeshPro>().DOColor(_MenuData.PlayTextBlinkColor, _MenuData.PlayTextBlinkDuration)
            .SetEase(Ease.Flash, _MenuData.PlayTextBlinkTime, _MenuData.PlayTextBlinkPeriod));
            sequence.AppendInterval(_MenuData.PlayTextAfterDelay);
            sequence.Append(Context.CoverPage.transform.DOLocalMove(_MenuData.CoverPageLocalMovement, _MenuData.CoverPageMovementDuration)
            .SetEase(_MenuData.CoverPageMovementEase).SetRelative(true));
            sequence.Join(Context.CoverPage.transform.DOLocalRotate(_MenuData.CoverPageLocalRotation, _MenuData.CoverPageRotateDuration)
            .SetEase(_MenuData.CoverPageRotateEase).SetRelative(true));
            sequence.AppendCallback(() =>
            {
                foreach (SpriteRenderer sr in Context.CoverPage.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.sortingLayerID = 0;
                }

                foreach (TextMeshPro tmp in Context.CoverPage.GetComponentsInChildren<TextMeshPro>())
                {
                    tmp.sortingLayerID = 0;
                }
            });
            sequence.Append(Context.CoverPage.transform.DOLocalMove(_MenuData.CoverPageReturnLocalMovement, _MenuData.CoverpageReturnDuration)
            .SetEase(_MenuData.CoverPageReturnEase).SetRelative(true));
            sequence.Join(Context.CoverPage.transform.DOLocalRotate(_MenuData.CoverPageReturnLocalRotation, _MenuData.CoverPageReturnRotateDuration)
            .SetEase(_MenuData.CoverPageReturnRotateEase).SetRelative(true));
            sequence.AppendInterval(_MenuData.AfterCoverPageWaitDuration);
            sequence.Append(_MainCamera.transform.DOMove(_MenuData.CameraMapSelectionWorldLocation, _MenuData.CameraToMapSelectionDuration)
            .SetEase(_MenuData.CameraToMapSelectionEase));
            sequence.Join(_MainCamera.DOFieldOfView(_MenuData.CameraMapSelectionFOV, _MenuData.CameraToMapSelectionDuration)
            .SetEase(_MenuData.CameraToMapSelectionEase));
            sequence.AppendCallback(() =>
            {
                TransitionTo<MapSelectionState>();
            });
        }
    }

    private class MapToFirstMenuTransition : MenuState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Sequence sequence = DOTween.Sequence();
            _MainCamera.transform.DOMove(_MenuData.MapToFirstCameraPositionEase.EndValue, _MenuData.MapToFirstCameraPositionEase.Duration)
            .SetEase(_MenuData.MapToFirstCameraPositionEase.Ease).SetRelative(_MenuData.MapToFirstCameraPositionEase.Relative);
            _MainCamera.DOFieldOfView(_MenuData.MapToFirstCameraFOVEase.EndValue, _MenuData.MapToFirstCameraFOVEase.Duration)
            .SetEase(_MenuData.MapToFirstCameraFOVEase.Ease).SetRelative(_MenuData.MapToFirstCameraFOVEase.Relative);
            sequence.Append(Context.CoverPage.transform.DOMove(_MenuData.MapToFirstFirstMenuMove1.EndValue, _MenuData.MapToFirstFirstMenuMove1.Duration)
            .SetEase(_MenuData.MapToFirstFirstMenuMove1.Ease).SetRelative(_MenuData.MapToFirstFirstMenuMove1.Relative));
            sequence.Join(Context.CoverPage.transform.DOLocalRotate(_MenuData.MaptoFirstFirstMenuRotate1.EndValue, _MenuData.MaptoFirstFirstMenuRotate1.Duration)
            .SetEase(_MenuData.MaptoFirstFirstMenuRotate1.Ease).SetRelative(_MenuData.MaptoFirstFirstMenuRotate1.Relative));
            sequence.AppendCallback(() =>
            {
                foreach (SpriteRenderer sr in Context.CoverPage.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.sortingLayerID = SortingLayer.NameToID("CoverPage");
                }

                foreach (TextMeshPro tmp in Context.CoverPage.GetComponentsInChildren<TextMeshPro>())
                {
                    tmp.sortingLayerID = SortingLayer.NameToID("CoverPage");
                }
            });
            sequence.Append(Context.CoverPage.transform.DOMove(_MenuData.MapToFirstFirstMenuMove2.EndValue, _MenuData.MapToFirstFirstMenuMove2.Duration)
            .SetEase(_MenuData.MapToFirstFirstMenuMove2.Ease).SetRelative(_MenuData.MapToFirstFirstMenuMove2.Relative));
            sequence.AppendCallback(() =>
            {
                TransitionTo<FirstMenuState>();
            });
        }
    }

    private class MapSelectionState : MenuState
    {
        private int _mapIndex = 0;
        private int _maxMapCount;

        public override void OnEnter()
        {
            base.OnEnter();
            Context.MapSelectionLeft.SetActive(true);
            Context.MapSelectionRight.SetActive(true);
            Context.SelectedMapName = "BrawlModeReforged";
            _maxMapCount = Context.Maps.transform.childCount;
        }

        public override void Update()
        {
            base.Update();
            if (_HLAxisRaw > 0.2f && !_hAxisInUse)
            {
                BlinkLeftRight(false);
                return;
            }
            if (_HLAxisRaw < -0.2f && !_hAxisInUse)
            {
                BlinkLeftRight(true);
                return;
            }

            if (_ADown && !_hAxisInUse)
            {
                TransitionTo<MapToCharacterTransition>();
                return;
            }
            if (_BDown && !_hAxisInUse)
            {
                TransitionTo<MapToFirstMenuTransition>();
                return;
            }
        }

        private void BlinkLeftRight(bool left)
        {
            _hAxisInUse = true;
            Context.Maps.transform.GetChild(_mapIndex).gameObject.SetActive(false);
            _mapIndex = Mathf.Abs((_mapIndex + (left ? -1 : 1)) % _maxMapCount);
            Context.Maps.transform.GetChild(_mapIndex).gameObject.SetActive(true);
            Context.SelectedMapName = Context.Maps.transform.GetChild(_mapIndex).name;
            if (left)
                Context.MapSelectionLeft.GetComponent<SpriteRenderer>().DOColor(_MenuData.LeftRightClickColor, _MenuData.LeftRightClickDuration)
                .SetEase(Ease.Flash, 2, 0);
            else
                Context.MapSelectionRight.GetComponent<SpriteRenderer>().DOColor(_MenuData.LeftRightClickColor, _MenuData.LeftRightClickDuration)
                    .SetEase(Ease.Flash, 2, 0);
        }
    }

    private class MapToCharacterTransition : MenuState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.MapSelectionLeft.SetActive(false);
            Context.MapSelectionRight.SetActive(false);
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(_MenuData.InitialStopDuration);
            sequence.Append(_MainCamera.transform.DOMove(_MenuData.CameraStopLocation1, _MenuData.CameraToCharacterMoveDuration1).SetEase(_MenuData.CameraMoveEase1));
            sequence.AppendInterval(_MenuData.CameraStopDuration1);
            sequence.Append(_MainCamera.transform.DOMove(_MenuData.CameraStopLocation2, _MenuData.CameraToCharacterMoveDuration2).SetEase(_MenuData.CameraMoveEase2));
            sequence.AppendCallback(() =>
            {
                TransitionTo<CharacterSelectionState>();
            });
        }
    }

    private class CharacterSelectionToMapTransition : MenuState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(_MenuData.CharacterToMapInitialStopDuration);
            sequence.Append(_MainCamera.transform.DOMove(_MenuData.CharacterToMapCameraLocationEase.EndValue, _MenuData.CharacterToMapCameraLocationEase.Duration).SetEase(_MenuData.CharacterToMapCameraLocationEase.Ease));
            sequence.AppendCallback(() =>
            {
                TransitionTo<MapSelectionState>();
            });
        }
    }


    private class CharacterSelectionState : MenuState
    {
        private List<PlayerMap> _playerMap;
        private Camera _mainCamera;
        private Camera _sceneCamera { get { return Context.SceneCamera; } }

        /// <summary>
        /// How many cursors are on a egg
        /// index means the egg index
        /// int means the cursor count
        /// </summary>
        private int[] _eggCursors;
        /// <summary>
        /// Which Cursor Selected which egg
        /// Index means the cursor index
        /// int means the egg index;
        /// </summary>
        private int[] _cursorSelectedEggIndex;
        private int[] _eggSelectedCursorIndex;

        /// <summary>
        /// Index of players means the slot holes from 1-6
        /// Not the rewired id
        /// </summary>
        private FSM<CharacterSelectionState>[] _playersFSM;
        private FSM<CharacterSelectionState>[] _eggsFSM;

        private bool[] _eggSelected;
        private int _eggsSelectedAmount
        {
            get
            {
                int count = 0;
                foreach (var _egg in _eggSelected)
                {
                    if (_egg) count++;
                }
                return count;
            }
        }
        private int _blueEggsSelectedAmount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (_eggSelected[i]) count++;
                }
                return count;
            }
        }
        private int _redEggsSelectedAmount
        {
            get
            {
                int count = 0;
                for (int i = 3; i < 6; i++)
                {
                    if (_eggSelected[i]) count++;
                }
                return count;
            }
        }

        public override void Init()
        {
            base.Init();
            _playerMap = new List<PlayerMap>();
            _mainCamera = Camera.main;

            _playersFSM = new FSM<CharacterSelectionState>[6];
            _eggsFSM = new FSM<CharacterSelectionState>[6];
            _eggCursors = new int[6];
            _cursorSelectedEggIndex = new int[6];
            _eggSelectedCursorIndex = new int[6];
            _eggSelected = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                _eggsFSM[i] = new FSM<CharacterSelectionState>(this);
                _eggsFSM[i].TransitionTo<EggNormalState>();
            }
        }

        public override void Update()
        {
            base.Update();
            /// Poll for Loading State A Press
            /// 1. A player pressed A
            /// 2. This player is in selected state
            /// 3. All players are in selected state
            /// 4. Both team has players
            foreach (PlayerMap _pm in _playerMap)
            {
                if (ReInput.players.GetPlayer(_pm.RewiredPlayerID).GetButtonDown("Jump") &&
                    _playersFSM[_pm.GamePlayerID] != null &&
                    _playersFSM[_pm.GamePlayerID].CurrentState.GetType().Equals(typeof(SelectedState)) &&
                    _playerMap.Count >= 2 &&
                    _blueEggsSelectedAmount > 0 &&
                    _redEggsSelectedAmount > 0 && _playerMap.Count == _eggsSelectedAmount)
                {
                    _saveData();
                    TransitionTo<CharacterSelectionToLoadingTransition>();
                    return;
                }
            }
            if (_BDown && _playerMap.Count == 0)
            {
                TransitionTo<CharacterSelectionToMapTransition>();
                return;
            }
            for (int i = 0; i < ReInput.players.playerCount; i++)
            {
                if (ReInput.players.GetPlayer(i).GetButtonDown("JoinGame"))
                    _assignNextPlayer(i);
                if (ReInput.players.GetPlayer(i).GetButtonDown("Block") &&
                    _isRewiredPlayerInGame(i) &&
                    _playersFSM[_getGamePlayerIDFromRewiredId(i)].CurrentState.GetType().BaseType.Equals(typeof(ControllableState)))
                    _unassignPlayer(i);
            }
            for (int i = 0; i < 6; i++)
            {
                if (_playersFSM[i] != null)
                    _playersFSM[i].Update();
                _eggsFSM[i].Update();
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();
            for (int i = 0; i < 6; i++)
            {
                if (_playersFSM[i] != null) _playersFSM[i].CurrentState.CleanUp();
                if (_eggsFSM[i] != null) _eggsFSM[i].CurrentState.CleanUp();
            }
        }

        private void _saveData()
        {
            int[] rewiredPlayerID = new int[_playerMap.Count];
            int[] GameplayerID = new int[_playerMap.Count];
            int[] colorIndex = new int[_playerMap.Count];
            for (int i = 0; i < _playerMap.Count; i++)
            {
                PlayerMap pm = _playerMap[i];
                rewiredPlayerID[i] = pm.RewiredPlayerID;
                GameplayerID[i] = pm.GamePlayerID;
                colorIndex[i] = _cursorSelectedEggIndex[pm.GamePlayerID];
                //saveData[i] = new PlayerInformation(pm.RewiredPlayerID, pm.GamePlayerID, _cursorSelectedEggIndex[pm.GamePlayerID]);
            }
            PlayerInformation saveData = new PlayerInformation(rewiredPlayerID, GameplayerID, colorIndex);
            Context._finalPlayerInformation = saveData;
            DataSaver.saveData(saveData, "PlayersInformation");
        }

        public void OnPlayerRechoose(int rewiredID)
        {
            ((SelectedState)_playersFSM[_getGamePlayerIDFromRewiredId(rewiredID)].CurrentState).OnPlayerRechoose();
        }

        private void _onPlayerEggStateChange()
        {
            Context._hintText.DOKill();
            if (_playerMap.Count == 1 && _eggsSelectedAmount == 1)
            {
                Context._hintText.DOText("Need More Players", 1f).SetDelay(2f);
            }
            else if (_playerMap.Count > 1 && _eggsSelectedAmount == _playerMap.Count && (_blueEggsSelectedAmount == 0 || _redEggsSelectedAmount == 0))
            {
                Context._hintText.DOText("Need Players On Both Teams", 1f).SetDelay(2f);
            }
            else if (_playerMap.Count > 1 && _eggsSelectedAmount == _playerMap.Count)
            {
                Context._hintText.DOText("Hit the Box", 1f);
            }
            else
                Context._hintText.DOText("", 0f);
        }

        private bool _isRewiredPlayerInGame(int rewiredID)
        {
            foreach (PlayerMap pm in _playerMap)
            {
                if (pm.RewiredPlayerID == rewiredID) return true;
            }
            return false;
        }
        private void _onCursorChange(int _change, int index)
        {
            _eggCursors[index] += _change;
            if (_eggCursors[index] > 0 && _eggsFSM[index].CurrentState.GetType().Equals(typeof(EggNormalState))) _eggsFSM[index].TransitionTo<EggHoveredState>();
            else if (_eggCursors[index] == 0 && _eggsFSM[index].CurrentState.GetType().Equals(typeof(EggHoveredState))) _eggsFSM[index].TransitionTo<EggNormalState>();
        }

        private PlayerMap _getPlayerFSMIndex(FSM<CharacterSelectionState> fsm)
        {
            for (int i = 0; i < 6; i++)
            {
                if (_playersFSM[i] != null && _playersFSM[i] == fsm)
                {
                    foreach (PlayerMap _pm in _playerMap)
                    {
                        if (_pm.GamePlayerID == i)
                            return _pm;
                    }
                }
            }
            return null;
        }

        private int _getEggFSMIndex(FSM<CharacterSelectionState> fsm)
        {
            for (int i = 0; i < 6; i++)
            {
                if (_eggsFSM[i] != null && _eggsFSM[i] == fsm)
                {
                    return i;
                }
            }
            return -1;
        }

        private int _getGamePlayerIDFromRewiredId(int rewiredID)
        {
            foreach (PlayerMap pm in _playerMap)
            {
                if (pm.RewiredPlayerID == rewiredID)
                    return pm.GamePlayerID;
            }
            return -1;
        }

        private int _getRewiredIDFromGameplayerId(int gamePlayerID)
        {
            foreach (PlayerMap pm in _playerMap)
            {
                if (pm.GamePlayerID == gamePlayerID)
                    return pm.RewiredPlayerID;
            }
            return -1;
        }

        private void _assignNextPlayer(int rewiredPlayerId)
        {
            if (_playerMap.Count >= 6) { return; }

            int gamePlayerId = _getNextGamePlayerId();
            if (gamePlayerId == 7) return;
            _playerMap.Add(new PlayerMap(rewiredPlayerId, gamePlayerId));
            _playersFSM[gamePlayerId] = new FSM<CharacterSelectionState>(this);
            _playersFSM[gamePlayerId].TransitionTo<UnselectingState>();
            Player rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerId);
            // Context._audioSource.PlayOneShot(_MenuData.MenuAudioData.ThirdMenuJoinGameAudioClip);
            rewiredPlayer.controllers.maps.SetMapsEnabled(false, "Assignment");
            _onPlayerEggStateChange();
        }

        private void _unassignPlayer(int rewiredPlayerId)
        {
            int gamePlayerId = -1;
            int playerMapIndex = -1;
            for (int i = 0; i < _playerMap.Count; i++)
            {
                if (_playerMap[i].RewiredPlayerID == rewiredPlayerId)
                {
                    gamePlayerId = _playerMap[i].GamePlayerID;
                    playerMapIndex = i;
                    break;
                }
            }
            if (gamePlayerId == -1) return;
            // Context._audioSource.PlayOneShot(_MenuData.MenuAudioData.ThirdMenuUnJoinGameAudioClip);

            ReInput.players.GetPlayer(rewiredPlayerId).controllers.maps.SetMapsEnabled(true, "Assignment");
            _playerMap.RemoveAt(playerMapIndex);
            _playersFSM[gamePlayerId].CurrentState.CleanUp();
            _playersFSM[gamePlayerId] = null;
            _onPlayerEggStateChange();
        }

        private int _getNextGamePlayerId()
        {
            for (int i = 0; i < 6; i++)
            {
                if (_playersFSM[i] == null)
                {
                    return i;
                }
            }
            return 7;
        }

        private class PlayerMap
        {
            public int RewiredPlayerID;
            public int GamePlayerID;

            public PlayerMap(int rewiredPlayerID, int gamePlayerID)
            {
                RewiredPlayerID = rewiredPlayerID;
                GamePlayerID = gamePlayerID;
            }
        }

        #region Player Cursor States
        private abstract class PlayerState : FSM<CharacterSelectionState>.State
        {
            protected int _gamePlayerIndex { get; private set; }
            protected int _rewiredPlayerIndex { get; private set; }

            public override void Init()
            {
                base.Init();
                ReInput.ControllerPreDisconnectEvent += _onControllerDisconnected;
                _gamePlayerIndex = Context._getPlayerFSMIndex(Parent).GamePlayerID;
                _rewiredPlayerIndex = Context._getPlayerFSMIndex(Parent).RewiredPlayerID;
            }

            protected virtual void _onControllerDisconnected(ControllerStatusChangedEventArgs args)
            {
                int rewiredId = args.controllerId;
                if (_rewiredPlayerIndex != rewiredId) return;
                Context._unassignPlayer(rewiredId);
                return;
            }

            public override void CleanUp()
            {
                base.CleanUp();
                ReInput.ControllerPreDisconnectEvent -= _onControllerDisconnected;
                Context.Context._selectionCursors[_gamePlayerIndex].localPosition = Context.Context._3rdMenuCursorsOriginalLocalPosition[_gamePlayerIndex];
                Context.Context._selectionCursors[_gamePlayerIndex].gameObject.SetActive(false);
                Context.Context._3rdMenuIndicators[_gamePlayerIndex].gameObject.SetActive(false);
                Context.Context._3rdMenuPrompts[_gamePlayerIndex].gameObject.SetActive(true);
                for (int i = 0; i < 6; i++) { Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(i).gameObject.SetActive(false); }
                Context.Context._characterSelectionHolders[_gamePlayerIndex].GetComponent<SpriteRenderer>().color = Context._MenuData.HoleNormalColor;
            }
        }

        private abstract class ControllableState : PlayerState
        {
            protected Vector3 _CursorPos { get { return Context.Context._selectionCursors[_gamePlayerIndex].position; } }
            protected float _ScreenX;
            protected float _ScreenY;

            public override void Init()
            {
                base.Init();
                _ScreenX = Screen.width;
                _ScreenY = Screen.height;
            }

            public override void OnEnter()
            {
                base.OnEnter();
                Context.Context._selectionCursors[_gamePlayerIndex].gameObject.SetActive(true);
                Context.Context._3rdMenuPrompts[_gamePlayerIndex].gameObject.SetActive(false);
            }

            public override void Update()
            {
                base.Update();
                float HLAxis = ReInput.players.GetPlayer(_rewiredPlayerIndex).GetAxisRaw("Move Horizontal");
                float VLAxis = ReInput.players.GetPlayer(_rewiredPlayerIndex).GetAxisRaw("Move Vertical");
                Transform cursor = Context.Context._selectionCursors[_gamePlayerIndex];
                Vector3 finalPosition = cursor.position + new Vector3(HLAxis * Context._MenuData.CursorMoveSpeed.x, 0f, -VLAxis * Context._MenuData.CursorMoveSpeed.y) * Time.deltaTime;
                finalPosition.x = Mathf.Clamp(finalPosition.x, Context._MenuData.MouseClampMinValue.x, Context._MenuData.MouseClampMaxValue.x);
                finalPosition.z = Mathf.Clamp(finalPosition.z, Context._MenuData.MouseClampMinValue.y, Context._MenuData.MouseClampMaxValue.y);
                cursor.position = finalPosition;
            }
        }

        private class UnselectingState : ControllableState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                Context.Context._3rdMenuIndicators[_gamePlayerIndex].gameObject.SetActive(true);
                // Disable all grey images
                for (int i = 0; i < 6; i++)
                {
                    Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(i).gameObject.SetActive(false);
                }
                // Change Hole Image to normal
                Context.Context._characterSelectionHolders[_gamePlayerIndex].GetComponent<SpriteRenderer>().color = Context._MenuData.HoleNormalColor;
            }

            public override void Update()
            {
                base.Update();
                float xPercentage = (_CursorPos.x - (0.2f)) / (0.6f - 0.2f);
                float yPercentage = (_CursorPos.z - (-0.3f)) / (0f + 0.3f);

                Vector3 translatedCursorPosition = new Vector3(_ScreenX * xPercentage, _ScreenY * yPercentage, 10f);

                RaycastHit hit;
                Ray ray = Context._sceneCamera.ScreenPointToRay(translatedCursorPosition);

                /// If cursor Casted to a egg
                if (Physics.Raycast(ray, out hit, 100f, Context._MenuData.EggLayer))
                {
                    TransitionTo<HoveringState>();
                    return;
                }
            }
        }

        private class HoveringState : ControllableState
        {
            private int _castedEggSiblingIndex;

            public override void OnEnter()
            {
                base.OnEnter();
                // Context.Context._audioSource.PlayOneShot(Context._MenuData.MenuAudioData.ThirdMenuEggJiggleAudioClip);
                Context.Context._3rdMenuIndicators[_gamePlayerIndex].gameObject.SetActive(false);

                float xPercentage = (_CursorPos.x - (0.2f)) / (0.6f - 0.2f);
                float yPercentage = (_CursorPos.z - (-0.3f)) / (0f + 0.3f);
                Vector3 translatedCursorPosition = new Vector3(_ScreenX * xPercentage, _ScreenY * yPercentage, 10f);


                RaycastHit hit;
                Ray ray = Context._sceneCamera.ScreenPointToRay(translatedCursorPosition);

                /// If cursor Casted to a egg
                if (Physics.Raycast(ray, out hit, 100f, Context._MenuData.EggLayer))
                {
                    _castedEggSiblingIndex = hit.transform.GetSiblingIndex();
                    Context._onCursorChange(1, _castedEggSiblingIndex);
                    /// Show Grey image on hole
                    Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).gameObject.SetActive(true);
                    Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).GetComponent<SpriteRenderer>().color = Context._MenuData.HoverImageColor;
                    // Also Change Hole Color to related color
                    Context.Context._characterSelectionHolders[_gamePlayerIndex].GetComponent<SpriteRenderer>().color = Context._MenuData.HoleCursorveHoverColor[_castedEggSiblingIndex];
                    // Hide the indicators
                    Context.Context._3rdMenuIndicators[_gamePlayerIndex].gameObject.SetActive(false);
                }
            }

            public override void Update()
            {
                base.Update();
                float xPercentage = (_CursorPos.x - (0.2f)) / (0.6f - 0.2f);
                float yPercentage = (_CursorPos.z - (-0.3f)) / (0f + 0.3f);
                Vector3 translatedCursorPosition = new Vector3(_ScreenX * xPercentage, _ScreenY * yPercentage, 10f);

                RaycastHit hit;
                Ray ray = Context._sceneCamera.ScreenPointToRay(translatedCursorPosition);

                /// If cursor Casted to another egg
                if (Physics.Raycast(ray, out hit, 100f, Context._MenuData.EggLayer))
                {
                    if (hit.transform.GetSiblingIndex() != _castedEggSiblingIndex)
                    {
                        Context._onCursorChange(-1, _castedEggSiblingIndex);
                        Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).gameObject.SetActive(false);
                        //Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).DOLocalMoveX(-244f, Context._MenuData.HoleImageOutDuration);
                        TransitionTo<HoveringState>();
                        return;
                    }
                    else
                    {
                        if (ReInput.players.GetPlayer(_rewiredPlayerIndex).GetButtonDown("Jump"))
                        {
                            /// Record this cursor selected which egg
                            Context._cursorSelectedEggIndex[_gamePlayerIndex] = _castedEggSiblingIndex;
                            Context._eggSelectedCursorIndex[_castedEggSiblingIndex] = _gamePlayerIndex;

                            TransitionTo<HoverToSelectedTransition>();
                            Context._eggsFSM[_castedEggSiblingIndex].TransitionTo<EggToChickenTransition>();
                            return;
                        }
                    }
                }
                else
                {
                    Context._onCursorChange(-1, _castedEggSiblingIndex);
                    Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).gameObject.SetActive(false);
                    TransitionTo<UnselectingState>();
                    return;
                }
            }

            public override void CleanUp()
            {
                base.CleanUp();
                Context._onCursorChange(-1, _castedEggSiblingIndex);
            }
        }

        private class HoverToSelectedTransition : PlayerState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                /// 1. Disable Cursor
                /// 2. Reset Cursor
                /// 3. Changed Color of Holes
                /// 4. Change Color of Hole Images
                /// 5. Maybe display a little VFX and sound;
                int castedEggIndex = Context._cursorSelectedEggIndex[_gamePlayerIndex];
                Context.Context._selectionCursors[_gamePlayerIndex].gameObject.SetActive(false);
                Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(castedEggIndex).GetComponent<SpriteRenderer>().color = Color.white;
                Context.Context._characterSelectionHolders[_gamePlayerIndex].GetComponent<SpriteRenderer>().color = Context._MenuData.HoleSelectedColor[castedEggIndex];
                Context.Context.HitBar.transform.GetChild(castedEggIndex).gameObject.SetActive(true);
            }
        }

        private class SelectedState : PlayerState
        {
            // public override void Update()
            // {
            //     base.Update();
            //     if (ReInput.players.GetPlayer(_rewiredPlayerIndex).GetButtonDown("Block"))
            //     {
            //         Context._eggsFSM[Context._cursorSelectedEggIndex[_gamePlayerIndex]].TransitionTo<EggNormalState>();
            //         Context._eggSelected[Context._cursorSelectedEggIndex[_gamePlayerIndex]] = false;
            //         Context._onPlayerEggStateChange();
            //         TransitionTo<UnselectingState>();
            //         return;
            //     }
            // }
            public void OnPlayerRechoose()
            {
                int castedEggIndex = Context._cursorSelectedEggIndex[_gamePlayerIndex];
                Context._eggsFSM[Context._cursorSelectedEggIndex[_gamePlayerIndex]].TransitionTo<EggNormalState>();
                Context._eggSelected[Context._cursorSelectedEggIndex[_gamePlayerIndex]] = false;
                Context._onPlayerEggStateChange();
                Context.Context.HitBar.transform.GetChild(castedEggIndex).gameObject.SetActive(false);
                Context.Context.HitBar.transform.GetChild(castedEggIndex).GetComponent<SpriteRenderer>().color = Color.black;
                TransitionTo<UnselectingState>();
                return;
            }
        }

        #endregion

        #region Egg States
        private abstract class EggState : FSM<CharacterSelectionState>.State
        {
            protected int _eggIndex;
            protected Transform _eggChild;

            public override void Init()
            {
                base.Init();
                _eggIndex = Context._getEggFSMIndex(Parent);
                _eggChild = Context.Context._eggs[_eggIndex].GetChild(0);
                ReInput.ControllerPreDisconnectEvent += _onControllerDisconnected;
            }

            protected virtual void _onControllerDisconnected(ControllerStatusChangedEventArgs args)
            {
            }

            public override void CleanUp()
            {
                base.CleanUp();
                ReInput.ControllerPreDisconnectEvent -= _onControllerDisconnected;
            }
        }

        private class EggNormalState : EggState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                Context.Context._eggs[_eggIndex].GetComponent<Collider>().enabled = true;
                _eggChild.localScale = Context._MenuData.EggNormalScale;
                // _eggChild.GetComponent<Renderer>().material.SetColor("_OutlineColor", Context._MenuData.EggNormalOutlineColor);
                Context.Context._hoverEggCharacterImages[_eggIndex].GetComponent<DOTweenAnimation>().DOPlayBackwards();
            }
        }

        private class EggHoveredState : EggState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                _eggChild.localScale = Context._MenuData.EggActivatedScale;
                // _eggChild.GetComponent<Renderer>().material.SetColor("_OutlineColor", Context._MenuData.EggCursorOverOutlineColor);
                _eggChild.GetComponent<DOTweenAnimation>().DORestart();
                Context.Context._hoverEggCharacterImages[_eggIndex].GetComponent<DOTweenAnimation>().DOPlayForward();
            }
        }

        private class EggToChickenTransition : EggState
        {
            private Sequence _sequence;

            public override void OnEnter()
            {
                base.OnEnter();
                Context.Context._eggs[_eggIndex].GetComponent<Collider>().enabled = false;
                Context.Context._hoverEggCharacterImages[_eggIndex].GetComponent<DOTweenAnimation>().DOPlayBackwards();
                _sequence = DOTween.Sequence();
                // Context.Context._audioSource.PlayOneShot(Context._MenuData.MenuAudioData.ThirdMenuEggSelectedAudioClip);

                _sequence.Append(Context.Context._eggs[_eggIndex].DOShakeRotation(Context._MenuData.ETC_EggShakeDuration, Context._MenuData.ETC_EggShakeStrength, Context._MenuData.ETC_EggShakeVibrato));
                _sequence.Append(Context.Context._eggs[_eggIndex].DOScale(Context._MenuData.ETC_EggScaleAmount, Context._MenuData.ETC_EggScaleDuration).SetEase(Context._MenuData.ETC_EggScaleAnimationCurve));
                _sequence.Join(Context.Context._eggs[_eggIndex].DOLocalMoveY(Context._MenuData.ETC_EggMoveYAmount, Context._MenuData.ETC_EggMoveYDuration).SetEase(Context._MenuData.ETC_EggMoveYAnimationCurve));
                _sequence.Append(Context.Context._chickens[_eggIndex].DOLocalMoveY(Context._MenuData.ETC_ChickenMoveYAmount, Context._MenuData.ETC_ChickenMoveYDuration).
                            SetEase(Context._MenuData.ETC_ChickenMoveYEase).
                            SetDelay(Context._MenuData.ETC_ChickenMoveYDelay));
                _sequence.AppendCallback(() =>
                {
                    // Context.Context._audioSource.PlayOneShot(Context._MenuData.MenuAudioData.ThirdMenuChickenLandAudioClip);
                    // Context.Context._camera.GetComponent<DOTweenAnimation>().DORestartAllById("Land");
                    // Instantiate(Context._MenuData.ETC_ChickenLandVFX, Context.Context._chickens[_eggIndex].position + Context._MenuData.ETC_ChickenLandVFXOffset, Context._MenuData.ETC_ChickenLandVFX.transform.rotation);
                    TransitionTo<ChickenState>();
                    return;
                });
            }

            protected override void _onControllerDisconnected(ControllerStatusChangedEventArgs args)
            {
                base._onControllerDisconnected(args);
                if (_eggIndex != Context._cursorSelectedEggIndex[args.controllerId]) return;
                _sequence.Kill();
                // Instantiate(Context._MenuData.ETC_ChickenDisappearVFX, Context.Context._chickens[_eggIndex].position + Context._MenuData.ETC_ChickenDisapperavFXOffset, Context._MenuData.ETC_ChickenDisappearVFX.transform.rotation);
                Context.Context._chickens[_eggIndex].localPosition = Context.Context._chickenOriginalLocalPosition[_eggIndex];
                Context.Context._eggs[_eggIndex].localPosition = Context.Context._eggsOriginalLocalPosition[_eggIndex];
                Context.Context._eggs[_eggIndex].localScale = Context.Context._eggsOriginalLocalScale[_eggIndex];
                Context._eggCursors[_eggIndex] = 0;
                TransitionTo<EggNormalState>();
            }
        }

        private class ChickenState : EggState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                /// Since now chicken has landed
                /// Switch Cursor to Selected state
                int playerindex = Context._eggSelectedCursorIndex[_eggIndex];
                Context._eggSelected[_eggIndex] = true;
                Context._onPlayerEggStateChange();
                Context.Context._chickens[_eggIndex].GetComponent<PlayerController>().enabled = true;
                Context.Context._chickens[_eggIndex].GetComponent<Rigidbody>().isKinematic = false;
                Context.Context._chickens[_eggIndex].GetComponent<PlayerController>().Init(Context._getRewiredIDFromGameplayerId(playerindex));
                Context._playersFSM[playerindex].TransitionTo<SelectedState>();
            }

            protected override void _onControllerDisconnected(ControllerStatusChangedEventArgs args)
            {
                base._onControllerDisconnected(args);
                if (_eggIndex != Context._cursorSelectedEggIndex[args.controllerId]) return;
                TransitionTo<EggNormalState>();
            }

            public override void OnExit()
            {
                base.OnExit();
                // Instantiate(Context._MenuData.ETC_ChickenDisappearVFX, Context.Context._chickens[_eggIndex].position + Context._MenuData.ETC_ChickenDisapperavFXOffset, Context._MenuData.ETC_ChickenDisappearVFX.transform.rotation);
                /// Reset Chicken Position, Animation
                /// Reset Egg Position, LocalScale
                /// Reset Egg Children Scale, Shader Color
                /// Reset _eggCursors
                // Context.Context._audioSource.PlayOneShot(Context._MenuData.MenuAudioData.ThirdMenuChickenToEggAudioClip);
                Context.Context._chickens[_eggIndex].localPosition = Context.Context._chickenOriginalLocalPosition[_eggIndex];
                Context.Context._eggs[_eggIndex].localPosition = Context.Context._eggsOriginalLocalPosition[_eggIndex];
                Context.Context._eggs[_eggIndex].localScale = Context.Context._eggsOriginalLocalScale[_eggIndex];
                Context._eggCursors[_eggIndex] = 0;
            }
        }
        #endregion
    }

    private class CharacterSelectionToLoadingTransition : MenuState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(_MenuData.CharacterSelectionToLoadingPauseDuration);
            sequence.Append(_MainCamera.transform.DOMove(_MenuData.CharacterSelectionToLoadingCameraLocationEase.EndValue,
            _MenuData.CharacterSelectionToLoadingCameraLocationEase.Duration).SetEase(_MenuData.CharacterSelectionToLoadingCameraFOVEase.Ease));
            sequence.Join(_MainCamera.DOFieldOfView(_MenuData.CharacterSelectionToLoadingCameraFOVEase.EndValue, _MenuData.CharacterSelectionToLoadingCameraFOVEase.Duration)
            .SetEase(_MenuData.CharacterSelectionToLoadingCameraFOVEase.Ease));
            sequence.AppendInterval(_MenuData.CharacterSelectionToLoadingPauseDuration2);
            sequence.Append(Context.SelectionPage.transform.DOMove(_MenuData.CharacterSelectionToLoadingPageMovementEase1.EndValue,
            _MenuData.CharacterSelectionToLoadingPageMovementEase1.Duration).SetEase(_MenuData.CharacterSelectionToLoadingPageMovementEase1.Ease)
            .SetRelative(_MenuData.CharacterSelectionToLoadingPageMovementEase1.Relative));
            sequence.AppendCallback(() =>
            {
                foreach (SpriteRenderer sr in Context.SelectionPage.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.sortingLayerID = 0;
                }

                foreach (TextMeshPro tmp in Context.SelectionPage.GetComponentsInChildren<TextMeshPro>())
                {
                    tmp.sortingLayerID = 0;
                }
            });
            sequence.Append(Context.SelectionPage.transform.DOMove(_MenuData.CharacterSelectionToLoadingPageMovementEase2.EndValue,
            _MenuData.CharacterSelectionToLoadingPageMovementEase2.Duration).SetEase(_MenuData.CharacterSelectionToLoadingPageMovementEase2.Ease)
            .SetRelative(_MenuData.CharacterSelectionToLoadingPageMovementEase2.Relative));
            sequence.AppendCallback(() =>
            {
                TransitionTo<LoadingState>();
            });
        }
    }

    private class LoadingState : MenuState
    {

    }
}
