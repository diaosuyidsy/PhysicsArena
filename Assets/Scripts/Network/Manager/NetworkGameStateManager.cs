using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using CharTween;
using UnityEngine.SceneManagement;
using TextFx;
using System;
using Mirror;

public class NetworkGameStateManager : NetworkBehaviour
{
    public GameMapData _gameMapdata;
    public ConfigData _configData;
    private FSM<NetworkGameStateManager> _gameStateFSM;
    private TextMeshProUGUI _holdAText;
    private Image _holdAImage;
    private TextMeshProUGUI _holdBText;
    private Image _holdBImage;
    private TextMeshProUGUI _holdYText;
    private Image _holdYImage;
    private Transform _tutorialImage;
    private Transform _tutorialBackgroundMask;
    private Transform _tutorialObjectiveImages;
    private Transform _playersHolder;

    private TextMeshProUGUI _countDownText;
    private Camera _cam;
    private Vector3 _endFocusPosition;
    private Transform _gameEndCanvas;
    private GameObject _gameEndBlackbackground;
    private Transform _gameEndTitleText;
    private Transform _pauseMenu;
    private Transform _pauseBackgroundMask;
    private Transform _pausePausedText;
    private Transform _pauseResume;
    private Transform _pauseQuit;
    private Transform _pauseWholeMask;
    private Transform _statisticPanel;
    private Transform _statisticUIHolder;
    private Transform _MVPDisplay;
    private Transform _MVPTitle;
    private Transform _MVP;
    private Transform _MVPCamera;
    private Transform _MVPPlayerHolder;
    private Transform _MVPSpotLight;
    private Transform _MVPPodium;
    private Transform _gameUI;
    private int _winner;
    private int _hitStopFrames;
    private float _hitStopTimeScale;
    private GameObject _gameManager;


    public void Awake()
    {
        _gameManager = GetComponent<NetworkGame>().gameObject;
        _gameUI = GameObject.Find("GameUI").transform;
        _gameEndCanvas = GameObject.Find("GameEndCanvas").transform;
        _gameEndBlackbackground = _gameEndCanvas.Find("EndImageBackground").gameObject;
        _gameEndTitleText = _gameEndCanvas.Find("TitleText");
        _holdAText = GameObject.Find("HoldCanvas").transform.Find("HoldA").GetComponent<TextMeshProUGUI>();
        _holdAImage = GameObject.Find("HoldCanvas").transform.Find("HoldAImage").GetComponent<Image>();
        _holdBText = GameObject.Find("HoldCanvas").transform.Find("HoldB").GetComponent<TextMeshProUGUI>();
        _holdBImage = GameObject.Find("HoldCanvas").transform.Find("HoldBImage").GetComponent<Image>();
        _holdYText = GameObject.Find("HoldCanvas").transform.Find("HoldY").GetComponent<TextMeshProUGUI>();
        _holdYImage = GameObject.Find("HoldCanvas").transform.Find("HoldYImage").GetComponent<Image>();
        _playersHolder = GameObject.Find("Players").transform;
        _tutorialImage = GameObject.Find("TutorialCanvas").transform.Find("TutorialImage");
        _tutorialBackgroundMask = GameObject.Find("TutorialCanvas").transform.Find("BackgroundMask");
        _tutorialObjectiveImages = GameObject.Find("TutorialCanvas").transform.Find("ObjectiveImages");
        Debug.Assert(_tutorialImage != null);
        _countDownText = GameObject.Find("TutorialCanvas").transform.Find("CountDown").GetComponent<TextMeshProUGUI>();
        _pauseMenu = GameObject.Find("PauseMenu").transform;
        _pauseBackgroundMask = _pauseMenu.Find("BackgroundMask");
        _pausePausedText = _pauseMenu.Find("Paused");
        _pauseResume = _pauseMenu.Find("Resume");
        _pauseQuit = _pauseMenu.Find("Quit");
        _pauseWholeMask = _pauseMenu.Find("WholeMask");
        _statisticPanel = _gameEndCanvas.Find("StatisticPanel");
        _MVPDisplay = _statisticPanel.Find("MVPDisplay");
        _MVPTitle = _statisticPanel.Find("MVPTitle");
        _statisticUIHolder = _statisticPanel.Find("StatisticUIHolder");
        _MVP = GameObject.Find("MVP").transform;
        _MVPCamera = _MVP.Find("MVP Camera");
        _MVPPlayerHolder = _MVP.Find("MVPPlayerHolder");
        _MVPSpotLight = _MVP.Find("MVPSpotLight");
        _MVPPodium = _MVP.Find("MVPPodium");
        _cam = Camera.main;
        // _gameStateFSM.TransitionTo<EmptyState>();
    }

    public void OnStart()
    {
        _gameStateFSM = new FSM<NetworkGameStateManager>(this);
        _gameStateFSM.TransitionTo<LandingState>();
    }

