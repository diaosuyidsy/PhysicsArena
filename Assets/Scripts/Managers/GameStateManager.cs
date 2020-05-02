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

public class GameStateManager : GameStateManagerBase
{
    private ConfigData _configData;
    private FSM<GameStateManager> _gameStateFSM;
    private GameObject _gameStartAImage;
    private GameObject _gameStartAButton;
    private GameObject _gameStartHoldAImage;
    private GameObject _holdAText;
    private Image _holdAImage;
    private GameObject _holdAButton;
    private GameObject _holdBText;
    private Image _holdBImage;
    private GameObject _holdBButton;
    private GameObject _holdYText;
    private Image _holdYImage;
    private GameObject _holdYButton;
    private Transform _tutorialImage;
    private Transform _tutorialBackgroundMask;
    private Transform _tutorialObjectiveImages;
    private Transform _playersHolder;
    private TextMeshProUGUI _countDownText;
    private Camera _cam;
    private Vector3 _endFocusPosition;
    private DarkCornerEffect _darkCornerEffect;
    private Transform _gameEndCanvas;
    private Transform _gameEndTitleText;
    private Transform _pauseMenu;
    private Transform _pauseBackgroundMask;
    private Transform _pausePausedText;
    private Transform _pauseResume;
    private Transform _pauseQuit;
    private Transform _pauseWholeMask;
    private Transform _statisticPanel;
    private Transform _statisticUIHolder;
    private Transform _MVPBackground;
    private Transform _MVPPlayerPortrait;
    private Transform _MVPTitle;
    private Transform _gameUI;
    private int _winner;
    private GameObject _gameManager;
    private int _hitStopFrames;
    private float _hitStopTimeScale;

    public GameStateManager(GameMapData _gmp, ConfigData _cfd, GameObject _gm)
    {
        _gameMapdata = _gmp;
        _gameManager = _gm;
        _configData = _cfd;
        _gameStateFSM = new FSM<GameStateManager>(this);
        _gameUI = GameObject.Find("GameUI").transform;
        _gameEndCanvas = GameObject.Find("GameEndCanvas").transform;
        _gameEndTitleText = _gameEndCanvas.Find("TitleText");
        _holdAText = GameObject.Find("HoldCanvas").transform.Find("HoldA").gameObject;
        _holdAButton = GameObject.Find("HoldCanvas").transform.Find("AButton").gameObject;
        _holdAImage = GameObject.Find("HoldCanvas").transform.Find("HoldAImage").GetComponent<Image>();
        _holdBText = GameObject.Find("HoldCanvas").transform.Find("HoldB").gameObject;
        _holdBButton = GameObject.Find("HoldCanvas").transform.Find("BButton").gameObject;
        _holdBImage = GameObject.Find("HoldCanvas").transform.Find("HoldBImage").GetComponent<Image>();
        _holdYText = GameObject.Find("HoldCanvas").transform.Find("HoldY").gameObject;
        _holdYButton = GameObject.Find("HoldCanvas").transform.Find("YButton").gameObject;
        _holdYImage = GameObject.Find("HoldCanvas").transform.Find("HoldYImage").GetComponent<Image>();
        PlayersInformation = DataSaver.loadData<PlayerInformation>("PlayersInformation");
        Debug.Assert(PlayersInformation != null, "Unable to load Players information");
        _playersHolder = GameObject.Find("Players").transform;
        _tutorialImage = GameObject.Find("TutorialCanvas").transform.Find("TutorialImage");
        _tutorialBackgroundMask = GameObject.Find("TutorialCanvas").transform.Find("BackgroundMask");
        _tutorialObjectiveImages = GameObject.Find("TutorialCanvas").transform.Find("ObjectiveImages");
        Debug.Assert(_tutorialImage != null);
        _gameStartAImage = GameObject.Find("TutorialCanvas").transform.Find("HoldA").gameObject;
        _gameStartAButton = GameObject.Find("TutorialCanvas").transform.Find("AButton").gameObject;
        _gameStartHoldAImage = GameObject.Find("TutorialCanvas").transform.Find("HoldAImage").gameObject;
        _countDownText = GameObject.Find("TutorialCanvas").transform.Find("CountDown").GetComponent<TextMeshProUGUI>();
        _pauseMenu = GameObject.Find("PauseMenu").transform;
        _pauseBackgroundMask = _pauseMenu.Find("BackgroundMask");
        _pausePausedText = _pauseMenu.Find("Paused");
        _pauseResume = _pauseMenu.Find("Resume");
        _pauseQuit = _pauseMenu.Find("Quit");
        _pauseWholeMask = _pauseMenu.Find("WholeMask");
        _statisticPanel = _gameEndCanvas.Find("StatisticPanel");
        _MVPBackground = _statisticPanel.Find("MVPBackground");
        _MVPPlayerPortrait = _statisticPanel.Find("MVPPlayerIcon");
        _MVPTitle = _statisticPanel.Find("MVPTitle");
        _statisticUIHolder = _statisticPanel.Find("StatisticUIHolder");
        for (int i = 0; i < PlayersInformation.ColorIndex.Length; i++)
        {
            GameObject player = GameObject.Instantiate(_configData.PlayerPrefabs[PlayersInformation.ColorIndex[i]]);
            player.transform.parent = _playersHolder;
        }
        EventManager.Instance.AddHandler<GameEnd>(_onGameEnd);
        EventManager.Instance.AddHandler<HitStopEvent>(_onHitStop);
        _cam = Camera.main;
        _darkCornerEffect = _cam.transform.GetChild(0).GetComponent<DarkCornerEffect>();
        _gameStateFSM.TransitionTo<TutorialState>();
        // _gameStateFSM.TransitionTo<MVPEndPanelState>();
    }

