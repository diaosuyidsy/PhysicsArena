using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using DG.Tweening;

public class GameStateManager
{
	private GameMapData _gameMapdata;
	private FSM<GameStateManager> _gameStateFSM;
	private PlayerInformation _playersInformation;
	private TextMeshProUGUI _tutorialTitle;
	private TextMeshProUGUI _tutorialText;

	public GameStateManager(GameMapData _gmp)
	{
		_gameMapdata = _gmp;
		_gameStateFSM = new FSM<GameStateManager>(this);
		_playersInformation = DataSaver.loadData<PlayerInformation>("PlayersInformation");
		Debug.Assert(_playersInformation != null, "Unable to load Players information");
		switch (_gameMapdata.GameMapMode)
		{
			case GameMapMode.FoodCartMode:
				_gameStateFSM.TransitionTo<FoodCartTutorialState>();
				break;
		}
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

	private abstract class TutorialState : GameState
	{

	}

	private class FoodCartTutorialState : TutorialState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			Sequence seq = DOTween.Sequence();
			seq.Append(Context._tutorialTitle.DOText("Winning Condition", _GameMapData.TutorialTitleEnterDuration, true, _GameMapData.TutorialTitleScrambleMode));
			seq.AppendInterval(_GameMapData.TutorialTitleAfterDelay);
			//seq.Append(Context._tutorialText.DOText(""))
		}
	}
}