    public void Update()
    {
        if (isServer && _gameStateFSM != null)
            _gameStateFSM.Update();
    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<GameEnd>(_onGameEnd);
        EventManager.Instance.AddHandler<HitStopEvent>(_onHitStop);
    }

    public void OnDisable()
    {
        EventManager.Instance.RemoveHandler<GameEnd>(_onGameEnd);
        EventManager.Instance.RemoveHandler<HitStopEvent>(_onHitStop);
        if (_gameStateFSM != null && _gameStateFSM.CurrentState != null)
            _gameStateFSM.CurrentState.CleanUp();
    }

    private void _onHitStop(HitStopEvent ev)
    {
        _hitStopFrames = ev.StopFrames;
        _hitStopTimeScale = ev.TimeScale;
        if (_gameStateFSM.CurrentState.GetType().Equals(typeof(GameLoop)))
        {
            _gameStateFSM.TransitionTo<HitStop>();
        }
    }

    private void _onGameEnd(GameEnd ge)
    {
        if (_gameStateFSM != null && _gameStateFSM.CurrentState.GetType().Equals(typeof(GameLoop)))
        {
            if (ge.WinnedObjective != null)
                _endFocusPosition = ge.WinnedObjective.position;
            else _endFocusPosition = ge.WinnedPosition;
            _winner = ge.Winner;
            _gameStateFSM.TransitionTo<WinState>();
            return;
        }
    }


