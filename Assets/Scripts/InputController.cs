using UnityEngine;
using Rewired;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InputController : MonoBehaviour
{
	public GameObject RestartText;
	private Player[] _players;
	private bool[] _enteredGame;
	private int playerNum = 0;
	private bool _canRestart = false;
	private float _timeForBlink = 0f;
	private Text _restartText1;
	private Text _restartText2;
	private float[] _playerHoldRestart;
	private int _chickenCounter = 0;
	private int _duckCounter = 3;

	private void Awake()
	{
		_players = new Player[6];
		_enteredGame = new bool[6];
		for (int i = 0; i < 6; i++)
		{
			_players[i] = ReInput.players.GetPlayer(i);
		}
		_restartText1 = RestartText.GetComponent<Text>();
		_restartText2 = RestartText.transform.GetChild(0).GetComponent<Text>();
		RestartText.SetActive(false);
		_playerHoldRestart = new float[6];
	}

	private void Update()
	{
		//for (int i = 0; i < 6; i++)
		//{
		//	if (!_enteredGame[i] && _players[i].GetButton("Jump"))
		//	{
		//		_enteredGame[i] = true;
		//		EnterGame(i);
		//	}
		//}
		if (_canRestart)
		{
			// Update Alpha Value of the Restart Text
			_updateAlpha();
			_holdToRestart();
		}
	}

	public void AllEnterGame()
	{
		// Start Generating Weapon
		WeaponGenerationManager.WGM.StartGeneratingWeapon();
		//for (int i = 0; i < ReInput.controllers.joystickCount; i++)
		//{
		//	if (ReInput.controllers.Joysticks[i].isConnected)
		//	{
		//		EnterGame(i);
		//	}
		//}

		foreach (Joystick a in ReInput.controllers.Joysticks)
		{
			EnterGame(a.id);
		}
		//for (int i = 0; i < ReInput.controllers.joystickCount; i++)
		//{
		//	EnterGame(i);
		//}
	}

	private void EnterGame(int playerID)
	{
		GameObject curPlayer = _getCharacter(playerID);
		curPlayer.GetComponent<PlayerController>().Init(playerID);
		curPlayer.transform.parent.parent.gameObject.SetActive(true);
		if (GameManager.GM.Players.Count <= playerNum) GameManager.GM.Players.Add(curPlayer);
		GameManager.GM.Players[playerNum] = curPlayer;
		playerNum++;
		// Disable the Press A to spawn text
		GameManagerStart.GMS.PressAToSpawn.SetActive(false);
	}

	private GameObject _getCharacter(int playerNum)
	{
		GameObject chara = null;
		string characterName = GameManager.GM.PlayersInformation[playerNum].PlayerName;
		// On the Chicken Team
		if (characterName == "Yellow" || characterName == "Pink" || characterName == "Orange")
		{
			chara = GameManager.GM.APlayers[_chickenCounter++];
		}
		else
		{
			// On the Duck Team
			chara = GameManager.GM.APlayers[_duckCounter++];
		}
		// Decorate the character
		chara.transform.GetChild(2).GetComponent<SkinnedMeshRenderer>().material = GameManager.GM.NameToMaterialsDict[characterName];
		return chara.GetComponentInChildren<PlayerController>().gameObject;
	}

	private void _holdToRestart()
	{
		for (int i = 0; i < 6; i++)
		{
			if (ReInput.players.GetPlayer(i).GetButton("Jump"))
			{
				_playerHoldRestart[i] += Time.deltaTime;
				if (Mathf.Abs(_playerHoldRestart[i] - 1f) < 0.2f)
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

	// This function is called from GameManager EndGame
	public void CanRestart()
	{
		_canRestart = true;
		RestartText.SetActive(true);
	}
}
