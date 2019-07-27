﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	public MenuData MenuData;
	public string MapName { get; set; }
	private Transform _selectingBar;
	private Transform _selectedBar;
	private GameObject _play;
	private GameObject _setting;
	private GameObject _quit;
	private Transform _title;
	private Transform _camera;
	private Transform _cartMode;
	private Transform _brawlMode;
	private Transform _2ndMenu;
	private Transform _3rdMenu;
	private TextMeshProUGUI _2ndMenuTitle;
	private TextMeshProUGUI _3rdMenuTitle;
	private Transform[] _3rdMenuHolders;
	private Transform[] _3rdMenuPrompts;
	private Transform[] _3rdMenuCursors;
	private Transform[] _3rdMenuIndicators;
	private Vector3[] _3rdMenuCursorsOriginalLocalPosition;
	private Transform _eggHolder;
	private Transform[] _eggs;
	private Transform[] _3rdMenuCharacterImages;
	private Transform[] _3rdMenuHoleImages;

	private Player _mainPlayer;
	private FSM<Menu> _menuFSM;

	private void Awake()
	{
		_mainPlayer = ReInput.players.GetPlayer(0);
		_selectingBar = transform.Find("MainMenu").Find("SelectingBar");
		_selectedBar = transform.Find("MainMenu").Find("SelectBar");
		_play = transform.Find("MainMenu").Find("Play").gameObject;
		_setting = transform.Find("MainMenu").Find("Setting").gameObject;
		_quit = transform.Find("MainMenu").Find("Quit").gameObject;
		_title = transform.Find("MainMenu").Find("Title");
		_2ndMenu = transform.Find("2ndMenu");
		_cartMode = _2ndMenu.Find("CartMode");
		_brawlMode = _2ndMenu.Find("BrawlMode");
		_2ndMenuTitle = _2ndMenu.Find("Title").GetComponent<TextMeshProUGUI>();
		_camera = Camera.main.transform;
		_3rdMenu = transform.Find("3rdMenu");
		_3rdMenuTitle = _3rdMenu.Find("Title").GetComponent<TextMeshProUGUI>();
		_3rdMenuHolders = new Transform[6];
		_3rdMenuPrompts = new Transform[6];
		_3rdMenuCursors = new Transform[6];
		_3rdMenuIndicators = new Transform[6];
		_3rdMenuCursorsOriginalLocalPosition = new Vector3[6];
		_eggs = new Transform[6];
		_eggHolder = GameObject.Find("Eggs").transform;
		_3rdMenuCharacterImages = new Transform[6];
		_3rdMenuHoleImages = new Transform[6];
		for (int i = 0; i < 6; i++)
		{
			_3rdMenuHolders[i] = _3rdMenu.Find("Holes").GetChild(i);
			_3rdMenuPrompts[i] = _3rdMenu.Find("PromptTexts").GetChild(i);
			_3rdMenuCursors[i] = _3rdMenu.Find("Cursors").GetChild(i);
			_3rdMenuIndicators[i] = _3rdMenu.Find("Indicators").GetChild(i);
			_3rdMenuCursorsOriginalLocalPosition[i] = new Vector3(_3rdMenuCursors[i].localPosition.x, _3rdMenuCursors[i].localPosition.y, _3rdMenuCursors[i].localPosition.z);
			_eggs[i] = _eggHolder.GetChild(i);
			_3rdMenuCharacterImages[i] = _3rdMenu.Find("CharacterImage").GetChild(i);
			_3rdMenuHoleImages[i] = _3rdMenu.Find("HoleImage").GetChild(i);
		}
		Debug.Assert(_selectingBar != null);
		Debug.Assert(_selectedBar != null);
		Debug.Assert(_play != null);
		Debug.Assert(_setting != null);
		Debug.Assert(_quit != null);
		Debug.Assert(_title != null);
		Debug.Assert(_brawlMode != null);
		Debug.Assert(_cartMode != null);
		_menuFSM = new FSM<Menu>(this);
		_menuFSM.TransitionTo<FirstMenuPlayState>();
	}

	private void Update()
	{
		_menuFSM.Update();
	}

	private abstract class MenuState : FSM<Menu>.State
	{
		protected float _VLAxisRaw { get { return Context._mainPlayer.GetAxisRaw("Move Vertical"); } }
		protected float _HLAxisRaw { get { return Context._mainPlayer.GetAxisRaw("Move Horizontal"); } }
		protected bool _ADown { get { return Context._mainPlayer.GetButtonDown("Jump"); } }
		protected bool _BDown { get { return Context._mainPlayer.GetButtonDown("Block"); } }
		protected bool _vAxisInUse = true;
		protected bool _hAxisInUse = true;
		protected MenuData _MenuData { get { return Context.MenuData; } }

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

	#region 3rd Menu States
	private class CharacterSelectionState : MenuState
	{
		private List<PlayerMap> _playerMap;
		private Camera _mainCamera;

		private int[] _eggCursors;

		/// <summary>
		/// Index of players means the slot holes from 1-6
		/// Not the rewired id
		/// </summary>
		private FSM<CharacterSelectionState>[] _playersFSM;
		private FSM<CharacterSelectionState>[] _eggsFSM;

		public override void Init()
		{
			base.Init();
			_playerMap = new List<PlayerMap>();
			_mainCamera = Camera.main;

			_playersFSM = new FSM<CharacterSelectionState>[6];
			_eggsFSM = new FSM<CharacterSelectionState>[6];
			_eggCursors = new int[6];
			for (int i = 0; i < 6; i++)
			{
				_eggsFSM[i] = new FSM<CharacterSelectionState>(this);
				_eggsFSM[i].TransitionTo<EggNormalState>();
			}
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

		public override void Update()
		{
			base.Update();
			for (int i = 0; i < 6; i++)
			{
				if (_playersFSM[i] != null)
					_playersFSM[i].Update();
				_eggsFSM[i].Update();
			}
			if (_BDown && _playerMap.Count == 0)
				TransitionTo<CharacterSelectionToMapTransition>();
			for (int i = 0; i < ReInput.players.playerCount; i++)
			{
				if (ReInput.players.GetPlayer(i).GetButtonDown("JoinGame"))
					_assignNextPlayer(i);
				if (ReInput.players.GetPlayer(i).GetButtonDown("Block"))
					_unassignPlayer(i);
			}
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

			rewiredPlayer.controllers.maps.SetMapsEnabled(false, "Assignment");
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

			ReInput.players.GetPlayer(rewiredPlayerId).controllers.maps.SetMapsEnabled(true, "Assignment");
			_playerMap.RemoveAt(playerMapIndex);
			_playersFSM[gamePlayerId].CurrentState.CleanUp();
			_playersFSM[gamePlayerId] = null;
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

		private abstract class PlayerState : FSM<CharacterSelectionState>.State
		{
			protected int _gamePlayerIndex { get; private set; }
			protected int _rewiredPlayerIndex { get; private set; }

			public override void Init()
			{
				base.Init();
				_gamePlayerIndex = Context._getPlayerFSMIndex(Parent).GamePlayerID;
				_rewiredPlayerIndex = Context._getPlayerFSMIndex(Parent).RewiredPlayerID;
			}
		}

		private abstract class ControllableState : PlayerState
		{
			protected Vector3 _CursorPos { get { return Context.Context._3rdMenuCursors[_gamePlayerIndex].position; } }

			public override void OnEnter()
			{
				base.OnEnter();
				Context.Context._3rdMenuCursors[_gamePlayerIndex].gameObject.SetActive(true);
				Context.Context._3rdMenuPrompts[_gamePlayerIndex].gameObject.SetActive(false);
			}

			public override void Update()
			{
				base.Update();
				float HLAxis = ReInput.players.GetPlayer(_rewiredPlayerIndex).GetAxis("Move Horizontal");
				float VLAxis = ReInput.players.GetPlayer(_rewiredPlayerIndex).GetAxis("Move Vertical");
				Transform cursor = Context.Context._3rdMenuCursors[_gamePlayerIndex];
				cursor.localPosition += new Vector3(HLAxis, -VLAxis) * Time.deltaTime * Context._MenuData.CursorMoveSpeed;
			}

			public override void CleanUp()
			{
				base.CleanUp();
				Context.Context._3rdMenuCursors[_gamePlayerIndex].localPosition = Context.Context._3rdMenuCursorsOriginalLocalPosition[_gamePlayerIndex];
				Context.Context._3rdMenuCursors[_gamePlayerIndex].gameObject.SetActive(false);
				Context.Context._3rdMenuIndicators[_gamePlayerIndex].gameObject.SetActive(false);
				Context.Context._3rdMenuPrompts[_gamePlayerIndex].gameObject.SetActive(true);
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
				Context.Context._3rdMenuHolders[_gamePlayerIndex].GetComponent<Image>().color = Context._MenuData.HoleNormalColor;
			}

			public override void Update()
			{
				base.Update();
				RaycastHit hit;
				Ray ray = Context._mainCamera.ScreenPointToRay(_CursorPos);

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
				Context.Context._3rdMenuIndicators[_gamePlayerIndex].gameObject.SetActive(false);
				RaycastHit hit;
				Ray ray = Context._mainCamera.ScreenPointToRay(_CursorPos);

				/// If cursor Casted to a egg
				if (Physics.Raycast(ray, out hit, 100f, Context._MenuData.EggLayer))
				{
					_castedEggSiblingIndex = hit.transform.GetSiblingIndex();
					Context._onCursorChange(1, _castedEggSiblingIndex);
					/// Show Grey image on hole
					Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).gameObject.SetActive(true);
					Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).GetComponent<Image>().color = Context._MenuData.HoverImageColor;

					//// Also Change Hole Color to related color
					Context.Context._3rdMenuHolders[_gamePlayerIndex].GetComponent<Image>().color = Context._MenuData.HoleCursorveHoverColor[_castedEggSiblingIndex];
					//// Hide the indicators
					Context.Context._3rdMenuIndicators[_gamePlayerIndex].gameObject.SetActive(false);
				}

			}

			public override void Update()
			{
				base.Update();
				RaycastHit hit;
				Ray ray = Context._mainCamera.ScreenPointToRay(_CursorPos);

				/// If cursor Casted to an egg
				if (Physics.Raycast(ray, out hit, 100f, Context._MenuData.EggLayer))
				{
					if (hit.transform.GetSiblingIndex() != _castedEggSiblingIndex)
					{
						TransitionTo<HoveringState>();
						return;
					}
					else
					{
						if (ReInput.players.GetPlayer(_rewiredPlayerIndex).GetButtonDown("Jump"))
						{
							TransitionTo<SelectedState>();
							return;
						}
					}
				}
				else
				{
					TransitionTo<UnselectingState>();
					return;
				}
			}

			public override void CleanUp()
			{
				base.CleanUp();
				Context._onCursorChange(-1, _castedEggSiblingIndex);
				Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).gameObject.SetActive(false);
				Context.Context._3rdMenuHolders[_gamePlayerIndex].GetComponent<Image>().color = Context._MenuData.HoleNormalColor;
			}

			public override void OnExit()
			{
				base.OnExit();
				Context._onCursorChange(-1, _castedEggSiblingIndex);
				Context.Context._3rdMenuHoleImages[_gamePlayerIndex].GetChild(_castedEggSiblingIndex).gameObject.SetActive(false);
			}
		}

		private class SelectedState : PlayerState
		{
			public override void Update()
			{
				base.Update();
				if (ReInput.players.GetPlayer(_rewiredPlayerIndex).GetButtonDown("Block"))
					TransitionTo<HoveringState>();
			}
		}

		private abstract class EggState : FSM<CharacterSelectionState>.State
		{
			protected int _eggIndex;
			protected Transform _eggChild;

			public override void Init()
			{
				base.Init();
				_eggIndex = Context._getEggFSMIndex(Parent);
				_eggChild = Context.Context._eggs[_eggIndex].GetChild(0);
			}
		}

		private class EggNormalState : EggState
		{
			public override void OnEnter()
			{
				base.OnEnter();
				_eggChild.localScale = Vector3.one;
				_eggChild.GetComponent<Renderer>().material.SetColor("_OutlineColor", Context._MenuData.EggNormalOutlineColor);
				Context.Context._3rdMenuCharacterImages[_eggIndex].GetComponent<DOTweenAnimation>().DOPlayBackwards();
			}
		}

		private class EggHoveredState : EggState
		{
			public override void OnEnter()
			{
				base.OnEnter();
				_eggChild.localScale = Context._MenuData.EggActivatedScale;
				_eggChild.GetComponent<Renderer>().material.SetColor("_OutlineColor", Context._MenuData.EggCursorOverOutlineColor);
				_eggChild.GetComponent<DOTweenAnimation>().DORestart();
				Context.Context._3rdMenuCharacterImages[_eggIndex].GetComponent<DOTweenAnimation>().DOPlayForward();
			}
		}
	}
	#endregion

	#region 2nd - 3rd Menu Transition States
	private class MapToCharacterSelectionTransition : MenuState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			Context._brawlMode.DOLocalMoveY(1500f, _MenuData.PanelMoveOutDuration).
				SetEase(_MenuData.PanelMoveOutEase).SetDelay(_MenuData.BrawlPanelMoveOutDelay);
			Context._cartMode.DOLocalMoveY(1500f, _MenuData.PanelMoveOutDuration).
				SetEase(_MenuData.PanelMoveOutEase).SetDelay(_MenuData.CartPanelMoveOutDelay);
			Context._2ndMenuTitle.DOText("", _MenuData.PanelMoveOutDuration).SetDelay(_MenuData.TextMoveOutDelay);
			Context._camera.DOLocalMoveX(15.58f, _MenuData.CameraToCharacterSelectionMoveDuration).SetDelay(_MenuData.CameraToCharacterSelectionMoveDelay)
				.SetEase(_MenuData.CameraToCharacterSelectionMoveEase);
			Context._3rdMenuTitle.DOText("Character Selection", _MenuData.ThirdMenuTitleMoveInDuration).SetDelay(_MenuData.ThirdMenuTitleMoveInDelay).OnComplete(() =>
			{
				TransitionTo<CharacterSelectionState>();
			});
			for (int i = 0; i < 6; i++)
			{
				Context._3rdMenuHolders[i].DOLocalMoveX(-763f, _MenuData.ThirdMenuHolderMoveInDuration[i]).
					SetEase(_MenuData.ThirdMenuHolderMoveInEase).
					SetDelay(_MenuData.ThirdMenuHolderMoveInDelay[i]);
				Context._3rdMenuPrompts[i].DOLocalMoveX(-763f, _MenuData.ThirdMenuHolderMoveInDuration[i]).
					SetEase(_MenuData.ThirdMenuHolderMoveInEase).
					SetDelay(_MenuData.ThirdMenuHolderMoveInDelay[i]);
			}
		}
	}

	private class CharacterSelectionToMapTransition : MenuState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			for (int i = 0; i < 6; i++)
			{
				Context._3rdMenuHolders[i].DOLocalMoveX(-1192f, _MenuData.ThirdMenuHolderMoveOutDuration[i]).
					SetEase(_MenuData.ThirdMenuHolderMoveOutEase).
					SetDelay(_MenuData.ThirdMenuHolderMoveOutDelay[i]);
				Context._3rdMenuPrompts[i].DOLocalMoveX(-1192f, _MenuData.ThirdMenuHolderMoveOutDuration[i]).
					SetEase(_MenuData.ThirdMenuHolderMoveOutEase).
					SetDelay(_MenuData.ThirdMenuHolderMoveOutDelay[i]);
			}
			Context._3rdMenuTitle.DOText("", _MenuData.ThirdMenuTitleMoveOutDuration).SetDelay(_MenuData.ThirdMenuTitleMoveOutDelay);
			Context._camera.DOLocalMoveX(4.3f, _MenuData.CameraFromCharacterSelectionToModeSelectMoveDuration).SetEase(_MenuData.CameraFromCharacterSelectionToModeSelectMoveEase).
				SetDelay(_MenuData.CameraFromCharacterSelectionToModeSelectMoveDelay);
			Context._cartMode.DOLocalMoveY(-38f, _MenuData.SecondMenuCarModeMoveTime).SetEase(_MenuData.SecondMenuCarModeEase).SetDelay(_MenuData.SecondMenuCarModeMoveDelay);
			Context._brawlMode.DOLocalMoveY(-38f, _MenuData.SecondMenuBrawlModeMoveTime).SetEase(_MenuData.SecondMenuBrawlModeEase).SetDelay(_MenuData.SecondMenuBrawlModeMoveDelay);
			Context._2ndMenuTitle.DOText(_MenuData.SecondMenuTitleString, _MenuData.SecondMenuTitleMoveTime).SetDelay(_MenuData.SecondMenuTitleMoveDelay).OnComplete(() =>
			{
				TransitionTo<CarModeState>();
			});
		}
	}
	#endregion

	#region 2nd Menu States
	private abstract class ModeMenuState : MenuState
	{

	}

	private abstract class MapSelectState : ModeMenuState
	{
		protected Transform _Mask;
		protected Transform _MapPage;
		protected int _MapIndex;
		protected bool _finishedmove;

		public override void OnEnter()
		{
			base.OnEnter();
			_finishedmove = true;
			_MapIndex = 0;
			_MapPage.GetChild(0).GetComponent<Image>().color = _MenuData.SelectedMapColor;
			for (int i = 1; i < _MapPage.childCount; i++)
			{
				_MapPage.GetChild(i).GetComponent<Image>().color = _MenuData.UnselectedMapColor;
			}
		}

		public override void Update()
		{
			base.Update();
			if (_VLAxisRaw > 0.2f && !_vAxisInUse && _MapIndex < _MapPage.childCount - 1 && _finishedmove)
			{
				_finishedmove = false;
				_MapPage.GetChild(_MapIndex++).GetComponent<Image>().color = _MenuData.UnselectedMapColor;
				_MapPage.DOLocalMoveY(60f, _MenuData.MapMoveDuration).
					SetEase(_MenuData.MapMoveEase).
					SetRelative(true).
					OnComplete(() =>
					{
						if (_MapIndex < _MapPage.childCount)
							_MapPage.GetChild(_MapIndex).GetComponent<Image>().color = _MenuData.SelectedMapColor;
						_finishedmove = true;
					});
				return;
			}

			if (_VLAxisRaw < -0.2f && !_vAxisInUse && _MapIndex > 0 && _finishedmove)
			{
				_finishedmove = false;
				_MapPage.GetChild(_MapIndex--).GetComponent<Image>().color = _MenuData.UnselectedMapColor;
				_MapPage.DOLocalMoveY(-60f, _MenuData.MapMoveDuration).
					SetEase(_MenuData.MapMoveEase).
					SetRelative(true).
					OnComplete(() =>
					{
						if (_MapIndex >= 0)
							_MapPage.GetChild(_MapIndex).GetComponent<Image>().color = _MenuData.SelectedMapColor;
						_finishedmove = true;
					});
				return;
			}

			if (_ADown)
			{
				Context.MapName = _MapPage.GetChild(_MapIndex).name;
				TransitionTo<MapToCharacterSelectionTransition>();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			_Mask.Find("MainPage").DOLocalMoveY(0f, _MenuData.ModeImageMoveDuration).SetEase(_MenuData.ModeImageMoveEase).SetDelay(_MenuData.ModeImageMoveDelay);
			_Mask.Find("MapPage").DOLocalMoveY(-110.6f, _MenuData.ModeMapMoveInDuration).SetEase(_MenuData.ModeImageMoveEase).SetDelay(_MenuData.ModeImageMoveDelay);
			Context._brawlMode.DOLocalMove(new Vector3(449f, -38f), _MenuData.ModePanelSelectedZoomDuration).SetEase(_MenuData.ModePanelSelectedZoomEase);
			Context._brawlMode.DOScale(new Vector3(1.74f, 1.74f), _MenuData.ModePanelSelectedZoomDuration).SetEase(_MenuData.ModePanelSelectedZoomEase);

			Context._cartMode.DOLocalMove(new Vector3(-452f, -38f), _MenuData.ModePanelSelectedZoomDuration).SetEase(_MenuData.ModePanelSelectedZoomEase);
			Context._cartMode.DOScale(new Vector3(1.74f, 1.74f), _MenuData.ModePanelSelectedZoomDuration).SetEase(_MenuData.ModePanelSelectedZoomEase);
		}
	}

	private class BrawlMapSelectState : MapSelectState
	{
		public override void Init()
		{
			base.Init();
			_Mask = Context._brawlMode.Find("Mask");
			_MapPage = Context._brawlMode.Find("Mask").Find("MapPage");
			Debug.Assert(_MapPage != null);
		}

		public override void Update()
		{
			base.Update();
			if (_BDown)
				TransitionTo<BrawlModeState>();
		}
	}

	private class CartMapSelectState : MapSelectState
	{
		public override void Init()
		{
			base.Init();
			_Mask = Context._cartMode.Find("Mask");
			_MapPage = Context._cartMode.Find("Mask").Find("MapPage");
			Debug.Assert(_MapPage != null);
		}

		public override void Update()
		{
			base.Update();
			if (_BDown)
				TransitionTo<CarModeState>();
		}
	}

	private abstract class ModeToMapSelectTransition : ModeMenuState
	{
		protected GameObject _Mode;
		protected GameObject _WholeMask;

		public override void OnEnter()
		{
			base.OnEnter();
			_WholeMask.SetActive(false);
			_Mode.GetComponent<Image>().DOColor(_MenuData.ModeSelectedBlinkColor, _MenuData.ModeSelectedBlinkDurition).
				SetEase(Ease.Flash, _MenuData.ModeSelectedBlinkTime, _MenuData.ModeSelectedBlinkPeriod).
				OnComplete(() =>
				{
					_Mode.transform.Find("Mask").Find("MainPage").DOLocalMoveY(140f, _MenuData.ModeImageMoveDuration).SetEase(_MenuData.ModeImageMoveEase).SetDelay(_MenuData.ModeImageMoveDelay);
					_Mode.transform.Find("Mask").Find("MapPage").DOLocalMoveY(-20f, _MenuData.ModeMapMoveInDuration).SetEase(_MenuData.ModeImageMoveEase).SetDelay(_MenuData.ModeImageMoveDelay);
					_OnBlinkComplete();
				});
		}

		protected virtual void _OnBlinkComplete()
		{
		}

	}

	private class BrawlModeToMapSelectTransition : ModeToMapSelectTransition
	{
		public override void Init()
		{
			base.Init();
			_Mode = Context._brawlMode.gameObject;
			_WholeMask = Context._brawlMode.Find("WholeMask").gameObject;
		}

		protected override void _OnBlinkComplete()
		{
			base._OnBlinkComplete();
			_Mode.transform.DOLocalMove(new Vector3(5f, 1f), _MenuData.ModePanelSelectedZoomDuration).SetEase(_MenuData.ModePanelSelectedZoomEase);
			_Mode.transform.DOScale(new Vector3(2.3f, 2.3f), _MenuData.ModePanelSelectedZoomDuration).SetEase(_MenuData.ModePanelSelectedZoomEase);

			Context._cartMode.DOLocalMove(new Vector3(-723f, -266f), _MenuData.ModePanelSelectedZoomDuration).SetEase(_MenuData.ModePanelSelectedZoomEase);
			Context._cartMode.transform.DOScale(new Vector3(1f, 1f), _MenuData.ModePanelSelectedZoomDuration).SetEase(_MenuData.ModePanelSelectedZoomEase).
				OnComplete(() =>
				{
					TransitionTo<BrawlMapSelectState>();
				});
		}
	}

	private class CartModeToMapSelectTransition : ModeToMapSelectTransition
	{
		public override void Init()
		{
			base.Init();
			_Mode = Context._cartMode.gameObject;
			_WholeMask = Context._cartMode.Find("WholeMask").gameObject;
		}

		protected override void _OnBlinkComplete()
		{
			base._OnBlinkComplete();
			_Mode.transform.DOLocalMove(new Vector3(5f, 1f), _MenuData.ModePanelSelectedZoomDuration).SetEase(Ease.OutQuad);
			_Mode.transform.DOScale(new Vector3(2.3f, 2.3f), _MenuData.ModePanelSelectedZoomDuration).SetEase(Ease.OutQuad);

			Context._brawlMode.DOLocalMove(new Vector3(736f, -266f), _MenuData.ModePanelSelectedZoomDuration).SetEase(Ease.OutQuad);
			Context._brawlMode.transform.DOScale(new Vector3(1f, 1f), _MenuData.ModePanelSelectedZoomDuration).SetEase(Ease.OutQuad).
				OnComplete(() =>
				{
					TransitionTo<CartMapSelectState>();
				});
		}
	}

	private abstract class ModeSelectState : ModeMenuState
	{
		protected GameObject _wholeMask;

		public override void OnEnter()
		{
			base.OnEnter();
			_wholeMask.SetActive(false);
		}

		public override void Update()
		{
			base.Update();
			if (_BDown)
				TransitionTo<ModeMenuToFirstMenuTransition>();
		}

		public override void OnExit()
		{
			base.OnExit();
			_wholeMask.SetActive(true);
		}
	}

	private class BrawlModeState : ModeSelectState
	{
		public override void Init()
		{
			base.Init();
			_wholeMask = Context._brawlMode.Find("WholeMask").gameObject;
			Debug.Assert(_wholeMask != null);
		}

		public override void Update()
		{
			base.Update();
			if (_HLAxisRaw < -0.2f && !_hAxisInUse)
				TransitionTo<CarModeState>();
			if (_ADown)
				TransitionTo<BrawlModeToMapSelectTransition>();
		}
	}

	private class CarModeState : ModeSelectState
	{
		public override void Init()
		{
			base.Init();
			_wholeMask = Context._cartMode.Find("WholeMask").gameObject;
			Debug.Assert(_wholeMask != null);
		}

		public override void Update()
		{
			base.Update();
			if (_HLAxisRaw > 0.2f && !_hAxisInUse)
				TransitionTo<BrawlModeState>();
			if (_ADown)
				TransitionTo<CartModeToMapSelectTransition>();
		}
	}
	#endregion

	#region 1st - 2nd Menu Transition States
	private class FirstMenuToModeMenuTransition : MenuState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			Context._play.GetComponent<TextMeshProUGUI>().color = Context.MenuData.SelectingFontColor;
			Context._selectedBar.GetComponent<Image>().enabled = true;
			Context._selectingBar.GetComponent<Image>().enabled = false;
			Context._play.GetComponent<TextMeshProUGUI>().DOColor(_MenuData.SelectedFontColor, _MenuData.FirstMenuSelectedBlinkDurition).
				SetEase(Ease.Flash, _MenuData.FirstMenuSelectedBlinkTime, _MenuData.FirstMenuSelectedBlinkPeriod).OnComplete(() =>
				{
					Context._title.DOLocalMoveX(_MenuData.FirstMenuTitleMoveOutPosition, _MenuData.FirstMenuTitleMoveOutDuration).
						SetEase(_MenuData.FirstMenuTitleMoveOutEase).
						SetDelay(_MenuData.FirstMenuTitleMoveOutDelay);
					Context._play.transform.DOLocalMoveX(_MenuData.FirstMenuPlayMoveOutPosition, _MenuData.FirstMenuPlayMoveOutDuration).
						SetEase(_MenuData.FirstMenuPlayMoveOutEase).
						SetDelay(_MenuData.FirstMenuPlayMoveOutDelay);
					Context._setting.transform.DOLocalMoveX(_MenuData.FirstMenuSettingMoveOutPosition, _MenuData.FirstMenuSettingMoveOutDuration).
						SetEase(_MenuData.FirstMenuSettingMoveOutEase).
						SetDelay(_MenuData.FirstMenuSettingMoveOutDelay);
					Context._quit.transform.DOLocalMoveX(_MenuData.FirstMenuQuitMoveOutPosition, _MenuData.FirstMenuQuitMoveOutDuration).
						SetEase(_MenuData.FirstMenuQuitMoveOutEase).
						SetDelay(_MenuData.FirstMenuQuitMoveOutDelay);
					Context._selectingBar.DOLocalMoveX(_MenuData.FirstMenuPlayMoveOutPosition, _MenuData.FirstMenuPlayMoveOutDuration).
						SetEase(_MenuData.FirstMenuPlayMoveOutEase).
						SetDelay(_MenuData.FirstMenuPlayMoveOutDelay);
					Context._selectedBar.DOLocalMoveX(_MenuData.FirstMenuPlayMoveOutPosition, _MenuData.FirstMenuPlayMoveOutDuration).
						SetEase(_MenuData.FirstMenuPlayMoveOutEase).
						SetDelay(_MenuData.FirstMenuPlayMoveOutDelay);
				});
			Context._camera.DOLocalMoveX(4.3f, _MenuData.FirstMenuToSecondCameraMoveTime).
				SetEase(_MenuData.FirstMenuToSecondCameraEase).
				SetDelay(_MenuData.FirstMenuToSecondCameraMoveDelay);

			Context._cartMode.DOLocalMoveY(-38f, _MenuData.SecondMenuCarModeMoveTime).SetEase(_MenuData.SecondMenuCarModeEase).SetDelay(_MenuData.SecondMenuCarModeMoveDelay);
			Context._brawlMode.DOLocalMoveY(-38f, _MenuData.SecondMenuBrawlModeMoveTime).SetEase(_MenuData.SecondMenuBrawlModeEase).SetDelay(_MenuData.SecondMenuBrawlModeMoveDelay);
			Context._2ndMenuTitle.DOText(_MenuData.SecondMenuTitleString, _MenuData.SecondMenuTitleMoveTime).SetDelay(_MenuData.SecondMenuTitleMoveDelay).OnComplete(() =>
			{
				TransitionTo<CarModeState>();
			});
		}
	}

	private class ModeMenuToFirstMenuTransition : MenuState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			Context._2ndMenuTitle.DOText("", _MenuData.SecondMenuTitleMoveOutTime).SetDelay(_MenuData.SecondMenuTitleMoveOutDelay);
			Context._cartMode.DOLocalMoveY(1500f, _MenuData.SecondMenuCarModeMoveOutTime).SetEase(_MenuData.SecondMenuCarModeEase).SetDelay(_MenuData.SecondMenuCarModeMoveOutDelay);
			Context._brawlMode.DOLocalMoveY(1500f, _MenuData.SecondMenuBrawlModeMoveOutTime).SetEase(_MenuData.SecondMenuBrawlModeEase).SetDelay(_MenuData.SecondMenuBrawlModeMoveOutDelay);
			Context._camera.DOMoveX(-2.94f, _MenuData.SecondMenuToFirstCameraMoveTime).SetEase(_MenuData.SecondMenuToFirstCameraEase).SetDelay(_MenuData.SecondMenuToFirstCameraMoveDelay);
			Context._title.DOLocalMoveX(-425f, _MenuData.FirstMenuTitleMoveInDuration).
					SetEase(_MenuData.FirstMenuTitleMoveInEase).
					SetDelay(_MenuData.FirstMenuTitleMoveInDelay);
			Context._play.transform.DOLocalMoveX(-748f, _MenuData.FirstMenuPlayMoveInDuration).
				SetEase(_MenuData.FirstMenuPlayMoveInEase).
				SetDelay(_MenuData.FirstMenuPlayMoveInDelay);
			Context._setting.transform.DOLocalMoveX(-748f, _MenuData.FirstMenuSettingMoveInDuration).
				SetEase(_MenuData.FirstMenuSettingMoveInEase).
				SetDelay(_MenuData.FirstMenuSettingMoveInDelay);
			Context._quit.transform.DOLocalMoveX(-748f, _MenuData.FirstMenuQuitMoveInDuration).
				SetEase(_MenuData.FirstMenuQuitMoveInEase).
				SetDelay(_MenuData.FirstMenuQuitMoveInDelay);
			Context._selectingBar.DOLocalMoveX(-554f, _MenuData.FirstMenuPlayMoveInDuration).
				SetEase(_MenuData.FirstMenuPlayMoveInEase).
				SetDelay(_MenuData.FirstMenuPlayMoveInDelay);
			Context._selectedBar.DOLocalMoveX(-554f, _MenuData.FirstMenuPlayMoveInDuration).
				SetEase(_MenuData.FirstMenuPlayMoveInEase).
				SetDelay(_MenuData.FirstMenuPlayMoveInDelay).OnComplete(() =>
				{
					TransitionTo<FirstMenuPlayState>();
				});
		}
	}
	#endregion

	#region 1st Menu States
	private abstract class FisrtMenuState : MenuState
	{
		protected bool _finishedEnter = false;
		protected float _yValue;
		protected GameObject _menuItem;

		public override void OnEnter()
		{
			base.OnEnter();
			Context._selectedBar.GetComponent<Image>().enabled = false;
			Context._selectingBar.GetComponent<Image>().enabled = true;
			_finishedEnter = false;
			Context._selectedBar.DOLocalMoveY(_yValue, 0f);
			Context._selectingBar.DOLocalMoveY(_yValue, _MenuData.FirstMenuSelectionTransitionDuration).SetEase(_MenuData.FirstMenuSelectionEase).
				OnComplete(() =>
				{
					_menuItem.GetComponent<TextMeshProUGUI>().color = Context.MenuData.SelectingFontColor;
					_finishedEnter = true;
				});
		}

		public override void OnExit()
		{
			base.OnExit();
			_menuItem.GetComponent<TextMeshProUGUI>().color = Context.MenuData.NormalFontColor;
		}
	}

	private class FirstMenuPlayState : FisrtMenuState
	{
		public override void Init()
		{
			base.Init();
			_yValue = -4.3f;
			_menuItem = Context._play;
		}

		public override void Update()
		{
			base.Update();
			if (_VLAxisRaw > 0.2f && !_vAxisInUse && _finishedEnter)
				TransitionTo<FirstMenuSettingState>();
			if (_ADown)
				TransitionTo<FirstMenuToModeMenuTransition>();
		}
	}

	private class FirstMenuSettingState : FisrtMenuState
	{
		public override void Init()
		{
			base.Init();
			_yValue = -123.4f;
			_menuItem = Context._setting;
		}

		public override void Update()
		{
			base.Update();
			if (_VLAxisRaw > 0.2f && !_vAxisInUse && _finishedEnter)
			{
				TransitionTo<FirstMenuQuitState>();
				return;
			}
			if (_VLAxisRaw < -0.2f && !_vAxisInUse && _finishedEnter)
			{
				TransitionTo<FirstMenuPlayState>();
				return;
			}
		}
	}

	private class FirstMenuQuitState : FisrtMenuState
	{
		public override void Init()
		{
			base.Init();
			_yValue = -241f;
			_menuItem = Context._quit;
		}

		public override void Update()
		{
			base.Update();
			if (_VLAxisRaw < -0.2f && !_vAxisInUse && _finishedEnter)
			{
				TransitionTo<FirstMenuSettingState>();
				return;
			}
		}
	}
	#endregion
}