    private abstract class GameState : FSM<NetworkGameStateManager>.State
    {
        protected bool _AnyADown
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetButtonDown("Jump");
            }
        }
        protected bool _AnyBDown
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetButtonDown("Block");

            }
        }

        protected bool _AnyAHolding
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetButton("Jump");
            }
        }

        protected bool _AnyBHolding
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetButton("Block");
            }
        }

        protected bool _AnyYHolding
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetButton("Left Trigger");
            }
        }

        protected bool _AnyPauseDown
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetButtonDown("Pause");
            }
        }

        protected float _AnyVLAxisRaw
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetAxisRaw("Move Vertical");
            }
        }

        protected bool _vAxisInUse = true;

        protected GameMapData _GameMapData;

        public override void Init()
        {
            base.Init();
            _GameMapData = Context._gameMapdata;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _vAxisInUse = true;
        }

        public override void Update()
        {
            base.Update();
            if (_AnyVLAxisRaw == 0f) _vAxisInUse = false;
        }
    }

    private class GameQuitState : GameState
    {

    }

    private abstract class StatisticsWordState : GameState
    {
        protected ConfigData _configData;

        public override void Init()
        {
            base.Init();
            _configData = Context._configData;
        }
    }

    private class MVPEndPanelState : StatisticsWordState
    {
        private bool _canHold = false;
        private bool _Apressed;
        private bool _Bpressed;
        private bool _Ypressed;
        public override void OnEnter()
        {
            base.OnEnter();
            EventManager.Instance.AddHandler<ButtonPressed>(_onButtonPressed);
            Camera.main.GetComponent<NetworkDarkCornerEffect>().enabled = false;
            Context.RpcMVP();
            for (int i = 0; i < Context._playersHolder.childCount; i++)
            {
                PlayerControllerMirror playercontroller = Context._playersHolder.GetChild(i).GetComponentInChildren<PlayerControllerMirror>(true);
                playercontroller.enabled = false;
                playercontroller.GetComponent<Rigidbody>().isKinematic = false;
            }
            Context._MVPDisplay.gameObject.SetActive(true);
            Context._MVPCamera.gameObject.SetActive(true);
            Context._MVPPlayerHolder.gameObject.SetActive(true);
            Context._MVPSpotLight.gameObject.SetActive(true);
            Context._MVPPodium.gameObject.SetActive(true);
            // int MVPColorIndex = Context.GetColorIndexFromRewiredID(NetworkServices.StatisticsManager.GetMVPRewiredID());
            int MVPColorIndex = 0;
            Transform MVPChicken = Context._MVPPlayerHolder.GetChild(MVPColorIndex);
            MVPChicken.gameObject.SetActive(true);
            MVPChicken.GetComponent<Animator>().SetTrigger("Enter");
            Color MVPColor = _configData.IndexToColor[MVPColorIndex];
            MVPColor.a = 0f;
            Context._MVPTitle.GetComponent<TextMeshProUGUI>().color = MVPColor;
            Sequence seq = DOTween.Sequence();
            seq.Append(Context._MVPPodium.DOLocalMoveY(0.7f, _configData.MVPPodiumMoveDuration).SetEase(_configData.MVPPodiumMoveEase).SetRelative(true)
                .OnPlay(() => Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPLandRiseAudioClip)));
            seq.Append(Context._MVPSpotLight.GetComponent<Light>().DOIntensity(_configData.MVPSpotLightIntensity, _configData.MVPSpotLightDuration).SetEase(_configData.MVPSpotLightEase).SetLoops(3, LoopType.Yoyo)
            .OnPlay(() => Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPLightFlickerAudioClip)));
            seq.AppendInterval(_configData.MVPSpotLightToLandDuration);
            seq.Append(MVPChicken.DOLocalMoveY(-3.116f, 0.2f).SetEase(Ease.InCirc).SetRelative(true)
            .OnComplete(() =>
            {
                Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPBirdLandAudioClip);
                Context._MVPCamera.GetComponent<DOTweenAnimation>().DORestartAllById("Land");
                MVPChicken.GetComponent<Animator>().SetTrigger("Pose");
            }));
            seq.AppendInterval(_configData.MVPLandToWordShowDuration);
            seq.Append(Context._MVPTitle.DOScale(5f, 0.2f).From().SetEase(Ease.OutCirc).SetRelative(true).OnComplete(() => Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPBadgeStampAudioClip)));
            seq.Join(Context._MVPTitle.GetComponent<TextMeshProUGUI>().DOFade(1f, 0.2f));
            seq.AppendInterval(_configData.MVPToUIMoveInDuration);
            seq.Append(Context._MVPPlayerHolder.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
            seq.Join(Context._MVPSpotLight.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
            seq.Join(Context._MVPPodium.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
            seq.Join(Context._MVPTitle.DOLocalMove(new Vector3(-587f, 187f), _configData.MVPScaleDownDuration).SetEase(Ease.Linear));
            seq.Join(Context._MVPTitle.DOScale(0.6f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear));
            // var statsresult = NetworkServices.StatisticsManager.GetStatisticResult();
            /// Move in all players 
            GameObject.Instantiate(_configData.MVPBadgePrefab, Context._statisticUIHolder.GetChild(MVPColorIndex));
            for (int i = 0; i < Context._playersHolder.childCount; i++)
            {
                int colorindex = Utility.GetColorIndexFromPlayer(Context._playersHolder.GetChild(i).gameObject);
                Transform frame = Context._statisticUIHolder.GetChild(colorindex);
                // frame.GetChild(0).GetComponent<Image>().sprite = statsresult[rewiredID].StatisticIcon;
                // frame.GetChild(1).GetComponent<TextMeshProUGUI>().text = statsresult[rewiredID].StatisticName;
                // frame.GetChild(2).GetComponent<TextMeshProUGUI>().text = statsresult[rewiredID].StatisticsInformation;
            }
            // int[] ci = _PlayersInformation.ColorIndex;
            // Array.Sort(ci);
            // for (int i = 0; i < ci.Length; i++)
            // {
            //     int x = _PlayersInformation.ColorIndex[i];
            //     Context._statisticUIHolder.GetChild(x).DOLocalMoveY(_configData.FrameYPosition[i], 0f);
            //     // seq.Append(Context._statisticUIHolder.GetChild(x).DOLocalMoveX(770f, _configData.FrameMoveInDuration)
            //     seq.Append(Context._statisticUIHolder.GetChild(x).DOScale(0.7f, _configData.FrameMoveInDuration)
            //     .SetEase(Ease.OutBack)
            //     .OnPlay(() => Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPStatisticPanelBopClip)));
            // }
            seq.Append(Context._holdAText.DOText("Next Map", 0.2f).OnPlay(() => Context._holdAText.gameObject.SetActive(true)));
            seq.Join(Context._holdBText.DOText("Menu", 0.2f).OnPlay(() => Context._holdBText.gameObject.SetActive(true)));
            seq.Join(Context._holdYText.DOText("Replay", 0.2f).OnPlay(() => Context._holdYText.gameObject.SetActive(true)));
            seq.AppendCallback(() => _canHold = true);
        }

        private void _onButtonPressed(ButtonPressed ev)
        {
            if (ev.ButtonName == "Jump")
                _Apressed = true;
            if (ev.ButtonName == "Block")
                _Bpressed = true;
            if (ev.ButtonName == "Left Trigger")
                _Ypressed = true;
        }

        public override void Update()
        {
            base.Update();
            if (_canHold && _Apressed)
            {
                if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
                    NetworkManager.singleton.ServerChangeScene(Utility.GetNextSceneName());
                else
                    NetworkManager.singleton.ServerChangeScene(Utility.GetSceneNameByBuildIndex(1));
            }

            if (_canHold && _Bpressed)
            {
                NetworkManager.singleton.ServerChangeScene("OnlineMenu");
            }

            if (_canHold && _Ypressed)
            {
                NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            EventManager.Instance.RemoveHandler<ButtonPressed>(_onButtonPressed);
        }
    }

    [ClientRpc]
    private void RpcMVP()
    {
        Camera.main.GetComponent<NetworkDarkCornerEffect>().enabled = false;
        for (int i = 0; i < _playersHolder.childCount; i++)
        {
            PlayerControllerMirror playercontroller = _playersHolder.GetChild(i).GetComponentInChildren<PlayerControllerMirror>(true);
            playercontroller.enabled = false;
            playercontroller.GetComponent<Rigidbody>().isKinematic = false;
        }
        _MVPDisplay.gameObject.SetActive(true);
        _MVPCamera.gameObject.SetActive(true);
        _MVPPlayerHolder.gameObject.SetActive(true);
        _MVPSpotLight.gameObject.SetActive(true);
        _MVPPodium.gameObject.SetActive(true);
        // int MVPColorIndex = Context.GetColorIndexFromRewiredID(NetworkServices.StatisticsManager.GetMVPRewiredID());
        int MVPColorIndex = 0;
        Transform MVPChicken = _MVPPlayerHolder.GetChild(MVPColorIndex);
        MVPChicken.gameObject.SetActive(true);
        MVPChicken.GetComponent<Animator>().SetTrigger("Enter");
        Color MVPColor = _configData.IndexToColor[MVPColorIndex];
        MVPColor.a = 0f;
        _MVPTitle.GetComponent<TextMeshProUGUI>().color = MVPColor;
        Sequence seq = DOTween.Sequence();
        seq.Append(_MVPPodium.DOLocalMoveY(0.7f, _configData.MVPPodiumMoveDuration).SetEase(_configData.MVPPodiumMoveEase).SetRelative(true)
            .OnPlay(() => _gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPLandRiseAudioClip)));
        seq.Append(_MVPSpotLight.GetComponent<Light>().DOIntensity(_configData.MVPSpotLightIntensity, _configData.MVPSpotLightDuration).SetEase(_configData.MVPSpotLightEase).SetLoops(3, LoopType.Yoyo)
        .OnPlay(() => _gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPLightFlickerAudioClip)));
        seq.AppendInterval(_configData.MVPSpotLightToLandDuration);
        seq.Append(MVPChicken.DOLocalMoveY(-3.116f, 0.2f).SetEase(Ease.InCirc).SetRelative(true)
        .OnComplete(() =>
        {
            _gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPBirdLandAudioClip);
            _MVPCamera.GetComponent<DOTweenAnimation>().DORestartAllById("Land");
            MVPChicken.GetComponent<Animator>().SetTrigger("Pose");
        }));
        seq.AppendInterval(_configData.MVPLandToWordShowDuration);
        seq.Append(_MVPTitle.DOScale(5f, 0.2f).From().SetEase(Ease.OutCirc).SetRelative(true).OnComplete(() => _gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPBadgeStampAudioClip)));
        seq.Join(_MVPTitle.GetComponent<TextMeshProUGUI>().DOFade(1f, 0.2f));
        seq.AppendInterval(_configData.MVPToUIMoveInDuration);
        seq.Append(_MVPPlayerHolder.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
        seq.Join(_MVPSpotLight.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
        seq.Join(_MVPPodium.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
        seq.Join(_MVPTitle.DOLocalMove(new Vector3(-587f, 187f), _configData.MVPScaleDownDuration).SetEase(Ease.Linear));
        seq.Join(_MVPTitle.DOScale(0.6f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear));
        // var statsresult = NetworkServices.StatisticsManager.GetStatisticResult();
        /// Move in all players 
        GameObject.Instantiate(_configData.MVPBadgePrefab, _statisticUIHolder.GetChild(MVPColorIndex));
        for (int i = 0; i < _playersHolder.childCount; i++)
        {
            int colorindex = Utility.GetColorIndexFromPlayer(_playersHolder.GetChild(i).gameObject);
            Transform frame = _statisticUIHolder.GetChild(colorindex);
            // frame.GetChild(0).GetComponent<Image>().sprite = statsresult[rewiredID].StatisticIcon;
            // frame.GetChild(1).GetComponent<TextMeshProUGUI>().text = statsresult[rewiredID].StatisticName;
            // frame.GetChild(2).GetComponent<TextMeshProUGUI>().text = statsresult[rewiredID].StatisticsInformation;
        }
        // int[] ci = _PlayersInformation.ColorIndex;
        // Array.Sort(ci);
        // for (int i = 0; i < ci.Length; i++)
        // {
        //     int x = _PlayersInformation.ColorIndex[i];
        //     Context._statisticUIHolder.GetChild(x).DOLocalMoveY(_configData.FrameYPosition[i], 0f);
        //     // seq.Append(Context._statisticUIHolder.GetChild(x).DOLocalMoveX(770f, _configData.FrameMoveInDuration)
        //     seq.Append(Context._statisticUIHolder.GetChild(x).DOScale(0.7f, _configData.FrameMoveInDuration)
        //     .SetEase(Ease.OutBack)
        //     .OnPlay(() => Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.MVPStatisticPanelBopClip)));
        // }
        seq.Append(_holdAText.DOText("Next Map", 0.2f).OnPlay(() => _holdAText.gameObject.SetActive(true)));
        seq.Join(_holdBText.DOText("Menu", 0.2f).OnPlay(() => _holdBText.gameObject.SetActive(true)));
        seq.Join(_holdYText.DOText("Replay", 0.2f).OnPlay(() => _holdYText.gameObject.SetActive(true)));
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
        private string _victoryTeam
        {
            get
            {
                if (Context._winner == 1) return "CHICKENS WIN";
                else if (Context._winner == 2) return "DUCKS WIN";
                else return "DRAW";
            }
        }
        private Color _virtoryTeamColor
        {
            get
            {
                if (Context._winner == 1 || Context._winner == 2)
                {
                    return NetworkServices.Config.ConfigData.TeamColor[Context._winner - 1];
                }
                else
                {
                    return Color.white;
                }
            }
        }
        private NetworkDarkCornerEffect _darkCornerEffect;
        public override void OnEnter()
        {
            base.OnEnter();
            Context.RpcOnWin(Context._endFocusPosition, _victoryTeam, _virtoryTeamColor.r, _virtoryTeamColor.g, _virtoryTeamColor.b, _virtoryTeamColor.a);
            _darkCornerEffect = Camera.main.GetComponent<NetworkDarkCornerEffect>();
            Debug.Assert(_darkCornerEffect != null, "Dark Corner Effect Missing");
            _darkCornerEffect.CenterPosition = _targetPosition;

            float maxlength = Utility.GetMaxLengthToCorner(_targetPosition);
            _darkCornerEffect.enabled = true;
            _darkCornerEffect.Length = maxlength;
            float middlelength = maxlength * _GameMapData.DarkCornerMiddlePercentage;
            float finallength = maxlength * _GameMapData.DarkCornerFinalPercentage;
            Context._gameEndTitleText.GetComponent<TextMeshProUGUI>().text = _victoryTeam;
            Context._gameEndTitleText.GetComponent<TextMeshProUGUI>().color = _virtoryTeamColor;
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => _darkCornerEffect.Length, x => _darkCornerEffect.Length = x, middlelength, _GameMapData.DarkCornerToMiddleDuration));
            seq.Join(Context._gameEndTitleText.DOScale(1f, _GameMapData.TitleTextInDuration).SetEase(_GameMapData.TitleTextInCurve).
                SetDelay(_GameMapData.TitleTextInDelay));
            seq.Join(Context._gameEndTitleText.DOScale(0f, _GameMapData.TitleTextOutDuration).SetDelay(_GameMapData.TitleStayDuration + _GameMapData.TitleTextInDuration).SetEase(Ease
                .InBack));
            seq.AppendInterval(_GameMapData.DarkCornerMiddleStayDuration);
            seq.Append(DOTween.To(() => _darkCornerEffect.Length, x => _darkCornerEffect.Length = x, finallength, _GameMapData.DarkCornerToFinalDuration));
            seq.AppendCallback(() =>
            {
                Context._gameEndTitleText.GetComponent<TMP_Text>().text = "";
                TransitionTo<MVPEndPanelState>();
                return;
            });
        }

        public override void Update()
        {
            base.Update();
            if (_darkCornerEffect.enabled)
            {
                _darkCornerEffect.CenterPosition = _targetPosition;
            }
        }
    }

    [ClientRpc]
    public void RpcOnWin(Vector3 _endFocusPosition, string _victoryTeam, float _victoryTeamColorR, float _victoryTeamColorG, float _victoryTeamColorB, float _victoryTeamColorA)
    {
        Vector2 _targetPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        _targetPosition.y = Screen.height - _targetPosition.y;
        NetworkDarkCornerEffect _darkCornerEffect = Camera.main.GetComponent<NetworkDarkCornerEffect>();
        Debug.Assert(_darkCornerEffect != null, "Dark Corner Effect Missing");
        _darkCornerEffect.CenterPosition = _targetPosition;

        float maxlength = Utility.GetMaxLengthToCorner(_targetPosition);
        _darkCornerEffect.enabled = true;
        _darkCornerEffect.Length = maxlength;
        float middlelength = maxlength * _gameMapdata.DarkCornerMiddlePercentage;
        float finallength = maxlength * _gameMapdata.DarkCornerFinalPercentage;
        _gameEndTitleText.GetComponent<TextMeshProUGUI>().text = _victoryTeam;
        _gameEndTitleText.GetComponent<TextMeshProUGUI>().color = new Color(_victoryTeamColorR, _victoryTeamColorG, _victoryTeamColorB, _victoryTeamColorA);
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => _darkCornerEffect.Length, x => _darkCornerEffect.Length = x, middlelength, _gameMapdata.DarkCornerToMiddleDuration));
        seq.Join(_gameEndTitleText.DOScale(1f, _gameMapdata.TitleTextInDuration).SetEase(_gameMapdata.TitleTextInCurve).
            SetDelay(_gameMapdata.TitleTextInDelay));
        seq.Join(_gameEndTitleText.DOScale(0f, _gameMapdata.TitleTextOutDuration).SetDelay(_gameMapdata.TitleStayDuration + _gameMapdata.TitleTextInDuration).SetEase(Ease
            .InBack));
        seq.AppendInterval(_gameMapdata.DarkCornerMiddleStayDuration);
        seq.Append(DOTween.To(() => _darkCornerEffect.Length, x => _darkCornerEffect.Length = x, finallength, _gameMapdata.DarkCornerToFinalDuration));
        seq.AppendCallback(() =>
        {
            _gameEndTitleText.GetComponent<TMP_Text>().text = "";
        });
    }

    private class GameLoop : GameState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            _GameMapData.BackgroundMusicMixer.SetFloat("Vol", 0f);
            _GameMapData.BackgroundMusicMixer.SetFloat("Cutoff", 22000f);
        }
        // public override void Update()
        // {
        //     base.Update();
        //     if (_AnyPauseDown)
        //     {
        //         TransitionTo<PauseState>();
        //         return;
        //     }
        // }
    }

    private class HitStop : GameState
    {
        private float timer;
        public override void OnEnter()
        {
            base.OnEnter();
            timer = Time.unscaledTime + Context._hitStopFrames * Time.unscaledDeltaTime;
            Time.timeScale = Context._hitStopTimeScale;
        }

        public override void Update()
        {
            base.Update();
            if (timer < Time.unscaledTime)
            {
                TransitionTo<GameLoop>();
                return;
            }
            if (_AnyPauseDown)
            {
                TransitionTo<PauseState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Time.timeScale = 1f;
        }

    }

    private class PauseState : GameState
    {
        private bool onresume;
        public override void OnEnter()
        {
            base.OnEnter();
            _GameMapData.BackgroundMusicMixer.SetFloat("Vol", -10f);
            _GameMapData.BackgroundMusicMixer.SetFloat("Cutoff", 450f);
            onresume = true;
            Context._pauseBackgroundMask.gameObject.SetActive(true);
            Context._pausePausedText.gameObject.SetActive(true);
            Context._pauseResume.gameObject.SetActive(true);
            Context._pauseQuit.gameObject.SetActive(true);
            _switchMenu();
            // for (int i = 0; i < Context._playersOutestHolder.Length; i++)
            // {
            //     PlayerControllerMirror playercontroller = Context._playersOutestHolder[i].GetComponentInChildren<PlayerControllerMirror>(true);
            //     playercontroller.enabled = false;
            // }
            Time.timeScale = 0f;

        }

        public override void Update()
        {
            base.Update();
            if (_AnyBDown || _AnyPauseDown)
            {
                Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.PauseMenuSelectionAudioClip);
                TransitionTo<GameLoop>();
                return;
            }
            if (onresume && _AnyADown)
            {
                Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.PauseMenuSelectionAudioClip);
                TransitionTo<GameLoop>();
                return;
            }
            if (!onresume && _AnyADown)
            {
                Context._pauseWholeMask.GetComponent<Image>().DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("NewMenu"));
                Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.PauseMenuSelectionAudioClip);
                TransitionTo<GameQuitState>();
                return;
            }
            if (_AnyVLAxisRaw < -0.2f && !_vAxisInUse && !onresume)
            {
                onresume = true;
                Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.PauseMenuBrowseAudioClip);
                _switchMenu();
                return;
            }
            else if (_AnyVLAxisRaw > 0.2f && !_vAxisInUse && onresume)
            {
                onresume = false;
                Context._gameManager.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.PauseMenuBrowseAudioClip);
                _switchMenu();
                return;
            }
        }

        private void _switchMenu()
        {
            if (onresume)
            {
                Color temp = Context._pauseResume.GetComponent<Image>().color;
                temp.a = 1f;
                Context._pauseResume.GetComponent<Image>().color = temp;
                temp = Context._pauseQuit.GetComponent<Image>().color;
                temp.a = 0.66f;
                Context._pauseQuit.GetComponent<Image>().color = temp;
            }
            else
            {
                Color temp = Context._pauseResume.GetComponent<Image>().color;
                temp.a = 0.66f;
                Context._pauseResume.GetComponent<Image>().color = temp;
                temp = Context._pauseQuit.GetComponent<Image>().color;
                temp.a = 1f;
                Context._pauseQuit.GetComponent<Image>().color = temp;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Time.timeScale = 1f;
            Context._pauseBackgroundMask.gameObject.SetActive(false);
            Context._pausePausedText.gameObject.SetActive(false);
            Context._pauseResume.gameObject.SetActive(false);
            Context._pauseQuit.gameObject.SetActive(false);
            for (int i = 0; i < Context._playersHolder.childCount; i++)
            {
                PlayerControllerMirror playercontroller = Context._playersHolder.GetChild(i).GetComponentInChildren<PlayerControllerMirror>(true);
                playercontroller.enabled = true;
            }
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
            Context.RpcLand();
            int numOfPlayers = Context._playersHolder.childCount;
            Sequence seq = DOTween.Sequence();
            Sequence uiseq = DOTween.Sequence();
            for (int i = 0; i < Context._gameUI.childCount; i++)
            {
                int x = i;
                uiseq.Append(Context._gameUI.GetChild(x).DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
            }
            //_cam.transform.DOLocalMove(_GameMapData.CameraMoveToPosition, _GameMapData.CameraMoveDuration).SetDelay(_GameMapData.CameraMoveDelay).SetEase(_GameMapData.CameraMoveEase);
            _cam.DOFieldOfView(_GameMapData.CameraTargetFOV, _GameMapData.CameraMoveDuration).SetDelay(_GameMapData.CameraMoveDelay).SetEase(_GameMapData.CameraMoveEase);
            int chickenPosIndex = 0;
            int duckPosIndex = 0;
            for (int i = 0; i < Context._playersHolder.childCount; i++)
            {
                int playerIndex = i;
                if (playerIndex < 3) Context._playersHolder.GetChild(playerIndex).position = _GameMapData.DuckLandingPostion[duckPosIndex++];
                else Context._playersHolder.GetChild(playerIndex).position = _GameMapData.ChickenLandingPosition[chickenPosIndex++];
                int temp = i;
                Context._playersHolder.GetChild(playerIndex).gameObject.SetActive(true);
                seq.Join(Context._playersHolder.GetChild(playerIndex).DOLocalMoveY(0.64f, _GameMapData.BirdsFlyDownDuration).SetDelay(_GameMapData.BirdsFlyDownDelay[temp]).SetEase(_GameMapData.BirdsFlyDownEase).
                OnComplete(() =>
                {
                    // NetworkServices.GameFeelManager.ViberateController(rewiredID, 1f, 0.3f);
                    // _cam.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.FirstLandAudioClip);
                    _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree");
                }));
            }
            seq.AppendInterval(_GameMapData.FightDelay);
            seq.Append(Context._countDownText.DOScale(_GameMapData.FightScale / 2f, 0.8f).SetEase(Ease.InSine).OnPlay(() =>
            {
                // _cam.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.ReadyAudioClip);
                Context._countDownText.text = Context._configData.ReadyString;
            }));
            seq.AppendInterval(0.3f);
            seq.Append(Context._countDownText.DOScale(0f, 0.2f));
            seq.Append(Context._countDownText.DOScale(_GameMapData.FightScale, _GameMapData.FightDuration).SetEase(_GameMapData.FightEase).OnPlay(() =>
            {
                // _cam.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.FightAudioClip);
                Context._countDownText.text = Context._configData.FightString;
            }));
            seq.AppendInterval(_GameMapData.FightStayOnScreenDuration);
            seq.Append(Context._countDownText.DOScale(0f, 0.2f));
            seq.AppendCallback(() =>
            {
                TransitionTo<GameLoop>();
            });
        }

        public override void OnExit()
        {
            base.OnExit();
            Context.RpcLandExit();
            /// Need to Enable everything that game loop has
            /// 1. Enable PlayerControllerMirror, set rigidbody kinematic = false
            /// TODO: Color the players
            /// 2. Enable Camera
            /// 3. Send Game Start Event
            for (int i = 0; i < Context._playersHolder.childCount; i++)
            {
                PlayerControllerMirror playercontroller = Context._playersHolder.GetChild(i).GetComponentInChildren<PlayerControllerMirror>(true);
                playercontroller.enabled = true;
                playercontroller.GetComponent<Rigidbody>().isKinematic = false;
            }
            EventManager.Instance.TriggerEvent(new GameStart());
        }
    }

    [ClientRpc]
    private void RpcLandExit()
    {
        for (int i = 0; i < _playersHolder.childCount; i++)
        {
            PlayerControllerMirror playercontroller = _playersHolder.GetChild(i).GetComponentInChildren<PlayerControllerMirror>(true);
            playercontroller.enabled = true;
            playercontroller.GetComponent<Rigidbody>().isKinematic = false;
        }
        EventManager.Instance.TriggerEvent(new GameStart());
    }

    [ClientRpc]
    private void RpcLand()
    {
        Sequence seq = DOTween.Sequence();
        Sequence uiseq = DOTween.Sequence();
        for (int i = 0; i < _gameUI.childCount; i++)
        {
            int x = i;
            uiseq.Append(_gameUI.GetChild(x).DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        }
        //_cam.transform.DOLocalMove(_GameMapData.CameraMoveToPosition, _GameMapData.CameraMoveDuration).SetDelay(_GameMapData.CameraMoveDelay).SetEase(_GameMapData.CameraMoveEase);
        _cam.DOFieldOfView(_gameMapdata.CameraTargetFOV, _gameMapdata.CameraMoveDuration).SetDelay(_gameMapdata.CameraMoveDelay).SetEase(_gameMapdata.CameraMoveEase);
        int chickenPosIndex = 0;
        int duckPosIndex = 0;
        for (int i = 0; i < _playersHolder.childCount; i++)
        {
            int playerIndex = Utility.GetColorIndexFromPlayer(_playersHolder.GetChild(i).gameObject);
            if (playerIndex < 3) _playersHolder.GetChild(i).position = _gameMapdata.DuckLandingPostion[duckPosIndex++];
            else _playersHolder.GetChild(i).position = _gameMapdata.ChickenLandingPosition[chickenPosIndex++];
            int temp = i;
            seq.Join(_playersHolder.GetChild(i).DOLocalMoveY(0.64f, _gameMapdata.BirdsFlyDownDuration).SetDelay(_gameMapdata.BirdsFlyDownDelay[temp]).SetEase(_gameMapdata.BirdsFlyDownEase).
            OnComplete(() =>
            {
                // NetworkServices.GameFeelManager.ViberateController(rewiredID, 1f, 0.3f);
                // _cam.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.FirstLandAudioClip);
                _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree");
            }));
        }
        seq.AppendInterval(_gameMapdata.FightDelay);
        seq.Append(_countDownText.DOScale(_gameMapdata.FightScale / 2f, 0.8f).SetEase(Ease.InSine).OnPlay(() =>
        {
            // _cam.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.ReadyAudioClip);
            _countDownText.text = _configData.ReadyString;
        }));
        seq.AppendInterval(0.3f);
        seq.Append(_countDownText.DOScale(0f, 0.2f));
        seq.Append(_countDownText.DOScale(_gameMapdata.FightScale, _gameMapdata.FightDuration).SetEase(_gameMapdata.FightEase).OnPlay(() =>
        {
            // _cam.GetComponent<AudioSource>().PlayOneShot(NetworkServices.AudioManager.AudioDataStore.FightAudioClip);
            _countDownText.text = _configData.FightString;
        }));
        seq.AppendInterval(_gameMapdata.FightStayOnScreenDuration);
        seq.Append(_countDownText.DOScale(0f, 0.2f));
    }

    private class EmptyState : GameState
    {

    }

    private abstract class TutorialState : GameState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._cam.GetComponent<AudioSource>().Play();
            _GameMapData.BackgroundMusicMixer.SetFloat("Vol", 0f);
            _GameMapData.BackgroundMusicMixer.SetFloat("Cutoff", 22000f);
        }
    }

    private class FoodCartTutorialState : TutorialState
    {
        private bool _canHoldA;

        public override void OnEnter()
        {
            base.OnEnter();
            Context._tutorialBackgroundMask.gameObject.SetActive(true);
            Sequence seq = DOTween.Sequence();
            seq.Append(Context._tutorialImage.DOScale(Vector3.one, _GameMapData.TutorialImageMoveInDuration).SetEase(_GameMapData.TutorialImageMoveInEase));
            seq.Append(Context._tutorialImage.DOScale(Vector3.one, 0f));
            for (int i = 0; i < Context._tutorialObjectiveImages.childCount; i++)
            {
                int x = i;
                seq.Append(Context._tutorialObjectiveImages.GetChild(x).DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            }
            seq.Append(Context._holdAText.DOText("Start Game", _GameMapData.HoldAMoveInDuration).SetDelay(_GameMapData.HoldAMoveInDelay).OnComplete(() => _canHoldA = true).OnPlay(() => Context._holdAText.gameObject.SetActive(true)));
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
                    // TransitionTo<MVPEndPanelState>();
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
            Context._tutorialBackgroundMask.gameObject.SetActive(false);
            for (int i = 0; i < Context._tutorialObjectiveImages.childCount; i++)
            {
                //Context._tutorialObjectiveImages.GetChild(i).DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(i * 0.1f);
                Context._tutorialObjectiveImages.GetChild(i).DOScale(Vector3.zero, 0f);
            }
            //Context._tutorialImage.DOScale(Vector3.zero, _GameMapData.TutorialImageMoveInDuration).SetEase(_GameMapData.TutorialImageMoveInEase).SetDelay(0.5f);
            Context._tutorialImage.DOScale(Vector3.zero, 0f);
            Context._holdAText.DOText("", 0f);
            Context._holdAText.gameObject.SetActive(false);
            Context._holdAImage.fillAmount = 0f;
        }
    }
}
