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

public class GameStateManager
{
    public PlayerController[] PlayerControllers;
    public PlayerInformation PlayersInformation;
    public List<Transform> CameraTargets;

    private GameMapData _gameMapdata;
    private ConfigData _configData;
    private FSM<GameStateManager> _gameStateFSM;
    private TextMeshProUGUI _holdAText;
    private Image _holdAImage;
    private Transform _tutorialImage;
    private Transform _tutorialBackgroundMask;
    private Transform _tutorialObjectiveImages;
    private Transform _playersHolder;
    private Transform[] _playersOutestHolder;
    private TextMeshProUGUI _countDownText;
    private Camera _cam;
    private Vector3 _endFocusPosition;
    private DarkCornerEffect _darkCornerEffect;
    private Transform _gameEndCanvas;
    private GameObject _gameEndBlackbackground;
    private Transform _gameEndTitleText;
    private Transform _pauseMenu;
    private Transform _pauseBackgroundMask;
    private Transform _pausePausedText;
    private Transform _pauseResume;
    private Transform _pauseQuit;
    private Transform _pauseWholeMask;
    private Transform _statisticIndicator;
    private Transform _statisticNominee;
    private Transform _statisticExtra;
    private Transform _statisticRecord;
    private Transform _statisticPanel;
    private Transform _MVPDisplay;
    private Transform _MVPTitle;
    private Transform _MVP;
    private Transform _MVPCamera;
    private Transform _MVPPlayerHolder;
    private Transform _MVPSpotLight;
    private Transform _MVPPodium;
    private int _winner;

