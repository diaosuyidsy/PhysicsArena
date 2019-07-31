﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class GameStateManager
{
	private GameMapData _gameMapdata;
	private FSM<GameStateManager> _gameStateFSM;
	private PlayerInformation _playersInformation;
	private TextMeshProUGUI _holdAText;
	private Image _holdAImage;
	private Transform _playersHolder;
	private Transform[] _playersOutestHolder;

	public GameStateManager(GameMapData _gmp)
	{
		_gameMapdata = _gmp;
		_gameStateFSM = new FSM<GameStateManager>(this);
		//_holdAText = GameObject.Find("HoldCanvas").transform.Find("HoldA").GetComponent<TextMeshProUGUI>();
		//_holdAImage = GameObject.Find("HoldCanvas").transform.Find("HoldAImage").GetComponent<Image>();
		_playersInformation = DataSaver.loadData<PlayerInformation>("PlayersInformation");
		Debug.Assert(_playersInformation != null, "Unable to load Players information");
		_playersHolder = GameObject.Find("Players").transform;
		_playersOutestHolder = new Transform[6];
		for (int i = 0; i < 6; i++)
		{
			_playersOutestHolder[i] = _playersHolder.GetChild(i);
			Debug.Log(i + " th child's name is: " + _playersHolder.GetChild(i).name);
		}
		_gameStateFSM.TransitionTo<LandingState>();
	}

	public void Update()
	{
		_gameStateFSM.Update();
	}

	public void Destroy()
	{
		_gameStateFSM.CurrentState.CleanUp();
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

		protected GameMapData _GameMapData;

		public override void Init()
		{
			base.Init();
			_PlayersInformation = Context._playersInformation;
			_GameMapData = Context._gameMapdata;
		}
	}

	private class LandingState : GameState
	{
		private SpriteRenderer _titleInAir;
		private Camera _cam;

		public override void Init()
		{
			base.Init();
			_titleInAir = GameObject.Find("TitleInAir").GetComponent<SpriteRenderer>();
			_cam = Camera.main;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			Sequence seq = DOTween.Sequence();
			_titleInAir.DOFade(1f, _GameMapData.BirfiaTitalFadeInOutDuration).SetEase(_GameMapData.BirfiaTitleFadeInOutCurve).
				OnPlay(() => _cam.GetComponent<DOTweenAnimation>().DOPlayById("Move1"));
			for (int i = 0; i < _PlayersInformation.ColorIndex.Length; i++)
			{
				int playerIndex = _PlayersInformation.ColorIndex[i];
				Debug.Log(playerIndex);
				Context._playersOutestHolder[playerIndex].gameObject.SetActive(true);
			}
			// TODO: Find the first 2 chicken duck and fly them down, then others
			Context._playersOutestHolder[1].DOLocalMoveY(0.64f, _GameMapData.BirdsFlyDownDuration).SetDelay(_GameMapData.BirdsFlyDownDelay[0]).SetEase(_GameMapData.BirdsFlyDownEase).
				OnComplete(() => _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree"));
			Context._playersOutestHolder[3].DOLocalMoveY(0.64f, _GameMapData.BirdsFlyDownDuration).SetDelay(_GameMapData.BirdsFlyDownDelay[1]).SetEase(_GameMapData.BirdsFlyDownEase).
				OnComplete(() => _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree"));
			Context._playersOutestHolder[0].DOLocalMoveY(0.64f, _GameMapData.BirdsFlyDownDuration).SetDelay(_GameMapData.BirdsFlyDownDelay[2]).SetEase(_GameMapData.BirdsFlyDownEase).
				OnComplete(() => _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree"));
			Context._playersOutestHolder[4].DOLocalMoveY(0.64f, _GameMapData.BirdsFlyDownDuration).SetDelay(_GameMapData.BirdsFlyDownDelay[3]).SetEase(_GameMapData.BirdsFlyDownEase).
				OnComplete(() => _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree"));
			//seq.AppendInterval(0.5f);
			//seq.AppendCallback(() => _cam.GetComponent<DOTweenAnimation>().DOPlayById("Shake"));
			//seq.Append(_cam.transform.DOLocalRotate(new Vector3(45f, 0f, 0f), _GameMapData.CameraMoveDuration).SetEase(_GameMapData.CameraRotateCurve));
			//seq.Join(_cam.DOFieldOfView(30f, _GameMapData.CameraMoveDuration));
		}
	}

	private abstract class TutorialState : GameState
	{

	}

	private class FoodCartTutorialState : TutorialState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			//Context._holdAText.DOText("Hold  A  To Skip")
			//Sequence seq = DOTween.Sequence();
			//seq.Append(Context._tutorialTitle.DOText("Winning Condition", _GameMapData.TutorialTitleEnterDuration, true, _GameMapData.TutorialTitleScrambleMode));
			//seq.AppendInterval(_GameMapData.TutorialTitleAfterDelay);
			//seq.Append(Context._tutorialText.DOText(""))
		}
	}
}
