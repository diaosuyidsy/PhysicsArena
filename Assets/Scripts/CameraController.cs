using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[HideInInspector]
	public Vector3 FollowTarget;

	public CameraData CameraData;

	private float _maxDistanceOrigin;
	private float _xDiffOrigin;
	private float _zDiffOrigin;
	private Vector3 _desiredPosition;
	private Vector3 _smoothedPosition;
	private float _desiredFOV;
	private float _smoothedFOV;
	private bool _winLock = false;
	private float transformY;

	private FSM<CameraController> _camFSM;

	private void Awake()
	{
		_camFSM = new FSM<CameraController>(this);
		_camFSM.TransitionTo<EntryState>();
	}
	// Use this for initialization
	//void Start()
	//{
	//	SetTarget(false);
	//	// Set the max Distance originally
	//	float maxDist = 0f;
	//	foreach (PlayerController go in Services.GameStateManager.PlayerControllers)
	//	{
	//		if (go == null) continue;
	//		float temp = Vector3.Distance(go.transform.position, FollowTarget);
	//		if (temp > maxDist)
	//		{
	//			maxDist = temp;
	//		}
	//	}
	//	_maxDistanceOrigin = maxDist;
	//	// Set X and Z Original Difference
	//	//_xDiffOrigin = transform.position.x - FollowTarget.x;
	//	_zDiffOrigin = transform.position.z - Services.GameStateManager.PlayerControllers[0].transform.position.z;
	//	_xDiffOrigin = 0f;
	//	transformY = transform.position.y;
	//	//iffOrigin = 0f;
	//}

	// Update is called once per frame
	void Update()
	{
		_camFSM.Update();
		// If player won, then do Won Logic and lock others
		//if (_winLock)
		//{
		//	_desiredPosition = new Vector3(FollowTarget.x + _xDiffOrigin, transformY, FollowTarget.z + _zDiffOrigin);
		//	_smoothedPosition = Vector3.Lerp(transform.position, _desiredPosition, CameraData.SmoothSpeed);
		//	transform.position = _smoothedPosition;
		//	GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, CameraData.WonFOVSize, CameraData.SmoothSpeed);
		//	return;
		//}
		//SetTarget();
		//if (FollowTarget == Vector3.zero || FollowTarget == new Vector3(CameraData.XOffset, 0f, CameraData.ZOffset))
		//{
		//	return;
		//}
		//_desiredPosition = new Vector3(FollowTarget.x + _xDiffOrigin, transformY, FollowTarget.z + _zDiffOrigin);
		//_smoothedPosition = Vector3.Lerp(transform.position, _desiredPosition, CameraData.SmoothSpeed);
		//transform.position = _smoothedPosition;
		////GetComponent<Camera> ().fieldOfView += (MaxDistance () - _maxDistanceOrigin) * CameraScaleSpeed;
		////_maxDistanceOrigin = MaxDistance ();
		//_desiredFOV = 2f * MaxDistance() + 3.99f;
		//GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, _desiredFOV, CameraData.SmoothSpeed);
		//GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView, CameraData.FOVSizeMin, CameraData.FOVSizeMax);
	}

	public void OnWinCameraZoom(Transform tar)
	{
		_winLock = true;
		FollowTarget = tar.position;
	}

	private abstract class CameraState : FSM<CameraController>.State
	{
		protected CameraData _CameraData;
		public override void Init()
		{
			base.Init();
			_CameraData = Context.CameraData;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			print(GetType().Name);
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

		public override void OnEnter()
		{
			base.OnEnter();
		}

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
			_desiredPosition.x = _targetOnGround.x;
			_desiredPosition.y = _targetOnGround.y + Mathf.Sin(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * _CameraData.CameraDistance;
			_desiredPosition.z = _targetOnGround.z - Mathf.Cos(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * _CameraData.CameraDistance;
			Context.transform.position = Vector3.Lerp(Context.transform.position, _desiredPosition, _CameraData.SmoothSpeed);

			float _desiredFOV = 2f * _getMaxDistance() + 3.99f;
			Context.GetComponent<Camera>().fieldOfView = Mathf.Lerp(Context.GetComponent<Camera>().fieldOfView, _desiredFOV, _CameraData.SmoothSpeed);
			Context.GetComponent<Camera>().fieldOfView = Mathf.Clamp(Context.GetComponent<Camera>().fieldOfView, _CameraData.FOVSizeMin, _CameraData.FOVSizeMax);
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

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<GameStart>(_onGameStart);
		EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDead);
		EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawn);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<GameStart>(_onGameStart);
		EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDead);
		EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawn);
	}
}