    public GameStateManager(GameMapData _gmp, ConfigData _cfd)
    {
        _gameMapdata = _gmp;
        _configData = _cfd;
        _gameStateFSM = new FSM<GameStateManager>(this);
        _gameEndCanvas = GameObject.Find("GameEndCanvas").transform;
        _gameEndBlackbackground = _gameEndCanvas.Find("EndImageBackground").gameObject;
        _gameEndTitleText = _gameEndCanvas.Find("TitleText");
        _holdAText = GameObject.Find("HoldCanvas").transform.Find("HoldA").GetComponent<TextMeshProUGUI>();
        _holdAImage = GameObject.Find("HoldCanvas").transform.Find("HoldAImage").GetComponent<Image>();
        PlayersInformation = DataSaver.loadData<PlayerInformation>("PlayersInformation");
        Debug.Assert(PlayersInformation != null, "Unable to load Players information");
        _playersHolder = GameObject.Find("Players").transform;
        _tutorialImage = GameObject.Find("TutorialCanvas").transform.Find("TutorialImage");
        _tutorialBackgroundMask = GameObject.Find("TutorialCanvas").transform.Find("BackgroundMask");
        _tutorialObjectiveImages = GameObject.Find("TutorialCanvas").transform.Find("ObjectiveImages");
        Debug.Assert(_tutorialImage != null);
        _playersOutestHolder = new Transform[6];
        _countDownText = GameObject.Find("TutorialCanvas").transform.Find("CountDown").GetComponent<TextMeshProUGUI>();
        _pauseMenu = GameObject.Find("PauseMenu").transform;
        _pauseBackgroundMask = _pauseMenu.Find("BackgroundMask");
        _pausePausedText = _pauseMenu.Find("Paused");
        _pauseResume = _pauseMenu.Find("Resume");
        _pauseQuit = _pauseMenu.Find("Quit");
        _pauseWholeMask = _pauseMenu.Find("WholeMask");
        PlayerControllers = new PlayerController[PlayersInformation.ColorIndex.Length];
        _statisticIndicator = _gameEndCanvas.Find("StatisticsIndicator");
        _statisticNominee = _gameEndCanvas.Find("StatisticsNominee");
        _statisticRecord = _gameEndCanvas.Find("StatisticsRecord");
        _statisticExtra = _gameEndCanvas.Find("StatisticsExtra");
        _statisticPanel = _gameEndCanvas.Find("StatisticPanel");
        _MVPDisplay = _statisticPanel.Find("MVPDisplay");
        _MVPTitle = _statisticPanel.Find("MVPTitle");
        _MVP = GameObject.Find("MVP").transform;
        _MVPCamera = _MVP.Find("MVP Camera");
        _MVPPlayerHolder = _MVP.Find("MVPPlayerHolder");
        _MVPSpotLight = _MVP.Find("MVPSpotLight");
        _MVPPodium = _MVP.Find("MVPPodium");
        CameraTargets = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            _playersOutestHolder[i] = _playersHolder.GetChild(i);
        }
        for (int i = 0; i < PlayersInformation.ColorIndex.Length; i++)
        {
            PlayerControllers[i] = _playersOutestHolder[PlayersInformation.ColorIndex[i]].GetComponentInChildren<PlayerController>(true);
            CameraTargets.Add(PlayerControllers[i].transform);
        }
        EventManager.Instance.AddHandler<GameEnd>(_onGameEnd);
        EventManager.Instance.AddHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.AddHandler<PlayerRespawned>(_onPlayerRespawn);
        _cam = Camera.main;
        _darkCornerEffect = _cam.GetComponent<DarkCornerEffect>();
        // _gameStateFSM.TransitionTo<FoodCartTutorialState>();
        _gameStateFSM.TransitionTo<MVPEndPanelState>();
    }

    public int GetColorIndexFromRewiredID(int rewiredID)
    {
        for (int i = 0; i < PlayersInformation.RewiredID.Length; i++)
        {
            if (PlayersInformation.RewiredID[i] == rewiredID) return PlayersInformation.ColorIndex[i];
        }
        Debug.LogError("Rewired ID: " + rewiredID + " Related Color Index not Found");
        return -1;
    }

    public void Update()
    {
        _gameStateFSM.Update();
    }

    public void Destroy()
    {
        EventManager.Instance.RemoveHandler<GameEnd>(_onGameEnd);
        EventManager.Instance.RemoveHandler<PlayerDied>(_onPlayerDied);
        EventManager.Instance.RemoveHandler<PlayerRespawned>(_onPlayerRespawn);
        if (_gameStateFSM.CurrentState != null)
            _gameStateFSM.CurrentState.CleanUp();
    }

    private void _onGameEnd(GameEnd ge)
    {
        if (!_gameStateFSM.CurrentState.GetType().Equals(typeof(WinState)))
        {
            _endFocusPosition = ge.WinnedObjective.position;
            _winner = ge.Winner;
            _gameStateFSM.TransitionTo<WinState>();
            return;
        }
    }

    private void _onPlayerDied(PlayerDied pd)
    {
        CameraTargets.Remove(pd.Player.transform);
    }

    private void _onPlayerRespawn(PlayerRespawned pr)
    {
        CameraTargets.Add(pr.Player.transform);
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

        protected float _AnyVLAxisRaw
        {
            get
            {
                float result = 0f;
                for (int i = 0; i < _PlayersInformation.RewiredID.Length; i++)
                {
                    result = ReInput.players.GetPlayer(_PlayersInformation.RewiredID[i]).GetAxisRaw("Move Vertical");
                    if (!Mathf.Approximately(0f, result)) return result;
                }
                return result;
            }
        }

        protected bool _vAxisInUse = true;

        protected GameMapData _GameMapData;

        public override void Init()
        {
            base.Init();
            _PlayersInformation = Context.PlayersInformation;
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
        public override void OnEnter()
        {
            base.OnEnter();
            Context._gameEndBlackbackground.SetActive(true);
            Context._darkCornerEffect.enabled = false;
        }
    }

    private class MVPEndPanelState : StatisticsWordState
    {
        public override void OnEnter()
        {
            Context._MVPDisplay.gameObject.SetActive(true);
            Context._MVPCamera.gameObject.SetActive(true);
            Context._MVPPlayerHolder.gameObject.SetActive(true);
            Context._MVPSpotLight.gameObject.SetActive(true);
            Context._MVPPodium.gameObject.SetActive(true);
            int MVPColorIndex = Context.GetColorIndexFromRewiredID(Services.StatisticsManager.GetMVPRewiredID());
            Transform MVPChicken = Context._MVPPlayerHolder.GetChild(MVPColorIndex);
            MVPChicken.gameObject.SetActive(true);
            MVPChicken.GetComponent<Animator>().SetTrigger("Enter");
            Color MVPColor = _configData.IndexToColor[MVPColorIndex];
            MVPColor.a = 0f;
            Context._MVPTitle.GetComponent<TextMeshProUGUI>().color = MVPColor;
            Sequence seq = DOTween.Sequence();
            seq.Append(Context._MVPPodium.DOLocalMoveY(0.7f, _configData.MVPPodiumMoveDuration).SetEase(_configData.MVPPodiumMoveEase).SetRelative(true));
            seq.Append(Context._MVPSpotLight.GetComponent<Light>().DOIntensity(_configData.MVPSpotLightIntensity, _configData.MVPSpotLightDuration).SetEase(_configData.MVPSpotLightEase).SetLoops(3, LoopType.Yoyo));
            seq.AppendInterval(_configData.MVPSpotLightToLandDuration);
            seq.Append(MVPChicken.DOLocalMoveY(-3.116f, 0.2f).SetEase(Ease.InCirc).SetRelative(true)
            .OnComplete(() =>
            {
                Context._MVPCamera.GetComponent<DOTweenAnimation>().DORestartAllById("Land");
                MVPChicken.GetComponent<Animator>().SetTrigger("Pose");
            }));
            seq.AppendInterval(_configData.MVPLandToWordShowDuration);
            seq.Append(Context._MVPTitle.DOScale(5f, 0.2f).From().SetEase(Ease.OutCirc).SetRelative(true));
            seq.Join(Context._MVPTitle.GetComponent<TextMeshProUGUI>().DOFade(1f, 0.2f));
            seq.AppendInterval(_configData.MVPToUIMoveInDuration);
            seq.Append(Context._MVPPlayerHolder.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
            seq.Join(Context._MVPSpotLight.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
            seq.Join(Context._MVPPodium.DOLocalMoveX(-1.022f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear).SetRelative(true));
            seq.Join(Context._MVPTitle.DOLocalMove(new Vector3(-587f, 112f), _configData.MVPScaleDownDuration).SetEase(Ease.Linear));
            seq.Join(Context._MVPTitle.DOScale(0.5f, _configData.MVPScaleDownDuration).SetEase(Ease.Linear));

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
        private string _victoryTeam
        {
            get
            {
                if (Context._winner == 1) return "CHICKENS WIN";
                else return "DUCKS WIN";
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
            Context._gameEndTitleText.GetComponent<TextMeshProUGUI>().text = _victoryTeam;
            Context._gameEndTitleText.GetComponent<TextMeshProUGUI>().color = Services.Config.ConfigData.TeamColor[Context._winner - 1];
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => Context._darkCornerEffect.Length, x => Context._darkCornerEffect.Length = x, middlelength, _GameMapData.DarkCornerToMiddleDuration));
            seq.Join(Context._gameEndTitleText.DOScale(1f, _GameMapData.TitleTextInDuration).SetEase(_GameMapData.TitleTextInCurve).
                SetDelay(_GameMapData.TitleTextInDelay));
            seq.Join(Context._gameEndTitleText.DOScale(0f, _GameMapData.TitleTextOutDuration).SetDelay(_GameMapData.TitleStayDuration + _GameMapData.TitleTextInDuration).SetEase(Ease
                .InBack));
            seq.AppendInterval(_GameMapData.DarkCornerMiddleStayDuration);
            seq.Append(DOTween.To(() => Context._darkCornerEffect.Length, x => Context._darkCornerEffect.Length = x, finallength, _GameMapData.DarkCornerToFinalDuration));
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
            _GameMapData.BackgroundMusicMixer.SetFloat("Vol", 0f);
            _GameMapData.BackgroundMusicMixer.SetFloat("Cutoff", 22000f);
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
            Time.timeScale = 0f;
        }

        public override void Update()
        {
            base.Update();
            if (onresume && _AnyADown)
            {
                TransitionTo<GameLoop>();
                return;
            }
            if (!onresume && _AnyADown)
            {
                Context._pauseWholeMask.GetComponent<Image>().DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("NewMenu"));
                TransitionTo<GameQuitState>();
                return;
            }
            if (_AnyVLAxisRaw < -0.2f && !_vAxisInUse && !onresume)
            {
                onresume = true;
                _switchMenu();
                return;
            }
            else if (_AnyVLAxisRaw > 0.2f && !_vAxisInUse && onresume)
            {
                onresume = false;
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
            int chickenPosIndex = 0;
            int duckPosIndex = 0;
            for (int i = 0; i < _PlayersInformation.ColorIndex.Length; i++)
            {
                int playerIndex = _PlayersInformation.ColorIndex[i];
                if (playerIndex < 3) Context._playersOutestHolder[playerIndex].position = _GameMapData.DuckLandingPostion[duckPosIndex++];
                else Context._playersOutestHolder[playerIndex].position = _GameMapData.ChickenLandingPosition[chickenPosIndex++];
                int temp = i;
                Context._playersOutestHolder[playerIndex].gameObject.SetActive(true);
                seq.Join(Context._playersOutestHolder[playerIndex].DOLocalMoveY(0.64f, _GameMapData.BirdsFlyDownDuration).SetDelay(_GameMapData.BirdsFlyDownDelay[temp]).SetEase(_GameMapData.BirdsFlyDownEase).
                OnComplete(() => _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree")));
            }
            seq.AppendInterval(_GameMapData.FightDelay);
            seq.Append(Context._countDownText.DOScale(_GameMapData.FightScale, _GameMapData.FightDuration).SetEase(_GameMapData.FightEase));
            seq.AppendInterval(_GameMapData.FightStayOnScreenDuration);
            seq.Append(Context._countDownText.DOScale(0f, 0.2f));
            seq.AppendCallback(() =>
            {
                Context._cam.GetComponent<AudioSource>().Play();
                TransitionTo<GameLoop>();
            });
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
            Context._tutorialBackgroundMask.gameObject.SetActive(true);
            Sequence seq = DOTween.Sequence();
            seq.Append(Context._tutorialImage.DOScale(Vector3.one, _GameMapData.TutorialImageMoveInDuration).SetEase(_GameMapData.TutorialImageMoveInEase));
            seq.Append(Context._tutorialImage.DOScale(Vector3.one, 0f));
            for (int i = 0; i < Context._tutorialObjectiveImages.childCount; i++)
            {
                seq.Join(Context._tutorialObjectiveImages.GetChild(i).DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(i * 0.2f));
            }
            seq.Append(Context._holdAText.DOText("Hold  A  To Start", _GameMapData.HoldAMoveInDuration).SetDelay(_GameMapData.HoldAMoveInDelay).OnComplete(() => _canHoldA = true));
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
            Context._tutorialBackgroundMask.gameObject.SetActive(false);
            for (int i = 0; i < Context._tutorialObjectiveImages.childCount; i++)
            {
                //Context._tutorialObjectiveImages.GetChild(i).DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(i * 0.1f);
                Context._tutorialObjectiveImages.GetChild(i).DOScale(Vector3.zero, 0f);
            }
            //Context._tutorialImage.DOScale(Vector3.zero, _GameMapData.TutorialImageMoveInDuration).SetEase(_GameMapData.TutorialImageMoveInEase).SetDelay(0.5f);
            Context._tutorialImage.DOScale(Vector3.zero, 0f);
            Context._holdAText.DOText("", 0f);
            Context._holdAImage.transform.DOScale(0f, 0f).SetEase(Ease.OutQuad);
        }
    }
}
