using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

public class ArenaScoreboard : MonoBehaviour
{
	public static ArenaScoreboard instance;
	public TextMeshProUGUI TeamChickenScoreText;
	public TextMeshProUGUI TeamDuckScoreText;
	public GameObject TimerMinuteBoard;
	public GameObject TimerSecondsBoard;
	public GameObject TimerTenSecondsBoard;
	public UnityEvent OnChickenScoreEvent;
	public UnityEvent OnDuckScoreEvent;

	[HideInInspector]
	public int ChickenScore;
	[HideInInspector]
	public int DuckScore;
	[Tooltip("In Seconds")]
	public int TotalGameTime = 180;

	private void Awake()
	{
		instance = this;
	}

	public void StartTimer()
	{
		InvokeRepeating("_refreshTimer", 0f, 1f);
	}

	private void _refreshTimer()
	{
		// If Total Game Time is less than zero, meaning the game is over
		if (TotalGameTime <= 0)
		{
			EndGame();
		}
		// Need to refresh the timer board
		int seconds = TotalGameTime % 10;
		int tenseconds = (TotalGameTime % 60) / 10;
		int minute = TotalGameTime / 60;
		TimerSecondsBoard.GetComponentInChildren<TextMeshProUGUI>().text = seconds.ToString();
		TimerTenSecondsBoard.GetComponentInChildren<TextMeshProUGUI>().text = tenseconds.ToString();
		TimerMinuteBoard.GetComponentInChildren<TextMeshProUGUI>().text = minute.ToString();
		TotalGameTime--;
	}

	public void EndGame(GameObject _go)
	{
		CancelInvoke();
		GameManager.GM.GameOver(ChickenScore > DuckScore ? 1 : 0, _go);
		Camera.main.GetComponent<CameraController>().OnWinCameraZoom(_go.transform);

	}

	public void EndGame()
	{
		CancelInvoke();
		GameManager.GM.GameOver(ChickenScore > DuckScore ? 1 : 0, TimerMinuteBoard);
		//Camera.main.GetComponent<CameraController>().OnWinCameraZoom(TimerMinuteBoard.transform);

	}

	private void OnChickeScore()
	{
		OnScore(0, 2);
	}

	private void OnDuckScore()
	{
		OnScore(1, 2);
	}

	private void OnChickenSuicide()
	{
		OnScore(0, -1);
	}
	private void OnDuckSuicide()
	{
		OnScore(1, -1);
	}

	private void Team1ScoreBig()
	{
		OnScore(0, 10);
	}

	private void Team2ScoreBig()
	{
		OnScore(1, 10);
	}
	/// <summary>
	/// Score point, flip board
	/// </summary>
	/// <param name="team">0 is chicken, 1 is duck</param>
	private void OnScore(int team, int amount)
	{
		if (team == 0)
		{
			ChickenScore += amount;
			OnChickenScoreEvent.Invoke();
		}
		else
		{
			DuckScore += amount;
			OnDuckScoreEvent.Invoke();
		}
	}

	public void RefreshScore()
	{
		TeamChickenScoreText.text = ChickenScore.ToString();
		TeamDuckScoreText.text = DuckScore.ToString();
	}

	private void OnEnable()
	{
		EventManager.StartListening("GameStart", StartTimer);
		EventManager.StartListening("OnTeam1Score", OnChickeScore);
		EventManager.StartListening("OnTeam2Score", OnDuckScore);
		EventManager.StartListening("OnTeam1Suicide", OnChickenSuicide);
		EventManager.StartListening("OnTeam2Suicide", OnDuckSuicide);
		EventManager.StartListening("Team1ScoreBig", Team1ScoreBig);
		EventManager.StartListening("Team2ScoreBig", Team2ScoreBig);

	}

	private void OnDisable()
	{
		EventManager.StopListening("GameStart", StartTimer);
		EventManager.StopListening("OnTeam1Score", OnChickeScore);
		EventManager.StopListening("OnTeam2Score", OnDuckScore);
		EventManager.StopListening("OnTeam1Suicide", OnChickenSuicide);
		EventManager.StopListening("OnTeam2Suicide", OnDuckSuicide);
		EventManager.StopListening("Team1ScoreBig", Team1ScoreBig);
		EventManager.StopListening("Team2ScoreBig", Team2ScoreBig);
	}
}
