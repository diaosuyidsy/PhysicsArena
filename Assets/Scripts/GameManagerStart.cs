using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Rewired;

public class GameManagerStart : MonoBehaviour
{
	public static GameManagerStart GMS;
	public GameObject WinningObj;
	public GameObject Payload;
	public GameObject Or;
	public GameObject Food;
	public GameObject PressAToSpawn;
	public GameObject CarIndicator;
	public GameObject FoodIndicator;
	public GameObject BasketIndicator;
	public GameObject HoldAToSkip;
	private float[] _playerHoldRestart;
	private bool _skipped = true;
	private float _timeForBlink;
	private Text _restartText1;
	private Text _restartText2;


	private void Awake()
	{
		GMS = this;
		_playerHoldRestart = new float[6];
		_timeForBlink = 0f;
		_restartText1 = HoldAToSkip.GetComponent<Text>();
		_restartText2 = HoldAToSkip.transform.GetChild(0).GetComponent<Text>();
	}

	private IEnumerator PlayIntro()
	{
		// Inside this if Block is the all tutorial content
		HoldAToSkip.SetActive(true);
		yield return new WaitForSeconds(1f);

		WinningObj.SetActive(true);

		yield return new WaitForSeconds(5f);

		Payload.SetActive(true);
		CarIndicator.SetActive(true);

		yield return new WaitForSeconds(7f);
		CarIndicator.SetActive(false);
		Or.SetActive(true);

		yield return new WaitForSeconds(3f);
		StartCoroutine(foodLightHelper());
		Food.SetActive(true);

		yield return new WaitForSeconds(4f);
		BasketIndicator.SetActive(false);

		_skipped = true;
		//PressAToSpawn.SetActive(true);

		//SceneManager.LoadSceneAsync(1);
		HoldAToSkip.SetActive(false);
		GameManager.GM.FillPlayerInformation();
		GameManager.GM.GetComponent<InputController>().enabled = true;
		GameManager.GM.GetComponent<InputController>().AllEnterGame();
	}

	private void Update()
	{
		if (!_skipped)
		{
			_updateAlpha();
			_holdToSkip();
		}
	}

	private void _holdToSkip()
	{
		for (int i = 0; i < 6; i++)
		{
			if (ReInput.players.GetPlayer(i).GetButton("Jump"))
			{
				_playerHoldRestart[i] += Time.deltaTime;
				if (Mathf.Abs(_playerHoldRestart[i] - 1f) < 0.2f)
				{
					_skipped = true;
					// What to skip
					StopAllCoroutines();
					WinningObj.SetActive(false);
					Payload.SetActive(false);
					CarIndicator.SetActive(false);
					Or.SetActive(false);
					FoodIndicator.SetActive(false);
					BasketIndicator.SetActive(false);
					Food.SetActive(false);
					HoldAToSkip.SetActive(false);

					PressAToSpawn.SetActive(false);

					GameManager.GM.FillPlayerInformation();
					GameManager.GM.GetComponent<InputController>().enabled = true;
					GameManager.GM.GetComponent<InputController>().AllEnterGame();
				}
			}
			else
			{
				_playerHoldRestart[i] = 0f;
			}
		}
	}

	private void _updateAlpha()
	{
		float BlinkAlpha = (0.5f + 0.5f * Mathf.Cos(_timeForBlink));
		Color textColor1 = _restartText1.color;
		Color textColor2 = _restartText2.color;
		textColor1.a = BlinkAlpha;
		textColor2.a = BlinkAlpha;
		_restartText1.color = textColor1;
		_restartText2.color = textColor2;
		_timeForBlink += Time.deltaTime * 2f;
	}

	public void StartIntro()
	{
		_skipped = false;
		StartCoroutine(PlayIntro());
	}

	IEnumerator foodLightHelper()
	{
		FoodIndicator.SetActive(true);
		yield return new WaitForSeconds(1f);
		FoodIndicator.SetActive(false);
		BasketIndicator.SetActive(true);
	}
}
