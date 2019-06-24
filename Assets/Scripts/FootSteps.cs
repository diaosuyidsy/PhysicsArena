using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSteps : MonoBehaviour
{
	private PlayerController _playerController;

	private void Awake()
	{
		_playerController = transform.parent.GetComponentInChildren<PlayerController>();
		Debug.Assert(_playerController != null);
	}

	public void OnLegStraight()
	{
		_playerController.FootStep();
	}
}