    public int GetColorIndexFromRewiredID(int rewiredID)
    {
        for (int i = 0; i < PlayersInformation.RewiredID.Length; i++)
        {
            if (PlayersInformation.RewiredID[i] == rewiredID) return PlayersInformation.ColorIndex[i];
        }
        Debug.LogError("Rewired ID: " + rewiredID + " Related Color Index not Found");
        return PlayersInformation.ColorIndex[0];
    }

    public int GetRewiredIDFromColorIndex(int colorindex)
    {
        for (int i = 0; i < PlayersInformation.ColorIndex.Length; i++)
        {
            if (PlayersInformation.ColorIndex[i] == colorindex) return PlayersInformation.RewiredID[i];
        }
        return -1;
    }

    public override void Update()
    {
        _gameStateFSM.Update();
    }

    public override void Destroy()
    {
        EventManager.Instance.RemoveHandler<GameEnd>(_onGameEnd);
        EventManager.Instance.RemoveHandler<HitStopEvent>(_onHitStop);
        if (_gameStateFSM.CurrentState != null)
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
        if (_gameStateFSM.CurrentState.GetType().Equals(typeof(GameLoop)))
        {
            if (ge.WinnedObjective != null)
                _endFocusPosition = ge.WinnedObjective.position;
            else _endFocusPosition = ge.WinnedPosition;
            _winner = ge.Winner;
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
        protected bool _AnyBDown
        {
            get
            {
                for (int i = 0; i < _PlayersInformation.RewiredID.Length; i++)
                {
                    if (ReInput.players.GetPlayer(_PlayersInformation.RewiredID[i]).GetButtonDown("Block")) return true;
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

        protected bool _AnyBHolding
        {
            get
            {
                for (int i = 0; i < _PlayersInformation.RewiredID.Length; i++)
                {
                    if (ReInput.players.GetPlayer(_PlayersInformation.RewiredID[i]).GetButton("Block")) return true;
                }
                return false;
            }
        }

        protected bool _AnyYHolding
        {
            get
            {
                for (int i = 0; i < _PlayersInformation.RewiredID.Length; i++)
                {
                    if (ReInput.players.GetPlayer(_PlayersInformation.RewiredID[i]).GetButton("QuestionMark")) return true;
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
            Context._darkCornerEffect.enabled = false;
        }
    }

    private class MVPEndPanelState : StatisticsWordState
    {
        private bool _canHold;
        public override void OnEnter()
        {
            base.OnEnter();
            // Disable all players controller
            for (int i = 0; i < Context._playersHolder.childCount; i++)
            {
                PlayerController playercontroller = Context._playersHolder.GetChild(i).GetComponent<PlayerController>();
                playercontroller.enabled = false;
                playercontroller.GetComponent<Rigidbody>().isKinematic = true;
            }
            // Disable Game UI
            Context._gameUI.gameObject.SetActive(false);
            // Get MVP Info
            int MVPRewiredID = Services.StatisticsManager.GetMVPRewiredID();
            int MVPTeamNumber = Utility.GetIdentificationFromRewiredID(MVPRewiredID).PlayerTeamNumber;
            int MVPColorIndex = Utility.GetIdentificationFromRewiredID(MVPRewiredID).ColorIndex;
            // Display MVP related Info
            // 1. Display MVP team background\
            Context._MVPBackground.GetComponent<Image>().sprite = _configData.TeamNumberToMVPBackground[MVPTeamNumber];
            Context._MVPBackground.gameObject.SetActive(true);
            // 2. Set Up MVP Player Portrait and Title Text
            Context._MVPPlayerPortrait.GetComponent<Image>().sprite = _configData.ColorIndexToMVPPlayerPortrait[MVPColorIndex];
            Context._MVPTitle.GetComponent<Image>().sprite = _configData.TeamNumberToMVPTitleSprite[MVPTeamNumber];
            // 3. Move In MVP Player Portrait and Title Text
            Context._MVPTitle.gameObject.SetActive(true);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(Context._MVPTitle.DOLocalMove(_configData.MVPTitleMoveAmount, _configData.MVPTitleMoveDuration)
            .SetEase(_configData.MVPTitleMoveEase));

            sequence.Join(Context._MVPPlayerPortrait.DOLocalMove(_configData.MVPPortraitMoveAmount, _configData.MVPPortraitMoveDuration)
            .SetEase(_configData.MVPPortraitMoveEase));
            // 4. Small Delay then start display statistic
            sequence.AppendInterval(_configData.MVPPortraitToStatisticDelay);
            var statsresult = Services.StatisticsManager.GetStatisticResult();
            // 5. Setup all statistic display
            for (int i = 0; i < _PlayersInformation.RewiredID.Length; i++)
            {
                int rewiredID = _PlayersInformation.RewiredID[i];
                int colorindex = Context.GetColorIndexFromRewiredID(rewiredID);
                Transform frame = Context._statisticUIHolder.GetChild(colorindex);
                frame.GetChild(0).GetComponent<Image>().sprite = statsresult[rewiredID].StatisticIcon[colorindex];
                frame.GetChild(1).GetComponent<Image>().sprite = _configData.ColorIndexToStatisticPlayerIcon[colorindex];
                frame.GetChild(2).GetComponent<TextMeshProUGUI>().text = statsresult[rewiredID].StatisticName;
                frame.GetChild(3).GetComponent<TextMeshProUGUI>().text = statsresult[rewiredID].StatisticsInformation;
            }

            // 6. Move In All Statistic display
            int[] ci = _PlayersInformation.ColorIndex;
            Array.Sort(ci);
            for (int i = 0; i < ci.Length; i++)
            {
                int x = _PlayersInformation.ColorIndex[i];
                Context._statisticUIHolder.GetChild(x).DOLocalMoveY(_configData.FrameYPosition[i], 0f);
                sequence.Append(Context._statisticUIHolder.GetChild(x).DOLocalMoveX(_configData.FrameXPosition, _configData.FrameMoveInDuration)
                .SetEase(Ease.OutBack)
                .OnPlay(() => Services.AudioManager.PlaySound("event:/SFX/Menu/Select")));
            }
            sequence.Append(Context._holdAText.transform.DOScale(1f, 0.2f).OnPlay(() => Context._holdAText.gameObject.SetActive(true)));
            sequence.Join(Context._holdBText.transform.DOScale(1f, 0.2f).OnPlay(() => Context._holdBText.gameObject.SetActive(true)));
            sequence.Join(Context._holdAButton.transform.DOScale(1f, 0.2f).OnPlay(() => Context._holdAButton.gameObject.SetActive(true)));
            sequence.Join(Context._holdBButton.transform.DOScale(1f, 0.2f).OnPlay(() => Context._holdBButton.gameObject.SetActive(true)));
            sequence.Join(Context._holdYButton.transform.DOScale(1f, 0.2f).OnPlay(() => Context._holdYButton.gameObject.SetActive(true)));
            sequence.Join(Context._holdYText.transform.DOScale(1f, 0.2f).OnPlay(() => Context._holdYText.gameObject.SetActive(true)));
            sequence.AppendCallback(() => _canHold = true);
        }

        public override void Update()
        {
            base.Update();
            if (_canHold && _AnyAHolding)
            {
                Context._holdAImage.fillAmount += Time.deltaTime * _GameMapData.FillASpeed;
                if (Context._holdAImage.fillAmount >= 1f)
                {
                    if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                    else
                        SceneManager.LoadScene(1);
                }
            }
            else
            {
                Context._holdAImage.fillAmount -= Time.deltaTime * _GameMapData.FillASpeed;
            }

            if (_canHold && _AnyBHolding)
            {
                Context._holdBImage.fillAmount += Time.deltaTime * _GameMapData.FillASpeed;
                if (Context._holdBImage.fillAmount >= 1f)
                {
                    SceneManager.LoadScene("ComicMenu");
                }
            }
            else
            {
                Context._holdBImage.fillAmount -= Time.deltaTime * _GameMapData.FillASpeed;
            }

            if (_canHold && _AnyYHolding)
            {
                Context._holdYImage.fillAmount += Time.deltaTime * _GameMapData.FillASpeed;
                if (Context._holdYImage.fillAmount >= 1f)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
            else
            {
                Context._holdYImage.fillAmount -= Time.deltaTime * _GameMapData.FillASpeed;
            }
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
                if (Context._winner == 1) return "TEAM Red WIN";
                else if (Context._winner == 2) return "Team Blue WIN";
                else return "DRAW";
            }
        }
        private Color _virtoryTeamColor
        {
            get
            {
                if (Context._winner == 1 || Context._winner == 2)
                {
                    return Services.Config.ConfigData.TeamColor[Context._winner - 1];
                }
                else
                {
                    return Color.white;
                }
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
            Context._gameEndTitleText.GetComponent<TextMeshProUGUI>().color = _virtoryTeamColor;
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
            // _GameMapData.BackgroundMusicMixer.SetFloat("Vol", 0f);
            // _GameMapData.BackgroundMusicMixer.SetFloat("Cutoff", 22000f);
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
            // _GameMapData.BackgroundMusicMixer.SetFloat("Vol", -10f);
            // _GameMapData.BackgroundMusicMixer.SetFloat("Cutoff", 450f);
            onresume = true;
            Context._pauseBackgroundMask.gameObject.SetActive(true);
            Context._pausePausedText.gameObject.SetActive(true);
            Context._pauseResume.gameObject.SetActive(true);
            Context._pauseQuit.gameObject.SetActive(true);
            _switchMenu();
            for (int i = 0; i < Context._playersHolder.childCount; i++)
            {
                PlayerController playercontroller = Context._playersHolder.GetChild(i).GetComponent<PlayerController>();
                playercontroller.enabled = false;
            }
            Time.timeScale = 0f;

        }

        public override void Update()
        {
            base.Update();
            if (_AnyBDown || _AnyPauseDown)
            {
                Services.AudioManager.PlaySound("event:/SFX/Menu/Select");
                TransitionTo<GameLoop>();
                return;
            }
            if (onresume && _AnyADown)
            {
                Services.AudioManager.PlaySound("event:/SFX/Menu/Select");
                TransitionTo<GameLoop>();
                return;
            }
            if (!onresume && _AnyADown)
            {
                Context._pauseWholeMask.GetComponent<Image>().DOFade(1f, 1f).OnComplete(() => SceneManager.LoadScene("ComicMenu"));
                Services.AudioManager.PlaySound("event:/SFX/Menu/Select");
                TransitionTo<GameQuitState>();
                return;
            }
            if (_AnyVLAxisRaw < -0.2f && !_vAxisInUse && !onresume)
            {
                onresume = true;
                Services.AudioManager.PlaySound("event:/SFX/Menu/Navigate");
                _switchMenu();
                return;
            }
            else if (_AnyVLAxisRaw > 0.2f && !_vAxisInUse && onresume)
            {
                onresume = false;
                Services.AudioManager.PlaySound("event:/SFX/Menu/Navigate");
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
                PlayerController playercontroller = Context._playersHolder.GetChild(i).GetComponent<PlayerController>();
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
            int numOfPlayers = _PlayersInformation.ColorIndex.Length;
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
                int colorIndex = Utility.GetColorIndexFromPlayer(Context._playersHolder.GetChild(i).gameObject);
                int rewiredID = Context.GetRewiredIDFromColorIndex(colorIndex);
                if (colorIndex < 2) Context._playersHolder.GetChild(i).position = _GameMapData.DuckLandingPostion[duckPosIndex++];
                else Context._playersHolder.GetChild(i).position = _GameMapData.ChickenLandingPosition[chickenPosIndex++];
                int temp = i;
                Context._playersHolder.GetChild(i).gameObject.SetActive(true);
                seq.Join(Context._playersHolder.GetChild(i).DOLocalMoveY(0.64f, _GameMapData.BirdsFlyDownDuration).SetDelay(_GameMapData.BirdsFlyDownDelay[temp]).SetEase(_GameMapData.BirdsFlyDownEase).
                OnComplete(() =>
                {
                    Services.GameFeelManager.ViberateController(rewiredID, 1f, 0.3f);
                    Services.AudioManager.PlaySound("event:/SFX/Menu/Landing");
                    _cam.transform.parent.GetComponent<DOTweenAnimation>().DORestartById("ShakeFree");
                }));
            }
            seq.AppendInterval(_GameMapData.FightDelay);
            seq.Append(Context._countDownText.DOScale(_GameMapData.FightScale / 2f, 0.8f).SetEase(Ease.InSine).OnPlay(() =>
            {
                Services.AudioManager.PlaySound("event:/SFX/Menu/Ready");
                Context._countDownText.text = Context._configData.ReadyString;
            }));
            seq.AppendInterval(0.3f);
            seq.Append(Context._countDownText.DOScale(0f, 0.2f));
            seq.Append(Context._countDownText.DOScale(_GameMapData.FightScale, _GameMapData.FightDuration).SetEase(_GameMapData.FightEase).OnPlay(() =>
            {
                Services.AudioManager.PlaySound("event:/SFX/Menu/Fight");
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
            /// Need to Enable everything that game loop has
            /// 1. Enable PlayerController, set rigidbody kinematic = false
            /// TODO: Color the players
            /// 2. Enable Camera
            /// 3. Send Game Start Event
            for (int i = 0; i < Context._playersHolder.childCount; i++)
            {
                int rewiredid = _PlayersInformation.RewiredID[i];
                PlayerController playercontroller = Context._playersHolder.GetChild(i).GetComponent<PlayerController>();
                playercontroller.enabled = true;
                playercontroller.Init(rewiredid);
                playercontroller.GetComponent<Rigidbody>().isKinematic = false;
            }
            Context._cam.GetComponent<CameraController>().enabled = true;
            EventManager.Instance.TriggerEvent(new GameStart());
        }
    }

    private class TutorialState : GameState
    {
        private bool _canHoldA;

        public override void OnEnter()
        {
            base.OnEnter();
            // Context._cam.GetComponent<AudioSource>().Play();
            // _GameMapData.BackgroundMusicMixer.SetFloat("Vol", 0f);
            // _GameMapData.BackgroundMusicMixer.SetFloat("Cutoff", 22000f);
            Context._darkCornerEffect.Length = 0f;
            Context._tutorialBackgroundMask.gameObject.SetActive(true);
            Sequence seq = DOTween.Sequence();
            seq.Append(Context._tutorialImage.DOScale(Vector3.one, _GameMapData.TutorialImageMoveInDuration).SetEase(_GameMapData.TutorialImageMoveInEase));
            seq.Append(Context._tutorialImage.DOScale(Vector3.one, 0f));
            for (int i = 0; i < Context._tutorialObjectiveImages.childCount; i++)
            {
                int x = i;
                seq.Append(Context._tutorialObjectiveImages.GetChild(x).DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            }
            seq.Append(Context._gameStartAButton.transform.DOScale(1f, _GameMapData.HoldAMoveInDuration).SetDelay(_GameMapData.HoldAMoveInDelay));
            seq.Join(Context._gameStartAImage.transform.DOScale(1f, _GameMapData.HoldAMoveInDuration).SetDelay(_GameMapData.HoldAMoveInDelay).OnComplete(() => _canHoldA = true));
        }
        public override void Update()
        {
            base.Update();
            if (_canHoldA && _AnyAHolding)
            {
                Context._gameStartHoldAImage.GetComponent<Image>().fillAmount += Time.deltaTime * _GameMapData.FillASpeed;
                if (Context._gameStartHoldAImage.GetComponent<Image>().fillAmount >= 1f)
                {
                    TransitionTo<LandingState>();
                    // TransitionTo<MVPEndPanelState>();
                    return;
                }
            }
            else
            {
                Context._gameStartHoldAImage.GetComponent<Image>().fillAmount -= Time.deltaTime * _GameMapData.FillASpeed;
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
            Context._gameStartAImage.SetActive(false);
            Context._gameStartAButton.SetActive(false);
            Context._gameStartHoldAImage.SetActive(false);
        }
    }
}
