﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[HideInInspector]
	public Vector3 FollowTarget;

	public CameraData CameraData;

	private Vector3 _winFocusPosition;
	private Camera _cam;
	private FSM<CameraController> _camFSM;

	private void Awake()
	{
		_cam = GetComponent<Camera>();
		_camFSM = new FSM<CameraController>(this);
		_camFSM.TransitionTo<EntryState>();
	}

	// Update is called once per frame
	void Update()
	{
		_camFSM.Update();
	}

	private abstract class CameraState : FSM<CameraController>.State
	{
		protected CameraData _CameraData;
		public override void Init()
		{
			base.Init();
			_CameraData = Context.CameraData;
		}
	}

	private class EntryState : CameraState
	{
	}

	private class AllDeadState : CameraState
	{
	}

	private class TrackingState : CameraState
	{
		private Vector3 _targetOnGround;
		private Vector3 _desiredPosition;

		public override void Update()
		{
			base.Update();
			//_setCameraPosition();
			_setCameraFOV();
		}

		private void _setCameraFOV()
		{
			Vector3 total = Vector3.zero;
			int length = 0;

			foreach (PlayerController go in Services.GameStateManager.PlayerControllers)
			{
				if (go != null && go.LegSwingReference.activeSelf)
				{
					total += go.transform.position;
					length++;
				}
			}
			total /= (length == 0 ? 1 : length);
			_targetOnGround = total;
			Context.FollowTarget = _targetOnGround;
			_desiredPosition.x = _targetOnGround.x;
			_desiredPosition.y = _targetOnGround.y + Mathf.Sin(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * _CameraData.CameraDistance;
			_desiredPosition.z = _targetOnGround.z - Mathf.Cos(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * _CameraData.CameraDistance;
			Context.transform.position = Vector3.Lerp(Context.transform.position, _desiredPosition, _CameraData.SmoothSpeed);

			float _desiredFOV = 2f * _getMaxDistance() + 3.99f;
			Context._cam.fieldOfView = Mathf.Lerp(Context._cam.fieldOfView, _desiredFOV, _CameraData.SmoothSpeed);
			Context._cam.fieldOfView = Mathf.Clamp(Context._cam.fieldOfView, _CameraData.FOVSizeMin, _CameraData.FOVSizeMax);
		}

		private void _setCameraPosition()
		{
			Vector3 total = Vector3.zero;
			int length = 0;

			foreach (PlayerController go in Services.GameStateManager.PlayerControllers)
			{
				if (go != null && go.LegSwingReference.activeSelf)
				{
					total += go.transform.position;
					length++;
				}
			}
			total /= (length == 0 ? 1 : length);
			_targetOnGround = total;
			float maxDistBetweenPlayers = 2f * _getMaxDistance() + 4f;
			maxDistBetweenPlayers = Mathf.Clamp(maxDistBetweenPlayers, _CameraData.MinDistance, _CameraData.MaxDistance);
			_desiredPosition.x = _targetOnGround.x;
			_desiredPosition.y = _targetOnGround.y + Mathf.Sin(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * maxDistBetweenPlayers;
			_desiredPosition.z = _targetOnGround.z - Mathf.Cos(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * maxDistBetweenPlayers;

			Context.transform.position = Vector3.Lerp(Context.transform.position, _desiredPosition, _CameraData.SmoothSpeed);
		}

		// Should get the max distance between any two players
		private float _getMaxDistance()
		{
			float maxDist = 0f;
			for (int i = 0; i < Services.GameStateManager.PlayerControllers.Length; i++)
			{
				if (!Services.GameStateManager.PlayerControllers[i].LegSwingReference.activeSelf)
					continue;
				for (int j = i + 1; j < Services.GameStateManager.PlayerControllers.Length; j++)
				{
					if (!Services.GameStateManager.PlayerControllers[j].LegSwingReference.activeSelf)
						continue;
					float temp = Vector3.Distance(Services.GameStateManager.PlayerControllers[i].transform.position, Services.GameStateManager.PlayerControllers[j].transform.position);
					if (temp > maxDist)
					{
						maxDist = temp;
					}
				}
			}
			return maxDist;
		}
	}

	private class WinState : CameraState
	{
		private Vector3 _desiredPosition;

		public override void Update()
		{
			base.Update();
			_desiredPosition.x = Context._winFocusPosition.x;
			_desiredPosition.y = Context._winFocusPosition.y + Mathf.Sin(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * _CameraData.CameraDistance;
			_desiredPosition.z = Context._winFocusPosition.z - Mathf.Cos(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * _CameraData.CameraDistance;
			Context.transform.position = Vector3.Lerp(Context.transform.position, _desiredPosition, _CameraData.SmoothSpeed);
			Context._cam.fieldOfView = Mathf.Lerp(Context._cam.fieldOfView, _CameraData.WonFOVSize, _CameraData.SmoothSpeed);
		}
	}

	private void _onGameStart(GameStart gs)
	{
		_camFSM.TransitionTo<TrackingState>();
	}

	private void _onPlayerDead(PlayerDied pd)
	{
		/// If all players are dead, transition to all dead state
		for (int i = 0; i < Services.GameStateManager.PlayerControllers.Length; i++)
		{
			if (Services.GameStateManager.PlayerControllers[i].LegSwingReference.activeSelf
				&& Services.GameStateManager.PlayerControllers[i] != pd.Player.GetComponent<PlayerController>()) return;
		}
		if (_camFSM.CurrentState.GetType().Equals(typeof(TrackingState))) _camFSM.TransitionTo<AllDeadState>();
	}

	private void _onPlayerRespawn(PlayerRespawned pr)
	{
		/// If a player respawn and they are in all dead state, transition to tracking state
		if (_camFSM.CurrentState.GetType().Equals(typeof(AllDeadState))) _camFSM.TransitionTo<TrackingState>();
	}

	private void _onGameWon(GameEnd ge)
	{
		if (!_camFSM.CurrentState.GetType().Equals(typeof(AllDeadState)) &&
			!_camFSM.CurrentState.GetType().Equals(typeof(TrackingState))) return;
		_winFocusPosition = new Vector3(ge.WinnedObjective.position.x, ge.WinnedObjective.position.y, ge.WinnedObjective.position.z);
		_camFSM.TransitionTo<WinState>();
		return;
	}

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<GameStart>(_onGameStart);
		EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDead);
		EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawn);
		EventManager.Instance.AddHandler<GameEnd>(_onGameWon);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<GameStart>(_onGameStart);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDead);
		EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawn);
		EventManager.Instance.RemoveHandler<GameEnd>(_onGameWon);
	}
}
