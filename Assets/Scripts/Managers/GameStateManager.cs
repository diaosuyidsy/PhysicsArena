using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class GameStateManager
{
	public PlayerController[] PlayerControllers;

	private GameMapData _gameMapdata;
	private FSM<GameStateManager> _gameStateFSM;
	private PlayerInformation _playersInformation;
	private TextMeshProUGUI _holdAText;
	private Image _holdAImage;
	private Transform _tutorialImage;
	private Transform _playersHolder;
	private Transform[] _playersOutestHolder;
	private TextMeshProUGUI _countDownText;
	private Camera _cam;
	private Vector3 _endFocusPosition;
	private DarkCornerEffect _darkCornerEffect;
	private Transform _gameEndCanvas;
	private GameObject _blackbackground;

	public GameStateManager(GameMapData _gmp)
	{
		_gameMapdata = _gmp;
		_gameStateFSM = new FSM<GameStateManager>(this);
		_gameEndCanvas = GameObject.Find("GameEndCanvas").transform;
		_blackbackground = _gameEndCanvas.Find("EndImageBackground").gameObject;
		_holdAText = GameObject.Find("HoldCanvas").transform.Find("HoldA").GetComponent<TextMeshProUGUI>();
		_holdAImage = GameObject.Find("HoldCanvas").transform.Find("HoldAImage").GetComponent<Image>();
		_playersInformation = DataSaver.loadData<PlayerInformation>("PlayersInformation");
		Debug.Assert(_playersInformation != null, "Unable to load Players information");
		_playersHolder = GameObject.Find("Players").transform;
		_tutorialImage = GameObject.Find("TutorialCanvas").transform.Find("TutorialImage");
		Debug.Assert(_tutorialImage != null);
		_playersOutestHolder = new Transform[6];
		_countDownText = GameObject.Find("TutorialCanvas").transform.Find("CountDown").GetComponent<TextMeshProUGUI>();
		PlayerControllers = new PlayerController[_playersInformation.ColorIndex.Length];
		for (int i = 0; i < 6; i++)
		{
			_playersOutestHolder[i] = _playersHolder.GetChild(i);
		}
		for (int i = 0; i < _playersInformation.ColorIndex.Length; i++)
		{
			PlayerControllers[i] = _playersOutestHolder[_playersInformation.ColorIndex[i]].GetComponentInChildren<PlayerController>(true);
		}
		EventManager.Instance.AddHandler<GameEnd>(_onGameEnd);
		_cam = Camera.main;
		_darkCornerEffect = _cam.GetComponent<DarkCornerEffect>();
		//_gameStateFSM.TransitionTo<LandingState>();
		_gameStateFSM.TransitionTo<FoodCartTutorialState>();
	}

	public void Update()
	{
		_gameStateFSM.Update();
	}

	public void Destroy()
	{
		EventManager.Instance.RemoveHandler<GameEnd>(_onGameEnd);
		if (_gameStateFSM.CurrentState != null)
			_gameStateFSM.CurrentState.CleanUp();
	}

	private void _onGameEnd(GameEnd ge)
	{
		if (!_gameStateFSM.CurrentState.GetType().Equals(typeof(WinState)))
		{
			_endFocusPosition = ge.WinnedObjective.position;
			_gameStateFSM.TransitionTo<WinState>();
			return;
		}
	}

	private abstract class GameState : FSM<GameStateManager>.State
	{
		protected PlayerInformation _PlayersInformation;
		protected bool _AnyADown
		{
			get
			{
				for (int i = 0; i < _PlayersInformation.RewiredID.Length; i++)
				{
					if (ReInput.players.GetPlayer(_PlayersInformation.RewiredID[i]).GetButtonDown("Jump")) return true;
				}
				return false;
			}
		}

		protected bool _AnyAHolding
		{
			get
			{
				for (int i = 0; i < _PlayersInformation.RewiredID.Length; i++)
				{
					if (ReInput.players.GetPlayer(_PlayersInformation.RewiredID[i]).GetButton("Jump")) return true;
				}
				return false;
			}
		}

		protected bool _AnyPauseDown
		{
			get
			{
				for (int i = 0; i < _PlayersInformation.RewiredID.Length; i++)
				{
					if (ReInput.players.GetPlayer(_PlayersInformation.RewiredID[i]).GetButtonDown("Pause")) return true;
				}
				return false;
			}
		}

		protected GameMapData _GameMapData;

		public override void Init()
		{
			base.Init();
			_PlayersInformation = Context._playersInformation;
			_GameMapData = Context._gameMapdata;
		}
	}

	private class StaticsticWorkState : GameState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			Context._blackbackground.SetActive(true);
		}
	}

	private class WinState : GameState
	{
		private Vector2 _targetPosition
		{
			get
			{
				Vector3 _temp = Camera.main.WorldToScreenPoint(Context._endFocusPosition);
				_temp.y = Screen.height - _temp.y;
				return _temp;
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
			Context._darkCornerEffect.CenterPosition = _targetPosition;

			float maxlength = Utility.GetMaxLengthToCorner(_targetPosition);
			Context._darkCornerEffect.enabled = true;
			Context._darkCornerEffect.Length = maxlength;
			float middlelength = maxlength * _GameMapData.DarkCornerMiddlePercentage;
			float finallength = maxlength * _GameMapData.DarkCornerFinalPercentage;

			Sequence seq = DOTween.Sequence();
			seq.Append(DOTween.To(() => Context._darkCornerEffect.Length, x => Context._darkCornerEffect.Length = x, middlelength, _GameMapData.DarkCornerToMiddleDuration));
			seq.AppendInterval(_GameMapData.DarkCornerMiddleStayDuration);
			seq.Append(DOTween.To(() => Context._darkCornerEffect.Length, x => Context._darkCornerEffect.Length = x, finallength, _GameMapData.DarkCornerToFinalDuration));
			seq.AppendCallback(() =>
			{
				Context._darkCornerEffect.enabled = false;
			});
		}

		public override void Update()
		{
			base.Update();
			if (Context._darkCornerEffect.enabled)
			{
				Context._darkCornerEffect.CenterPosition = _targetPosition;
			}
		}
	}

	private class GameLoop : GameState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			/// Resume Music
			Context._cam.GetComponent<AudioSource>().Play();
		}

		public override void Update()
		{
			base.Update();
			if (_AnyPauseDown)
			{
				TransitionTo<PauseState>();
				return;
			}
		}
	}

	private class PauseState : GameState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			Time.timeScale = 0f;
		}

		public override void Update()
		{
			base.Update();
			if (_AnyPauseDown)
			{
				TransitionTo<GameLoop>();
				return;
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			Time.timeScale = 1f;
		}
	}

	private class LandingState : GameState
	{
		private Camera _cam;

		public override void Init()
		{
			base.Init();
			_cam = Camera.main;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			int numOfPlayers = _PlayersInformation.ColorIndex.Length;
			Sequence seq = DOTween.Sequence();
			//_cam.transform.DOLocalMove(_GameMapData.CameraMoveToPosition, _GameMapData.CameraMoveDuration).SetDelay(_GameMapData.CameraMoveDelay).SetEase(_GameMapData.CameraMoveEase);
			_cam.DOFieldOfView(_GameMapData.CameraTargetFOV, _GameMapData.CameraMoveDuration).SetDelay(_GameMapData.CameraMoveDelay).SetEase(_GameMapData.CameraMoveEase);
			for (int i = 0; i < _PlayersInformation.ColorIndex.Length; i++)
			{
				int playerIndex = _PlayersInformation.ColorIndex[i];
				int temp = i;
				Context._playersOutestHolder[playerIndex].gameObject.SetActive(true);
				seq.Join(Context._playersOutestHolder[playerIndex].DOLocalMoveY(0.64f, _GameMapData.BirdsFlyDownDuration).SetDelay(_GameMapData.BirdsFlyDownDelay[temp]).SetEase(_GameMapData.BirdsFlyDownEase).
				OnComplete(() => _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree")));
			}
			seq.AppendInterval(_GameMapData.FightDelay);
			seq.Append(Context._countDownText.DOScale(_GameMapData.FightScale, _GameMapData.FightDuration).SetEase(_GameMapData.FightEase));
			seq.AppendInterval(_GameMapData.FightStayOnScreenDuration);
			seq.Append(Context._countDownText.DOScale(0f, 0.2f));
			seq.AppendCallback(() => TransitionTo<GameLoop>());
		}

		public override void OnExit()
		{
			base.OnExit();
			/// Need to Enable everything that game loop has
			/// 1. Enable PlayerController, set rigidbody kinematic = false
			/// TODO: Color the players
			/// 2. Enable Camera
			/// 3. Send Game Start Event
			for (int i = 0; i < _PlayersInformation.ColorIndex.Length; i++)
			{
				int playerindex = _PlayersInformation.ColorIndex[i];
				int rewiredid = _PlayersInformation.RewiredID[i];
				PlayerController playercontroller = Context._playersOutestHolder[playerindex].GetComponentInChildren<PlayerController>(true);
				playercontroller.enabled = true;
				playercontroller.Init(rewiredid);
				playercontroller.GetComponent<Rigidbody>().isKinematic = false;
			}
			Context._cam.GetComponent<CameraController>().enabled = true;
			EventManager.Instance.TriggerEvent(new GameStart());
		}
	}

	private abstract class TutorialState : GameState
	{

	}

	private class FoodCartTutorialState : TutorialState
	{
		private bool _canHoldA;

		public override void OnEnter()
		{
			base.OnEnter();
			Context._tutorialImage.DOScale(Vector3.one, _GameMapData.TutorialImageMoveInDuration).SetEase(_GameMapData.TutorialImageMoveInEase);
			Context._holdAText.DOText("Hold  A  To Start", _GameMapData.HoldAMoveInDuration).SetDelay(_GameMapData.HoldAMoveInDelay).OnComplete(() => _canHoldA = true);
		}

		public override void Update()
		{
			base.Update();
			if (_canHoldA && _AnyAHolding)
			{
				Context._holdAImage.fillAmount += Time.deltaTime * _GameMapData.FillASpeed;
				if (Context._holdAImage.fillAmount >= 1f)
				{
					TransitionTo<LandingState>();
					return;
				}
			}
			else
			{
				Context._holdAImage.fillAmount -= Time.deltaTime * _GameMapData.FillASpeed;
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			Context._tutorialImage.DOScale(Vector3.zero, _GameMapData.TutorialImageMoveInDuration).SetEase(_GameMapData.TutorialImageMoveInEase);
			Context._holdAText.DOText("", 0f);
			Context._holdAImage.transform.DOScale(0f, 0f).SetEase(Ease.OutQuad);
		}
	}
}
