using System.Collections;
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
    private Vector3 _minPosition;
    private Vector3 _maxPosition;
    private List<CameraTargets> _cameraTargets;

    private void Awake()
    {
        _minPosition = transform.position - Services.GameStateManager._gameMapdata.CameraClampRelativePosition;
        _maxPosition = transform.position + Services.GameStateManager._gameMapdata.CameraClampRelativePosition;
        _cam = GetComponent<Camera>();
        _cameraTargets = new List<CameraTargets>();
        _camFSM = new FSM<CameraController>(this);
        _camFSM.TransitionTo<EntryState>();
    }

    private void Start()
    {
        GameObject[] team1players = GameObject.FindGameObjectsWithTag("Team1");
        GameObject[] team2players = GameObject.FindGameObjectsWithTag("Team2");
        foreach (GameObject t in team1players)
        {
            EventManager.Instance.TriggerEvent(new OnAddCameraTargets(t, 1));
        }
        foreach (GameObject t in team2players)
        {
            EventManager.Instance.TriggerEvent(new OnAddCameraTargets(t, 1));
        }
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

            foreach (CameraTargets ct in Context._cameraTargets)
            {
                Transform go = ct.Target.transform;
                PlayerController pc = go.GetComponent<PlayerController>();
                if ((pc != null
                    && pc.OnDeathHidden[0].activeSelf)
                    || (pc == null))
                {
                    total += (go.position * ct.Weight);
                    length += ct.Weight;
                }
            }
            total /= (length == 0 ? 1 : length);
            _targetOnGround = total;
            Context.FollowTarget = _targetOnGround;
            _desiredPosition.x = _targetOnGround.x;
            _desiredPosition.y = _targetOnGround.y + Mathf.Sin(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * _CameraData.CameraDistance;
            _desiredPosition.z = _targetOnGround.z - Mathf.Cos(Context.transform.localEulerAngles.x * Mathf.Deg2Rad) * _CameraData.CameraDistance;
            // _desiredPosition.x = Mathf.Clamp(_desiredPosition.x, Context._minPosition.x, Context._maxPosition.x);
            // _desiredPosition.y = Mathf.Clamp(_desiredPosition.y, Context._minPosition.y, Context._maxPosition.y);
            // _desiredPosition.z = Mathf.Clamp(_desiredPosition.z, Context._minPosition.z, Context._maxPosition.z);
            Context.transform.position = Vector3.Lerp(Context.transform.position, _desiredPosition, _CameraData.SmoothSpeed);

            float _desiredFOV = 2f * _getMaxDistance() + 5f;
            Context._cam.fieldOfView = Mathf.Lerp(Context._cam.fieldOfView, _desiredFOV, _CameraData.SmoothSpeed);
            Context._cam.fieldOfView = Mathf.Clamp(Context._cam.fieldOfView, _CameraData.FOVSizeMin, _CameraData.FOVSizeMax);
        }

        // Should get the max distance between any two players
        private float _getMaxDistance()
        {
            float maxDist = 0f;
            for (int i = 0; i < Context._cameraTargets.Count; i++)
            {
                for (int j = i + 1; j < Context._cameraTargets.Count; j++)
                {
                    float temp = Vector3.Distance(Context._cameraTargets[i].Target.transform.position, Context._cameraTargets[j].Target.transform.position);
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
        EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(pd.Player));
        /// If all players are dead, transition to all dead state
        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            if (_cameraTargets[i].Target.GetComponent<PlayerController>() != null) return;
        }
        if (_camFSM.CurrentState.GetType().Equals(typeof(TrackingState))) _camFSM.TransitionTo<AllDeadState>();
    }

    private void _onPlayerRespawn(PlayerRespawned pr)
    {
        EventManager.Instance.TriggerEvent(new OnAddCameraTargets(pr.Player, 1));
        /// If a player respawn and they are in all dead state, transition to tracking state
        if (_camFSM.CurrentState.GetType().Equals(typeof(AllDeadState))) _camFSM.TransitionTo<TrackingState>();
    }

    private void _onGameWon(GameEnd ge)
    {
        if (!_camFSM.CurrentState.GetType().Equals(typeof(AllDeadState)) &&
            !_camFSM.CurrentState.GetType().Equals(typeof(TrackingState))) return;
        if (ge.WinnedObjective != null)
            _winFocusPosition = new Vector3(ge.WinnedObjective.position.x, ge.WinnedObjective.position.y, ge.WinnedObjective.position.z);
        else _winFocusPosition = ge.WinnedPosition;
        _camFSM.TransitionTo<WinState>();
        return;
    }

    private void _onAddCameraTarget(OnAddCameraTargets ev)
    {
        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            if (_cameraTargets[i].Target == ev.Target)
            {
                _cameraTargets[i].Weight = ev.Weight;
                return;
            }
        }
        // Else it's a new Target
        _cameraTargets.Add(new CameraTargets(ev.Target, ev.Weight));
    }

    private void _onRemoveCameraTarget(OnRemoveCameraTargets ev)
    {
        int index = -1;
        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            if (_cameraTargets[i].Target == ev.Target)
            {
                index = i;
                break;
            }
        }
        if (index != -1)
            _cameraTargets.RemoveAt(index);
    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<GameStart>(_onGameStart);
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDead);
        EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawn);
        EventManager.Instance.AddHandler<GameEnd>(_onGameWon);
        EventManager.Instance.AddHandler<OnAddCameraTargets>(_onAddCameraTarget);
        EventManager.Instance.AddHandler<OnRemoveCameraTargets>(_onRemoveCameraTarget);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<GameStart>(_onGameStart);
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDead);
        EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawn);
        EventManager.Instance.RemoveHandler<GameEnd>(_onGameWon);
        EventManager.Instance.RemoveHandler<OnAddCameraTargets>(_onAddCameraTarget);
        EventManager.Instance.RemoveHandler<OnRemoveCameraTargets>(_onRemoveCameraTarget);
    }
}
