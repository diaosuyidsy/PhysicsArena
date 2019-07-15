using System.Collections;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{
	[Header("Player Data Section")]
	public CharacterData CharacterDataStore;
	[Header("Player Body Setting Section")]
	public GameObject LegSwingReference;
	public GameObject Chest;
	[Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
	public GameObject[] LeftArms;
	[Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
	public GameObject[] RightArms;
	public GameObject LeftHand;
	public GameObject RightHand;
	public GameObject TurnReference;
	public GameObject[] OnDeathHidden;
	[HideInInspector] public GameObject MeleeVFXHolder;

	[Header("Auxillary Aiming Section")]
	public bool EnableAuxillaryAiming = true;
	public float MaxWeaponCD = 0.3f;

	[HideInInspector]
	public GameObject HandObject;
	[HideInInspector]
	public float MeleeCharge = 0f;
	[HideInInspector]
	public float DropCharge = 0f;
	[HideInInspector]
	public bool IsPunching = false;
	[HideInInspector]
	public bool HandTaken = false;
	[HideInInspector]
	public bool IsOnGround = false;
	[HideInInspector]
	public bool IsOccupied = false;

	[Header("Block VFX & UI")]
	public GameObject BlockVFX;
	public GameObject BlockUI;
	public GameObject BlockUIFill;

	#region Statistics Variables
	public int PlayerNumber;
	public GameObject EnemyWhoHitPlayer;
	#endregion

	#region States
	private enum State
	{
		Empty,
		Walking,
		Jumping,
		Picking,
		Holding,
		MachineGuning,
		Dead,
		Shooting,
		Meleeing,
		Blocking,
	}
	// Normal State should include: Walking, Jumping, Picking, Holding, Dead
	private State normalState;
	// Attack State Should include: Shooting, Meleeing, Blocking
	private State attackState;
	#endregion

	#region Private Variable
	private Player _player;
	private Rigidbody _rb;
	private float _distToGround;
	private HingeJoint _chesthj;
	private HingeJoint _leftArm2hj;
	private HingeJoint _rightArm2hj;
	private HingeJoint _leftArmhj;
	private HingeJoint _rightArmhj;
	private HingeJoint _leftHandhj;
	private HingeJoint _rightHandhj;
	private float RotationSpeed;
	private bool _checkArm = true;
	private string _rightTriggerRegister = "";
	private bool _canControl = true;
	private Vector3 _freezeBody;
	private float _previousFrameVel = 0f;
	private float _weaponCD;
	[SerializeField]
	private float _axuillaryMaxDistance = 30f;
	[SerializeField]
	private float _auxillaryMaxAngle = 5f;
	private bool _auxillaryRotationLock = false;
	private bool _dropping = false;
	private float _blockCharge = 0f;
	private bool _blockCanRegen = false;
	private bool _isJumping = false;
	#endregion

	#region Private Timer Variable
	// Linked to BlockRegenInterval
	private float timer_blockregen = 0f;
	private bool starttimer_blockregen = false;
	// Linked to Statistics
	private float timer_hitMarkerStayTime = 0f;
	private bool starttimer_hitmakrer = false;
	// Linked to Melee Punch Hold
	private float timer_meleeHoldTime = 0f;
	private bool starttimer_meleeHold = false;
	#endregion

	[Header("Debug Section: Please Touch Me~")]
	#region Debug Toggle
	public bool debugT_CheckArm = true;
	#endregion

	public void Init(int controllerNumber)
	{
		PlayerNumber = controllerNumber;
		_player = ReInput.players.GetPlayer(controllerNumber);
		IsOccupied = true;
	}
	/// <summary>
	/// should be deleted when done prototyping
	/// </summary>
	/// 
	private void Init()
	{
		_player = ReInput.players.GetPlayer(PlayerNumber);
		IsOccupied = true;
	}
	private void Start()
	{
		_rb = GetComponent<Rigidbody>();
		_distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
		_chesthj = Chest.GetComponent<HingeJoint>();
		_leftArm2hj = LeftArms[0].GetComponent<HingeJoint>();
		_rightArm2hj = RightArms[0].GetComponent<HingeJoint>();
		_leftArmhj = LeftArms[1].GetComponent<HingeJoint>();
		_rightArmhj = RightArms[1].GetComponent<HingeJoint>();
		_leftHandhj = LeftArms[2].GetComponent<HingeJoint>();
		_rightHandhj = RightArms[2].GetComponent<HingeJoint>();
		_freezeBody = new Vector3(0, transform.localEulerAngles.y, 0);
		LegSwingReference.GetComponent<Animator>().SetFloat("WalkSpeedMultiplier", 1f);
		Init();
	}

	// Update is called once per frame
	void Update()
	{
		RunTimer();
		if (!_canControl || !IsOccupied)
			return;

		CheckRewiredInput();
		CheckJump();
		if (_checkArm && debugT_CheckArm)
			CheckArm();
		if (attackState == State.Shooting && EnableAuxillaryAiming)
			AuxillaryAim();
		CheckFire();
		CheckDrop();
		CheckBlock();
	}

	// This is primarily for dropping item when velocity change too much 
	private void FixedUpdate()
	{
		if (_canControl || !IsOccupied)
		{
			CheckMovement();
		}
		if (_rb.velocity.magnitude >= CharacterDataStore.CharacterMovementDataStore.DropWeaponVelocityThreshold)
			DropHelper();

	}

	// Late Update is for standing the character
	private void LateUpdate()
	{
		if (!_canControl) return;
		_freezeBody.y = transform.localEulerAngles.y;
		transform.localEulerAngles = _freezeBody;
	}

	// OnEnterDeathZone controls the behavior how player reacts when it dies
	// It's called immediately after player enters death zone
	public void OnEnterDeathZone(PlayerDied pd)
	{
		if (pd.Player != gameObject) return;
		_canControl = false;
		if (attackState == State.Meleeing)
		{
			starttimer_meleeHold = false;
			timer_meleeHoldTime = 0f;
			attackState = State.Empty;
			// This is add a push force when melee
			_rb.AddForce(transform.forward * MeleeCharge * CharacterDataStore.CharacterMeleeDataStore.SelfPushForce, ForceMode.Impulse);
			StopAllCoroutines();

			StartCoroutine(MeleePunchLeftHandHelper(CharacterDataStore.CharacterMeleeDataStore.FistReleaseTime, _leftArmhj));
			StartCoroutine(MeleePunchHelper(_rightArmhj, _rightHandhj, CharacterDataStore.CharacterMeleeDataStore.FistReleaseTime));
			EventManager.Instance.TriggerEvent(new PunchReleased(gameObject, PlayerNumber));
		}
		_auxillaryRotationLock = false;
		foreach (GameObject go in OnDeathHidden)
		{
			go.SetActive(false);
		}
		DropHelper();
		StatsAfterDeath();
		StartCoroutine(Respawn(CharacterDataStore.CharacterMovementDataStore.RespawnTime));

	}

	private void StatsAfterDeath()
	{
		// First we need to record who killed the player
		// If no one killed him, then he commited suicide
		if (EnemyWhoHitPlayer == null)
		{
			if (PlayerNumber < GameManager.GM.SuicideRecord.Count)
			{
				GameManager.GM.SuicideRecord[PlayerNumber]++;
			}
			else
			{
				Debug.LogError("Something is wrong with the controller number");
			}

			print("Player" + PlayerNumber + "Has Committed Suicide for " + GameManager.GM.SuicideRecord[PlayerNumber] + " Times");

			//EventManager.TriggerEvent("On" + tag + "Suicide");
		}
		else
		{
			if (!EnemyWhoHitPlayer.CompareTag(tag))
			{
				// Record Enemy Killed another player
				int killer = EnemyWhoHitPlayer.GetComponent<PlayerController>().PlayerNumber;

				if (PlayerNumber < GameManager.GM.KillRecord.Count)
				{
					GameManager.GM.KillRecord[killer]++;
				}
				else
				{
					Debug.LogError("Something is wrong with the controller number");
				}
				print("Player" + PlayerNumber + "Was Killed By Player" + EnemyWhoHitPlayer.GetComponent<PlayerController>().PlayerNumber +
					" and it has killed " + GameManager.GM.KillRecord[killer] + " Players");
				//EventManager.TriggerEvent("On" + EnemyWhoHitPlayer.tag + "Score");
			}
			else
			{
				int muderer = EnemyWhoHitPlayer.GetComponent<PlayerController>().PlayerNumber;
				if (PlayerNumber < GameManager.GM.TeammateMurderRecord.Count)
				{
					GameManager.GM.TeammateMurderRecord[muderer]++;
				}
				else
				{
					Debug.LogError("Something is wrong with the controller number");
				}
				print("Player" + PlayerNumber + "Was conspired and murdered By Player" + EnemyWhoHitPlayer.GetComponent<PlayerController>().PlayerNumber +
					" and it has killed " + GameManager.GM.TeammateMurderRecord[muderer] + " Teammates");
				//EventManager.TriggerEvent("On" + tag + "Suicide");

			}

		}
		// Need to clean up the marker and stuff
		EnemyWhoHitPlayer = null;
		timer_hitMarkerStayTime = 0f;
		starttimer_hitmakrer = false;
	}

	IEnumerator Respawn(float time)
	{
		_rb.isKinematic = true;
		GameManager.GM.SetToRespawn(gameObject, 5f);
		yield return new WaitForSeconds(time);
		_rb.isKinematic = false;
		GameManager.GM.SetToRespawn(gameObject, 0f);
		foreach (GameObject go in OnDeathHidden)
		{
			go.SetActive(true);
		}
		_canControl = true;
		EventManager.Instance.TriggerEvent(new PlayerRespawned(gameObject));
	}

	private void CheckDrop()
	{
		// Drop Only happens when player is holding something
		if (HandObject == null || normalState != State.Holding || attackState == State.Shooting)
			return;

		// If taken something, and pushed LT, drop the thing
		if (_player.GetButton("Left Trigger"))
		{
			_dropping = true;
			//DropHelper ();
			StartCoroutine(PowerDrop(2f));
		}
		else
		{
			if (_dropping)
			{
				StopAllCoroutines();
				JointSpring js = _chesthj.spring;
				js.targetPosition = 10f * DropCharge;
				DropCharge = 0f;
				_chesthj.spring = js;
				normalState = State.Empty;
				_dropping = false;
				DropHelper();
			}
		}
	}

	IEnumerator PowerDrop(float time)
	{
		float elapsedTime = 0f;
		JointSpring js = _chesthj.spring;

		float initspringtagetPosition = js.targetPosition;

		while (elapsedTime <= time)
		{
			DropCharge = elapsedTime / time;
			elapsedTime += Time.deltaTime;
			js.targetPosition = Mathf.Lerp(initspringtagetPosition, -100f, elapsedTime / time);
			_chesthj.spring = js;
			yield return new WaitForEndOfFrame();
		}

	}

	public void DropHelper()
	{
		if (HandObject == null) return;
		// Drop the thing
		HandObject.SendMessage("Drop");
		normalState = State.Empty;
		_dropping = false;
		// Change to non-dropping state after a while
		HandTaken = false;
		// Return the body to normal position
		_checkArm = true;
		StopAllCoroutines();
		ResetBody();
		// These two are necessary for all objects
		HandObject.GetComponent<Rigidbody>().isKinematic = false;
		HandObject.layer = LayerMask.NameToLayer("Pickup");
		// Specialized checking
		if (HandObject.CompareTag("Weapon"))
		{
			// Stop the shooting
			HandObject.GetComponent<WeaponBase>().Fire(false);
			// Disable the UI
			HandObject.SendMessage("KillUI");
		}
		if (HandObject.CompareTag("Team1Resource") || HandObject.CompareTag("Team2Resource"))
		{
			HandObject.SendMessage("RegisterLastHolder", PlayerNumber);
		}
		// Nullify the holder
		HandObject = null;
		// Change the speed back to normal
		LegSwingReference.GetComponent<Animator>().SetFloat("WalkSpeedMultiplier", 1f);
		// Clear the right trigger register
		_rightTriggerRegister = "";
		// Set Auxillary Aim to false
		_auxillaryRotationLock = false;
	}

	public void CheckFire()
	{
		// If player push the button down
		if (_player.GetButtonDown("Right Trigger"))
		{
			switch (_rightTriggerRegister)
			{
				case "Weapon":
					if (HandObject != null && !_dropping)
					{
						attackState = State.Shooting;
						if (EnableAuxillaryAiming)
							AuxillaryAim();
					}
					break;
				case "Hook":
					if (HandObject != null && !_dropping)
					{
						AuxillaryAimOnce(DesignPanelManager.DPM.HookGunAuxillaryAimSlider.value);
					}
					break;
				case "FistGun":
					if (HandObject != null && !_dropping)
					{
						AuxillaryAimOnce(DesignPanelManager.DPM.HookGunAuxillaryAimSlider.value);
					}
					break;
				default:
					//If we don't have anything on hand, we are applying melee action
					if (attackState != State.Meleeing && attackState != State.Blocking && normalState != State.Picking && normalState != State.Holding)
					{
						attackState = State.Meleeing;
						_checkArm = false;
						StopAllCoroutines();
						StartCoroutine(MeleeClockFistLeftHandHelper(CharacterDataStore.CharacterMeleeDataStore.ClockFistTime, _leftArmhj));
						StartCoroutine(MeleeClockFistHelper(_rightArm2hj, _rightArmhj, _rightHandhj, CharacterDataStore.CharacterMeleeDataStore.ClockFistTime));
					}
					break;
			}
			if (HandObject != null && !_dropping)
			{
				HandObject.GetComponent<WeaponBase>().Fire(true);
			}
		}

		// If player lift the button up
		if (_player.GetButtonUp("Right Trigger"))
		{
			if (HandObject != null && !_dropping)
			{
				HandObject.GetComponent<WeaponBase>().Fire(false);
			}
			switch (_rightTriggerRegister)
			{
				case "Weapon":
					attackState = State.Empty;
					// Auxillary Aiming
					_auxillaryRotationLock = false;
					_weaponCD = 0f;
					break;
				default:
					// If we previously started melee and released the trigger, then release the fist
					if (attackState == State.Meleeing)
					{
						starttimer_meleeHold = false;
						timer_meleeHoldTime = 0f;
						attackState = State.Empty;
						// This is add a push force when melee
						_rb.AddForce(transform.forward * MeleeCharge * CharacterDataStore.CharacterMeleeDataStore.SelfPushForce, ForceMode.Impulse);
						StopAllCoroutines();

						StartCoroutine(MeleePunchLeftHandHelper(CharacterDataStore.CharacterMeleeDataStore.FistReleaseTime, _leftArmhj));
						StartCoroutine(MeleePunchHelper(_rightArmhj, _rightHandhj, CharacterDataStore.CharacterMeleeDataStore.FistReleaseTime));
						EventManager.Instance.TriggerEvent(new PunchReleased(gameObject, PlayerNumber));
					}
					_auxillaryRotationLock = false;
					break;
			}
		}

		// If players are holding the Right Trigger button
		if (_player.GetButton("Right Trigger"))
		{
			// Means we want to fire
			switch (_rightTriggerRegister)
			{
				case "Team1Resource":
				case "Team2Resource":
					if (!_dropping)
						DropHelper();
					break;
			}
		}
	}

	public void CheckBlock()
	{
		// ShieldEnergy is the percentage of shield energy. 
		float _shieldEnergy = (CharacterDataStore.CharacterBlockDataStore.MaxBlockCD - _blockCharge) / CharacterDataStore.CharacterBlockDataStore.MaxBlockCD;

		if (!_player.GetButton("Block") && _blockCanRegen)
		{
			_blockCharge -= (Time.deltaTime * CharacterDataStore.CharacterBlockDataStore.BlockRegenRate);
			if (_blockCharge <= 0f) _blockCanRegen = false;
		}

		if (attackState == State.Meleeing || attackState == State.Shooting || normalState == State.Picking || normalState == State.Holding
			|| normalState == State.Dead) return;

		if (_player.GetButtonDown("Block") && _blockCharge <= CharacterDataStore.CharacterBlockDataStore.MaxBlockCD)
		{
			attackState = State.Blocking;

			BlockVFX.SetActive(true);
			BlockUI.SetActive(true);
		}

		// if Player hold the button, check if player could block then block
		if (_player.GetButton("Block") && attackState == State.Blocking)
		{
			_blockCanRegen = false;
			BlockHelper(_leftArmhj, _leftHandhj);
			BlockHelper(_rightArmhj, _rightHandhj);
			_blockCharge += Time.deltaTime;
			if (_blockCharge > CharacterDataStore.CharacterBlockDataStore.MaxBlockCD)
			{
				attackState = State.Empty;
				starttimer_blockregen = true;
				timer_blockregen = 0f;
				ResetBody();
				BlockVFX.SetActive(false);
			}

			// Change BlockFill UI scale
			BlockUIFill.transform.localScale = new Vector3(_shieldEnergy, 1f, 1f);
		}

		// When player released the button, should skip blockregeninterval seconds before it can regen
		if (_player.GetButtonUp("Block") && _blockCharge <= CharacterDataStore.CharacterBlockDataStore.MaxBlockCD)
		{
			starttimer_blockregen = true;
			timer_blockregen = 0f;
			ResetBody();
			attackState = State.Empty;
		}

		if (_player.GetButtonUp("Block"))
		{
			BlockVFX.SetActive(false);
			BlockUI.SetActive(false);
		}
	}

	// All timers should be contained in this function
	// And this function could not be stopped by universal stop sign _canControlCharacter
	private void RunTimer()
	{
		if (starttimer_blockregen)
		{
			timer_blockregen += Time.deltaTime;
			if (timer_blockregen > CharacterDataStore.CharacterBlockDataStore.BlockRegenInterval)
			{
				starttimer_blockregen = false;
				timer_blockregen = 0f;
				_blockCanRegen = true;
			}
		}
		// This is for hit statistics
		// Player hitter marker only stays on a the player for no more than 2s
		if (starttimer_hitmakrer)
		{
			timer_hitMarkerStayTime += Time.deltaTime;
			if (timer_hitMarkerStayTime > 3f)
			{
				starttimer_hitmakrer = false;
				timer_hitMarkerStayTime = 0f;
				EnemyWhoHitPlayer = null;
			}
		}

		if (starttimer_meleeHold)
		{
			timer_meleeHoldTime += Time.deltaTime;
			if (timer_meleeHoldTime > CharacterDataStore.CharacterMeleeDataStore.MeleeHoldTime)
			{
				// Release the punch when hold time exceeds
				starttimer_meleeHold = false;
				timer_meleeHoldTime = 0f;
				// If we previously started melee and released the trigger, then release the fist
				if (attackState == State.Meleeing && _canControl)
				{
					attackState = State.Empty;
					// This is add a push force when melee
					_rb.AddForce(transform.forward * MeleeCharge * CharacterDataStore.CharacterMeleeDataStore.SelfPushForce, ForceMode.Impulse);
					StopAllCoroutines();

					StartCoroutine(MeleePunchLeftHandHelper(CharacterDataStore.CharacterMeleeDataStore.FistReleaseTime, _leftArmhj));
					StartCoroutine(MeleePunchHelper(_rightArmhj, _rightHandhj, CharacterDataStore.CharacterMeleeDataStore.FistReleaseTime));
					EventManager.Instance.TriggerEvent(new PunchReleased(gameObject, PlayerNumber));
				}
				_auxillaryRotationLock = false;
			}
		}
	}

	private void AuxillaryAim()
	{
		// Auxillary Aiming
		_weaponCD += Time.deltaTime;
		if (_weaponCD <= MaxWeaponCD)
		{
			GameObject target = null;
			float minAngle = 360f;
			foreach (GameObject otherPlayer in GameManager.GM.Players)
			{
				if (otherPlayer != null && !otherPlayer.CompareTag(tag))
				{
					// If other player are within max Distance, then check for the smalliest angle player
					if (Vector3.Distance(otherPlayer.transform.position, gameObject.transform.position) <= _axuillaryMaxDistance)
					{
						Vector3 targetDir = otherPlayer.transform.position - transform.position;
						float angle = Vector3.Angle(targetDir, transform.forward);
						if (angle <= _auxillaryMaxAngle && angle < minAngle)
						{
							minAngle = angle;
							target = otherPlayer;
						}
					}
				}
			}
			// Now we got the target Player, time to auxillary against it
			if (target != null)
			{
				_auxillaryRotationLock = true;
				transform.LookAt(target.transform);
			}
		}
		else
		{
			_auxillaryRotationLock = false;
		}
	}
	private void AuxillaryAimOnce(float maxangle)
	{
		GameObject target = null;
		float minAngle = 360f;
		foreach (GameObject otherPlayer in GameManager.GM.Players)
		{
			if (otherPlayer != null && !otherPlayer.CompareTag(tag))
			{
				// If other player are within max Distance, then check for the smalliest angle player
				if (Vector3.Distance(otherPlayer.transform.position, gameObject.transform.position) <= _axuillaryMaxDistance)
				{
					Vector3 targetDir = otherPlayer.transform.position - transform.position;
					float angle = Vector3.Angle(targetDir, transform.forward);
					if (angle <= maxangle && angle < minAngle)
					{
						minAngle = angle;
						target = otherPlayer;
					}
				}
			}
		}
		// Now we got the target Player, time to auxillary against it
		if (target != null)
		{
			transform.LookAt(target.transform);
			if (HandObject != null)
			{
				HandObject.transform.eulerAngles = transform.eulerAngles + new Vector3(0f, 90f, 0f);
			}
		}
	}
	public void OnPickUpItem(string tag)
	{
		// Actual Logic Below
		_rightTriggerRegister = tag;
		// if pick up resource, then slow down

		switch (tag)
		{
			case "Team1Resource":
			case "Team2Resource":
			case "WoodStamp":
			case "SuckGun":
			case "Bazooka":
			case "Weapon":
				_checkArm = false;

				// Bend the body back
				JointSpring tempjs = _chesthj.spring;
				tempjs.targetPosition = -5f;
				_chesthj.spring = tempjs;

				StartCoroutine(PickUpWeaponHelper(_leftArm2hj, _leftArmhj, true, 0.1f));
				StartCoroutine(PickUpWeaponHelper(_rightArm2hj, _rightArmhj, false, 0.1f));
				break;
			case "Hook":
			case "FistGun":
				_checkArm = false;

				// Bend the body back
				JointSpring tempjs1 = _chesthj.spring;
				tempjs1.targetPosition = -5f;
				_chesthj.spring = tempjs1;

				StartCoroutine(PickUpWeaponHalfHelper(_leftArmhj, 0.1f));
				StartCoroutine(PickUpWeaponHalfHelper(_rightArmhj, 0.1f));
				break;
			default:
				break;
		}
	}

	private void CheckArm()
	{
		// Arm2: right max: 90 --> -74
		//       left min: -75 --> 69
		// Arm: Connected Mass Scale 1 --> 0
		//      Target Position: 0 --> 180
		//      Limits: max 90 --> 121
		// Hand: Limit Max: 90 --> 0
		if (attackState == State.Meleeing || normalState == State.Holding) return;

		bool LTPushDown = _player.GetButton("Left Trigger");
		bool LTLiftUp = _player.GetButtonUp("Left Trigger");

		LeftHand.GetComponent<Fingers>().SetTaken(!LTPushDown || _dropping);
		RightHand.GetComponent<Fingers>().SetTaken(!LTPushDown || _dropping);

		if (LTPushDown)
			normalState = State.Picking;
		else
			normalState = State.Empty;

		CheckArmHelper(LTPushDown, _leftArm2hj, _leftArmhj, _leftHandhj, true);
		CheckArmHelper(LTPushDown, _rightArm2hj, _rightArmhj, _rightHandhj, false);

		// Bend the body all together
		JointSpring tempjs = _chesthj.spring;
		tempjs.targetPosition = (LTPushDown ? 1f : 0f) * 90f;
		tempjs.targetPosition = Mathf.Clamp(tempjs.targetPosition, _chesthj.limits.min + 5, _chesthj.limits.max - 5);
		_chesthj.spring = tempjs;
	}

	// If sender is not null, meaning the hit could be blocked
	public void OnMeleeHit(Vector3 force, float _meleeCharge, GameObject sender = null)
	{
		// First check if the player could block the attack
		if (sender != null && attackState == State.Blocking && AngleWithin(transform.forward, sender.transform.forward, 180f - CharacterDataStore.CharacterBlockDataStore.BlockAngle))
		{
			sender.GetComponentInParent<PlayerController>().OnMeleeHit(-force * CharacterDataStore.CharacterBlockDataStore.BlockMultiplier, _meleeCharge);
			// Statistics: Block Success
			if (PlayerNumber < GameManager.GM.BlockTimes.Count)
			{
				GameManager.GM.BlockTimes[PlayerNumber]++;
			}
			else
			{
				Debug.LogError("Something is wrong with the controller number");
			}
			// Statistics: Kill
			sender.GetComponentInParent<PlayerController>().Mark(gameObject);
			// End Statistics
		}
		else // Player is hit cause he could not block
		{
			EventManager.Instance.TriggerEvent(new PlayerHit(sender, gameObject, force, (sender == null) ? -1 : sender.GetComponent<PlayerController>().PlayerNumber, PlayerNumber, _meleeCharge));

			_rb.AddForce(force, ForceMode.Impulse);
		}

	}

	// This function is used for marking and statistics
	public void Mark(GameObject enforcer)
	{
		EnemyWhoHitPlayer = enforcer;
		timer_hitMarkerStayTime = 0f;
		starttimer_hitmakrer = true;
	}

	private void CheckJump()
	{
		if (_player.GetButtonDown("Jump") && IsGrounded())
		{
			_rb.AddForce(new Vector3(0, CharacterDataStore.CharacterMovementDataStore.JumpForce, 0), ForceMode.Impulse);
			_isJumping = true;
			EventManager.Instance.TriggerEvent(new PlayerJump(gameObject, GetComponentInChildren<UIController>().UI.gameObject, PlayerNumber, GetGroundTag()));
			OnDeathHidden[3].SetActive(false);
		}
	}

	public void ApplyWalkForce(float _force)
	{
		//string HLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis1";
		//string VLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis2";
		float HLAxis = _player.GetAxis("Move Horizontal");
		float VLAxis = _player.GetAxis("Move Vertical");

		if (IsGrounded() && (!Mathf.Approximately(HLAxis, 0f) || !Mathf.Approximately(VLAxis, 0f)))
			_rb.AddForce(transform.forward * _force, ForceMode.Impulse);
	}

	private void CheckMovement()
	{
		float HLAxis = _player.GetAxis("Move Horizontal");
		float VLAxis = _player.GetAxis("Move Vertical");

		if (!Mathf.Approximately(HLAxis, 0f) || !Mathf.Approximately(VLAxis, 0f))
		{
			var isOnGround = IsGrounded();
			var isfacingcliff = IsFacingCliff();
			// Get the percent of input force player put in
			float normalizedInputVal = Mathf.Sqrt(Mathf.Pow(HLAxis, 2f) + Mathf.Pow(VLAxis, 2f)) / Mathf.Sqrt(2);
			// Add force based on that percentage
			var targetVelocity = transform.forward;
			targetVelocity *= CharacterDataStore.CharacterMovementDataStore.WalkSpeed;

			var velocity = _rb.velocity;
			var velocityChange = (targetVelocity - velocity);
			velocityChange.x = Mathf.Clamp(velocityChange.x, -CharacterDataStore.CharacterMovementDataStore.MaxVelocityChange, CharacterDataStore.CharacterMovementDataStore.MaxVelocityChange);
			velocityChange.z = Mathf.Clamp(velocityChange.z, -CharacterDataStore.CharacterMovementDataStore.MaxVelocityChange, CharacterDataStore.CharacterMovementDataStore.MaxVelocityChange);
			velocityChange.y = 0f;
			if (isOnGround && !isfacingcliff)
			{
				_rb.AddForce(velocityChange, ForceMode.VelocityChange);
			}
			else if (isOnGround && isfacingcliff)
			{
				_rb.AddForce(velocityChange * CharacterDataStore.CharacterMovementDataStore.FacingCliffMultiplier, ForceMode.VelocityChange);
			}
			else
			{
				_rb.AddForce(velocityChange * CharacterDataStore.CharacterMovementDataStore.InAirSpeedMultiplier, ForceMode.VelocityChange);
			}
			// Turn player according to the rotation of the joystick
			float playerRot = transform.rotation.eulerAngles.y > 180f ? (transform.rotation.eulerAngles.y - 360f) : transform.rotation.eulerAngles.y;
			float controllerRot = Mathf.Atan2(HLAxis, VLAxis * -1f) * Mathf.Rad2Deg;
			if (!(Mathf.Abs(playerRot - controllerRot) < 20f))
			{
				RotationSpeed += Time.deltaTime;
			}
			else
			{
				RotationSpeed = CharacterDataStore.CharacterMovementDataStore.MinRotationSpeed;
			}
			// Check if player's speed is not within x degree of the controller angle
			// Then disable the animator if so
			// Turn on the animator of the Leg Swing Preference
			float playerVelRot = Mathf.Atan2(_rb.velocity.x, _rb.velocity.z) * Mathf.Rad2Deg;

			LegSwingReference.GetComponent<Animator>().enabled = true;

			RotationSpeed = Mathf.Clamp(RotationSpeed, CharacterDataStore.CharacterMovementDataStore.MinRotationSpeed, CharacterDataStore.CharacterMovementDataStore.MaxRotationSpeed);
			Transform target = TurnReference.transform.GetChild(0);
			Vector3 relativePos = target.position - transform.position;

			TurnReference.transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Atan2(HLAxis, VLAxis * -1f) * Mathf.Rad2Deg, transform.eulerAngles.z);
			Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
			Quaternion tr = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotationSpeed);
			if (!_auxillaryRotationLock)
				transform.rotation = tr;
		}
		else
		{
			LegSwingReference.GetComponent<Animator>().enabled = false;
			LegSwingReference.transform.eulerAngles = Vector3.zero;
		}
	}

	#region Helper Functions

	// Checks if Vector3 A and Vector3 B are within degree
	// Only smaller than 180 degrees checking
	private bool AngleWithin(Vector3 A, Vector3 B, float degree)
	{
		return Vector3.Angle(A, B) > degree;
	}
	private void CheckRewiredInput()
	{
		if (_player == null) return;
		//LeftTrigger = _player.GetAxis("Left Trigger");
		//RightTrigger = _player.GetAxis("Right Trigger");

		//LeftTrigger = Mathf.Approximately(LeftTrigger, 0f) || Mathf.Approximately(LeftTrigger, -1f) ? 0f : 1f;
		//RightTrigger = Mathf.Approximately(RightTrigger, 0f) || Mathf.Approximately(RightTrigger, -1f) ? 0f : 1f;

		if (!_player.GetButton("Left Trigger") && HandObject != null)
			normalState = State.Holding;
	}

	private bool IsGrounded()
	{
		RaycastHit hit;
		return Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.CharacterMovementDataStore.JumpMask);
	}

	private bool IsFacingCliff()
	{
		RaycastHit hit;
		Physics.Raycast(transform.position + transform.forward * 0.5f, Vector3.down, out hit);
		return hit.collider.gameObject.CompareTag("DeathZone");
	}

	private string GetGroundTag()
	{
		RaycastHit hit;
		Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.CharacterMovementDataStore.JumpMask);
		if (hit.collider == null) return "";
		return hit.collider.tag;
	}

	private void CheckArmHelper(bool down, HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
	{
		// Arm2: right max: 90 --> -74
		//       left min: -75 --> 69        
		JointLimits lm1 = Arm2hj.limits;
		if (IsLeftHand)
			lm1.min = down ? 69f : -75f;
		else
			lm1.max = down ? -74f : 90f;
		Arm2hj.limits = lm1;

		//  Arm: Limits: -90, 90 --> 0, 121
		JointLimits lm = Armhj.limits;
		lm.max = down ? 121f : 90f;
		lm.min = down ? 0f : -90f;
		Armhj.limits = lm;

		// Arm: Target Position: 0 --> 180
		JointSpring js = Armhj.spring;
		js.targetPosition = down ? 180f : 0f;
		Armhj.spring = js;

		// Hand: Limit Max: 90 --> 0
		JointLimits tlm = Handhj.limits;
		tlm.max = down ? 0f : 90f;
		Handhj.limits = tlm;

	}

	void ResetBody()
	{
		ResetBodyHelper(_leftArm2hj, _leftArmhj, _leftHandhj, true);
		ResetBodyHelper(_rightArm2hj, _rightArmhj, _rightHandhj, false);
		JointSpring js = _chesthj.spring;
		js.targetPosition = 0f;
		_chesthj.spring = js;
	}

	// Reset All Body
	void ResetBodyHelper(HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
	{
		JointLimits lm2 = Arm2hj.limits;
		JointLimits lm = Armhj.limits;
		JointSpring js = Armhj.spring;
		JointSpring hjs = Handhj.spring;
		if (IsLeftHand)
		{
			lm2.max = 70f;
			lm2.min = -75f;
		}
		else
		{
			lm2.max = 90f;
			lm2.min = -75f;
		}
		lm.min = -90f;
		lm.max = 90f;
		js.targetPosition = 0f;
		hjs.targetPosition = 0f;
		Handhj.spring = hjs;
		Armhj.spring = js;
		Armhj.limits = lm;
		Arm2hj.limits = lm2;
	}

	private void BlockHelper(HingeJoint Armhj, HingeJoint Handhj)
	{
		JointSpring ajs = Armhj.spring;
		ajs.targetPosition = CharacterDataStore.CharacterBlockDataStore.ArmTargetPosition;
		Armhj.spring = ajs;

		JointSpring hjs = Handhj.spring;
		hjs.targetPosition = CharacterDataStore.CharacterBlockDataStore.HandTargetPosition;
		Handhj.spring = hjs;
	}

	private void MeleeAlternateSchemaHelper()
	{
		if (!IsPunching) return;
		RaycastHit hit;
		// This Layermask get all player's layer except this player's
		int layermask = GameManager.GM.AllPlayers ^ (1 << gameObject.layer);
		if (Physics.SphereCast(transform.position, CharacterDataStore.CharacterMeleeDataStore.PunchRadius, transform.forward, out hit, CharacterDataStore.CharacterMeleeDataStore.PunchDistance, layermask))
		{
			IsPunching = false;
			foreach (var rb in hit.transform.GetComponentInParent<PlayerController>().gameObject.GetComponentsInChildren<Rigidbody>())
			{
				rb.velocity = Vector3.zero;
			}
			Vector3 force = transform.forward * CharacterDataStore.CharacterMeleeDataStore.PunchForce * MeleeCharge;
			hit.transform.GetComponentInParent<PlayerController>().OnMeleeHit(force, MeleeCharge, gameObject);
			hit.transform.GetComponentInParent<PlayerController>().Mark(gameObject);
		}
	}

	IEnumerator MeleeClockFistLeftHandHelper(float time, HingeJoint LeftArmhj)
	{
		float elapsedTime = 0f;
		JointSpring ljs = LeftArmhj.spring;
		float initLATargetPosition = ljs.targetPosition;
		while (elapsedTime < time)
		{
			elapsedTime += Time.deltaTime;
			ljs.targetPosition = Mathf.Lerp(initLATargetPosition, 80f, MeleeCharge);
			LeftArmhj.spring = ljs;
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator MeleeClockFistHelper(HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, float time)
	{
		IsPunching = false;
		float elapesdTime = 0f;
		JointLimits lm2 = Arm2hj.limits;
		JointLimits hl = Handhj.limits;
		JointSpring js = Armhj.spring;

		float initLm2Max = lm2.max;
		float initLm2Min = lm2.min;
		float initLmTargetPosition = js.targetPosition;
		float inithlMax = hl.max;
		float inithlMin = hl.min;

		EventManager.Instance.TriggerEvent(new PunchStart(gameObject, PlayerNumber, RightHand.transform));

		while (elapesdTime < time)
		{
			elapesdTime += Time.deltaTime;
			MeleeCharge = elapesdTime / time;

			lm2.max = Mathf.Lerp(initLm2Max, 4f, MeleeCharge);
			lm2.min = Mathf.Lerp(initLm2Min, -17f, MeleeCharge);

			js.targetPosition = Mathf.Lerp(initLmTargetPosition, -85f, MeleeCharge);

			hl.max = Mathf.Lerp(inithlMax, 130f, MeleeCharge);
			hl.min = Mathf.Lerp(inithlMin, 110f, MeleeCharge);

			Arm2hj.limits = lm2;
			Armhj.spring = js;
			Handhj.limits = hl;
			yield return new WaitForEndOfFrame();
		}
		EventManager.Instance.TriggerEvent(new PunchHolding(gameObject, PlayerNumber, RightHand.transform));

		starttimer_meleeHold = true;
	}
	IEnumerator MeleePunchLeftHandHelper(float time, HingeJoint LeftHandhj)
	{
		float elapsedTime = 0f;
		JointSpring ljs = LeftHandhj.spring;
		float initLATargetPosition = ljs.targetPosition;

		while (elapsedTime < time)
		{
			elapsedTime += Time.deltaTime;
			ljs.targetPosition = Mathf.Lerp(initLATargetPosition, -120f, elapsedTime / time);
			LeftHandhj.spring = ljs;

			yield return new WaitForEndOfFrame();
		}
	}
	IEnumerator MeleePunchHelper(HingeJoint Armhj, HingeJoint Handhj, float time)
	{
		float elapesdTime = 0f;
		IsPunching = true;
		JointLimits hl = Handhj.limits;
		JointSpring js = Armhj.spring;
		float initLmTargetPosition = js.targetPosition;
		float inithlMax = hl.max;
		float inithlMin = hl.min;

		while (elapesdTime < time)
		{
			elapesdTime += Time.deltaTime;
			js.targetPosition = Mathf.Lerp(initLmTargetPosition, 180f, elapesdTime / time);
			hl.max = Mathf.Lerp(inithlMax, 12f, elapesdTime / time);
			hl.min = Mathf.Lerp(inithlMin, -2.8f, elapesdTime / time);
			Armhj.spring = js;
			Handhj.limits = hl;
			if (elapesdTime >= 0f)
			{
				if (DesignPanelManager.DPM.MeleeAlternateSchemaToggle.isOn)
				{
					MeleeAlternateSchemaHelper();
				}
			}

			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(0.1f);
		EventManager.Instance.TriggerEvent(new PunchDone(gameObject, PlayerNumber, RightHand.transform));
		_checkArm = true;
		ResetBody();
		IsPunching = false;
		MeleeCharge = 0f;
	}

	IEnumerator PickUpWeaponHelper(HingeJoint Arm2hj, HingeJoint Armhj, bool IsLeftHand, float time)
	{
		float elapesdTime = 0f;
		JointLimits lm2 = Arm2hj.limits;
		JointLimits lm = Armhj.limits;

		float initLm2LeftMax = lm2.max;
		float initLm2LeftMin = lm2.min;

		float initLmMax = lm.max;
		while (elapesdTime < time)
		{
			elapesdTime += Time.deltaTime;
			if (IsLeftHand)
			{
				lm2.max = Mathf.Lerp(initLm2LeftMax, 103f, elapesdTime / time);
				lm2.min = Mathf.Lerp(initLm2LeftMin, 95f, elapesdTime / time);
			}
			else
			{
				lm2.max = Mathf.Lerp(initLm2LeftMax, -88f, elapesdTime / time);
				lm2.min = Mathf.Lerp(initLm2LeftMin, -98f, elapesdTime / time);
			}
			lm.max = Mathf.Lerp(initLmMax, 180f, elapesdTime / time);
			Arm2hj.limits = lm2;
			Armhj.limits = lm;
			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator PickUpWeaponHalfHelper(HingeJoint Armhj, float time)
	{
		float elapesdTime = 0f;
		JointSpring js = Armhj.spring;
		float initArmTargetPosition = js.targetPosition;

		while (elapesdTime < time)
		{
			elapesdTime += Time.deltaTime;
			js.targetPosition = Mathf.Lerp(initArmTargetPosition, 90f, elapesdTime / time);
			Armhj.spring = js;
			yield return new WaitForEndOfFrame();
		}
	}

	/// <summary>
	/// This function is called from FootSteps on LegSwingRefernece
	/// </summary>
	public void FootStep()
	{
		if (IsGrounded())
		{
			EventManager.Instance.TriggerEvent(new FootStep(OnDeathHidden[2], GetGroundTag()));
		}
	}
	#endregion

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == 13 && _isJumping)
		{
			_isJumping = false;
			EventManager.Instance.TriggerEvent(new PlayerLand(gameObject, GetComponentInChildren<UIController>().UI.gameObject, PlayerNumber, GetGroundTag()));
			OnDeathHidden[3].SetActive(true);
		}
	}

	private void OnEnable()
	{
		EventManager.Instance.AddHandler<PlayerDied>(OnEnterDeathZone);
	}

	private void OnDisable()
	{
		EventManager.Instance.RemoveHandler<PlayerDied>(OnEnterDeathZone);

	}

	public void SetControl(bool canControl)
	{
		_canControl = canControl;
		if (!canControl)
			LegSwingReference.GetComponent<Animator>().enabled = _canControl;
	}

}
