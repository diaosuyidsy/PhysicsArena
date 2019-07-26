using System.Collections;
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

	private class CharacterSelectionState : MenuState
	{
		private List<PlayerMap> _playerMap;
		private int _gamePlayerIdCounter;
		private bool[] _slotsTaken;
		private bool[][] _whoScannedEgg;
		private Camera _mainCamera;
		private int[] _cursorPreviousScannedEgg;
		private bool[] _eggState;

		public override void Init()
		{
			base.Init();
			_playerMap = new List<PlayerMap>();
			_slotsTaken = new bool[6];
			_whoScannedEgg = new bool[6][];
			for (int i = 0; i < 6; i++)
			{
				_whoScannedEgg[i] = new bool[6];
			}
			_mainCamera = Camera.main;
			_cursorPreviousScannedEgg = new int[] { -1, -1, -1, -1, -1, -1 };
			_eggState = new bool[6];
		}

		public override void OnEnter()
		{
			base.OnEnter();
		}

		public override void Update()
		{
			base.Update();
			if (_BDown && _playerMap.Count == 0)
				TransitionTo<CharacterSelectionToMapTransition>();
			for (int i = 0; i < ReInput.players.playerCount; i++)
			{
				if (ReInput.players.GetPlayer(i).GetButtonDown("JoinGame"))
					_assignNextPlayer(i);
				if (ReInput.players.GetPlayer(i).GetButtonDown("Block"))
					_unassignPlayer(i);
			}

			foreach (PlayerMap _pm in _playerMap)
			{
				_controlCursor(_pm);
			}

			for (int i = 0; i < 6; i++)
			{
				if (Context._3rdMenuCursors[i].gameObject.activeSelf)
					_cursorCast(Context._3rdMenuCursors[i].position, i);
			}

			for (int i = 0; i < 6; i++)
			{
				_eggStateUpdate(i);
			}
		}

		private void _eggStateUpdate(int eggIndex)
		{
			/// If egg is activated && no cursor is on it
			/// Deactivate it
			Transform theEggChild = Context._eggs[eggIndex].GetChild(0);
			if (_eggState[eggIndex])
			{
				for (int i = 0; i < 6; i++)
				{
					if (_whoScannedEgg[eggIndex][i]) return;
				}
				/// Deactivation
				theEggChild.localScale = Vector3.one;
				theEggChild.GetComponent<Renderer>().material.SetColor("_OutlineColor", _MenuData.EggNormalOutlineColor);
				_eggState[eggIndex] = false;
				Context._3rdMenuCharacterImages[eggIndex].GetComponent<DOTweenAnimation>().DOPlayBackwards();
			}
			/// If egg is not activated && 1 or more cursor is on it
			/// Activate it
			else
			{
				for (int i = 0; i < 6; i++)
				{
					if (_whoScannedEgg[eggIndex][i])
					{
						/// Activation
						theEggChild.localScale = _MenuData.EggActivatedScale;
						theEggChild.GetComponent<Renderer>().material.SetColor("_OutlineColor", _MenuData.EggCursorOverOutlineColor);
						theEggChild.GetComponent<DOTweenAnimation>().DORestart();
						_eggState[eggIndex] = true;
						Context._3rdMenuCharacterImages[eggIndex].GetComponent<DOTweenAnimation>().DOPlayForward();
						return;
					}
				}
			}
		}

		private void _cursorCast(Vector3 pos, int _cursorIndex)
		{
			RaycastHit hit;
			Ray ray = _mainCamera.ScreenPointToRay(pos);

			/// If cursor Casted to a egg
			if (Physics.Raycast(ray, out hit, 100f, _MenuData.EggLayer))
			{
				/// If cursor's last casted target is not this egg, 
				/// Not that egg
				int siblingindex = hit.transform.GetSiblingIndex();
				if (_cursorPreviousScannedEgg[_cursorIndex] == siblingindex) return;
				if (_cursorPreviousScannedEgg[_cursorIndex] != -1 && _cursorPreviousScannedEgg[_cursorIndex] != siblingindex)
				{
					_whoScannedEgg[_cursorPreviousScannedEgg[_cursorIndex]][_cursorIndex] = false;
				}
				_whoScannedEgg[siblingindex][_cursorIndex] = true;
				_cursorPreviousScannedEgg[_cursorIndex] = siblingindex;
				// Show Grey image on hole
				for (int i = 0; i < 6; i++)
				{
					if (i != siblingindex)
						Context._3rdMenuHoleImages[_cursorIndex].GetChild(i).gameObject.SetActive(false);
				}
				Context._3rdMenuHoleImages[_cursorIndex].GetChild(siblingindex).gameObject.SetActive(true);
				Context._3rdMenuHoleImages[_cursorIndex].GetChild(siblingindex).GetComponent<Image>().color = _MenuData.HoverImageColor;
				// Also Change Hole Color to related color
				Context._3rdMenuHolders[_cursorIndex].GetComponent<Image>().color = _MenuData.HoleCursorveHoverColor[siblingindex];
				// Hide the indicators
				Context._3rdMenuIndicators[_cursorIndex].gameObject.SetActive(false);
			}
			else if (_cursorPreviousScannedEgg[_cursorIndex] != -1)
			{
				_whoScannedEgg[_cursorPreviousScannedEgg[_cursorIndex]][_cursorIndex] = false;
				_cursorPreviousScannedEgg[_cursorIndex] = -1;
				// Disable all grey images
				for (int i = 0; i < 6; i++)
				{
					Context._3rdMenuHoleImages[_cursorIndex].GetChild(i).gameObject.SetActive(false);
				}
				// Change Hole Image to normal
				Context._3rdMenuHolders[_cursorIndex].GetComponent<Image>().color = _MenuData.HoleNormalColor;
				// Show the indicators
				Context._3rdMenuIndicators[_cursorIndex].gameObject.SetActive(true);
			}
		}

		private void _controlCursor(PlayerMap playermap)
		{
			float HLAxis = ReInput.players.GetPlayer(playermap.RewiredPlayerID).GetAxis("Move Horizontal");
			float VLAxis = ReInput.players.GetPlayer(playermap.RewiredPlayerID).GetAxis("Move Vertical");
			Transform cursor = Context._3rdMenuCursors[playermap.GamePlayerID];
			cursor.localPosition += new Vector3(HLAxis, -VLAxis) * Time.deltaTime * _MenuData.CursorMoveSpeed;
		}

		private void _assignNextPlayer(int rewiredPlayerId)
		{
			if (_playerMap.Count >= 6) { return; }

			int gamePlayerId = _getNextGamePlayerId();
			if (gamePlayerId == 7) return;
			_playerMap.Add(new PlayerMap(rewiredPlayerId, gamePlayerId));

			Player rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerId);

			rewiredPlayer.controllers.maps.SetMapsEnabled(false, "Assignment");
			Context._3rdMenuCursors[gamePlayerId].gameObject.SetActive(true);
			Context._3rdMenuIndicators[gamePlayerId].gameObject.SetActive(true);
			Context._3rdMenuPrompts[gamePlayerId].gameObject.SetActive(false);
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
			/// Also need to Disable the Cursor Related Data
			if (_cursorPreviousScannedEgg[gamePlayerId] != -1)
				_whoScannedEgg[_cursorPreviousScannedEgg[gamePlayerId]][gamePlayerId] = false;
			_cursorPreviousScannedEgg[gamePlayerId] = -1;
			Context._3rdMenuCursors[gamePlayerId].localPosition = Context._3rdMenuCursorsOriginalLocalPosition[gamePlayerId];
			Context._3rdMenuCursors[gamePlayerId].gameObject.SetActive(false);
			Context._3rdMenuIndicators[gamePlayerId].gameObject.SetActive(false);
			Context._3rdMenuPrompts[gamePlayerId].gameObject.SetActive(true);

			ReInput.players.GetPlayer(rewiredPlayerId).controllers.maps.SetMapsEnabled(true, "Assignment");
			_playerMap.RemoveAt(playerMapIndex);
			_slotsTaken[gamePlayerId] = false;
		}

		private int _getNextGamePlayerId()
		{
			for (int i = 0; i < 6; i++)
			{
				if (!_slotsTaken[i])
				{
					_slotsTaken[i] = true;
					return i;
				}
			}
			return 7;
		}

		public override void OnExit()
		{
			base.OnExit();
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
	}

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
}
