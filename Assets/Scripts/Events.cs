using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStart : GameEvent
{
	public GameStart()
	{
	}
}

public class GameEnd : GameEvent
{
	public int Winner;
	public Transform WinnedObjective;
	public GameWinType GameWinType;

	public GameEnd(int winner, Transform winnedObjective, GameWinType gameWinType)
	{
		Winner = winner;
		WinnedObjective = winnedObjective;
		GameWinType = gameWinType;
	}
}

public class PlayerHit : GameEvent
{
	public GameObject Hiter { get; }
	public GameObject Hitted { get; }
	public Vector3 Force { get; }
	/// <summary>
	/// Represents the Rewired Player Number
	/// </summary>
	public int HiterPlayerNumber { get; }
	public int HittedPlayerNumber { get; }
	public float MeleeCharge { get; }
	public bool IsABlock;

	public PlayerHit(GameObject hiter, GameObject hitted, Vector3 force, int hiterPlayerNumber, int hittedPlayerNumber, float meleeCharge, bool isABlock)
	{
		Hiter = hiter;
		Hitted = hitted;
		Force = force;
		HiterPlayerNumber = hiterPlayerNumber;
		HittedPlayerNumber = hittedPlayerNumber;
		MeleeCharge = meleeCharge;
		IsABlock = isABlock;
	}
}

public class PlayerDied : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public GameObject PlayerHitter { get; }
	public bool HitterIsValid { get; }

	public PlayerDied(GameObject player, int playerNumber, GameObject playerHitter, bool hitterIsValid)
	{
		Player = player;
		PlayerNumber = playerNumber;
		PlayerHitter = playerHitter;
		HitterIsValid = hitterIsValid;
	}
}

public class PlayerStunned : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public Transform PlayerHead { get; }

	public PlayerStunned(GameObject player, int playerNumber, Transform playerHead)
	{
		Player = player;
		PlayerNumber = playerNumber;
		PlayerHead = playerHead;
	}
}

public class PlayerUnStunned : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public PlayerUnStunned(GameObject player, int playerNumber)
	{
		Player = player;
		PlayerNumber = playerNumber;
	}
}

public class PlayerRespawned : GameEvent
{
	public GameObject Player { get; }
	public PlayerRespawned(GameObject _player)
	{
		Player = _player;
	}
}

public class PlayerSlowed : GameEvent
{
	public PlayerSlowed(GameObject player, int playerNumber, GameObject playerFeet)
	{
		Player = player;
		PlayerNumber = playerNumber;
		PlayerFeet = playerFeet;
	}

	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public GameObject PlayerFeet { get; }
}

public class PlayerUnslowed : GameEvent
{
	public PlayerUnslowed(GameObject player, int playerNumber, GameObject playerFeet)
	{
		Player = player;
		PlayerNumber = playerNumber;
		PlayerFeet = playerFeet;
	}

	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public GameObject PlayerFeet { get; }
}


public class ObjectDespawned : GameEvent
{
	public GameObject Obj { get; }
	public ObjectDespawned(GameObject _obj)
	{
		Obj = _obj;
	}
}

public class WeaponSpawned : GameEvent
{
	public GameObject Weapon { get; }
	public WeaponSpawned(GameObject _weapon)
	{
		Weapon = _weapon;
	}
}

public class ObjectPickedUp : GameEvent
{
	public GameObject Player;
	public int PlayerNumber;
	public GameObject Obj;

	public ObjectPickedUp(GameObject player, int playerNumber, GameObject _object)
	{
		Player = player;
		PlayerNumber = playerNumber;
		Obj = _object;
	}
}

public class ObjectDropped : GameEvent
{
	public GameObject Player;
	public int PlayerNumber;
	public GameObject Obj;

	public ObjectDropped(GameObject player, int playerNumber, GameObject obj)
	{
		Player = player;
		PlayerNumber = playerNumber;
		Obj = obj;
	}
}

public class WaterGunFired : GameEvent
{
	public GameObject WaterGun { get; }
	public GameObject WaterGunOwner { get; }
	public int WaterGunOwnerPlayerNumber { get; }

	public WaterGunFired(GameObject _watergun, GameObject _watergunowner, int _watergunownerplayernumber)
	{
		WaterGun = _watergun;
		WaterGunOwner = _watergunowner;
		WaterGunOwnerPlayerNumber = _watergunownerplayernumber;
	}
}

public class HookGunFired : GameEvent
{
	public GameObject HookGun { get; }
	public GameObject HookGunOwner { get; }
	public int HookGunOwnerPlayerNumber { get; }

	public HookGunFired(GameObject _hookgun, GameObject _hookgunowner, int _hookgunownerplayernumber)
	{
		HookGun = _hookgun;
		HookGunOwner = _hookgunowner;
		HookGunOwnerPlayerNumber = _hookgunownerplayernumber;
	}
}

public class HookHit : GameEvent
{
	public GameObject HookGun { get; }
	public GameObject Hook { get; }
	public GameObject Hooker { get; }
	public GameObject Hooked { get; }
	public int HookerPlayerNumber { get; }
	public int HookedPlayerNumber { get; }

	public HookHit(GameObject _hookgun, GameObject _hook, GameObject _hooker, GameObject _hooked, int _hookerplayernumber, int _hookedplayernumber)
	{
		HookGun = _hookgun;
		Hook = _hook;
		Hooker = _hooker;
		Hooked = _hooked;
		HookerPlayerNumber = _hookerplayernumber;
		HookedPlayerNumber = _hookedplayernumber;
	}
}

public class SuckGunFired : GameEvent
{
	public GameObject SuckGun { get; }
	public GameObject SuckGunOwner { get; }
	public int SuckGunOwnerPlayerNumber { get; }

	public SuckGunFired(GameObject _suckgun, GameObject _suckgunowner, int _suckgunownerplayernumber)
	{
		SuckGun = _suckgun;
		SuckGunOwner = _suckgunowner;
		SuckGunOwnerPlayerNumber = _suckgunownerplayernumber;
	}
}

public class SuckGunSuck : GameEvent
{
	public GameObject SuckGun { get; }
	public GameObject SuckBall { get; }
	public GameObject SuckGunOwner { get; }
	public int SuckGunOwnerPlayerNumber { get; }

	public List<GameObject> SuckedPlayers { get; private set; }
	public List<int> SuckedPlayersNumber { get; private set; }

	public SuckGunSuck(GameObject _suckgun, GameObject _suckball, GameObject _suckgunowner, int _suckgunownerplayernumber, List<GameObject> _suckedplayers)
	{
		SuckGun = _suckgun;
		SuckBall = _suckball;
		SuckGunOwner = _suckgunowner;
		SuckGunOwnerPlayerNumber = _suckgunownerplayernumber;
		_init(_suckedplayers);
	}

	private void _init(List<GameObject> _suckedplayers)
	{
		SuckedPlayers = new List<GameObject>();
		SuckedPlayersNumber = new List<int>();
		foreach (GameObject player in _suckedplayers)
		{
			SuckedPlayers.Add(player);
			SuckedPlayersNumber.Add(player.GetComponent<PlayerController>().PlayerNumber);
		}
	}
}

public class FistGunFired : GameEvent
{
	public GameObject FistGun { get; }
	public GameObject FistGunOwner { get; }
	public int FistGunOwnerPlayerNumber { get; }

	public FistGunFired(GameObject _fistGun, GameObject _fistGunOwner, int _fistgunownerplayernumber)
	{
		FistGun = _fistGun;
		FistGunOwner = _fistGunOwner;
		FistGunOwnerPlayerNumber = _fistgunownerplayernumber;
	}
}

public class FistGunHit : GameEvent
{
	public GameObject FistGun { get; }
	public GameObject Fist { get; }
	public GameObject FistGunUser { get; }
	public GameObject Hitted { get; }
	public int UserPlayerNumber { get; }
	public int HittedPlayerNumber { get; }

	public FistGunHit(GameObject _FistGun, GameObject _Fist, GameObject _FistGunUser, GameObject _Hitted, int _UserPlayerNumber, int _HittedPlayerNumber)
	{
		FistGun = _FistGun;
		Fist = _Fist;
		FistGunUser = _FistGunUser;
		Hitted = _Hitted;
		UserPlayerNumber = _UserPlayerNumber;
		HittedPlayerNumber = _HittedPlayerNumber;
	}
}

public class BazookaFired : GameEvent
{
	public GameObject BazookaGun;
	public GameObject BazookaUser;
	public int PlayerNumber;

	public BazookaFired(GameObject bazookaGun, GameObject bazookaUser, int playerNumber)
	{
		BazookaGun = bazookaGun;
		BazookaUser = bazookaUser;
		PlayerNumber = playerNumber;
	}
}

public class BazookaBombed : GameEvent
{
	public GameObject BazookaGun;
	public GameObject BazookaUser;
	public int PlayerNumber;
	public List<GameObject> HitPlayers;

	public BazookaBombed(GameObject bazookaGun, GameObject bazookaUser, int playerNumber, List<GameObject> hitPlayers)
	{
		BazookaGun = bazookaGun;
		BazookaUser = bazookaUser;
		PlayerNumber = playerNumber;
		HitPlayers = hitPlayers;
	}
}

public class BlockStart : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public BlockStart(GameObject player, int playerNumber)
	{
		Player = player;
		PlayerNumber = playerNumber;
	}
}

public class BlockEnd : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public BlockEnd(GameObject player, int playerNumber)
	{
		Player = player;
		PlayerNumber = playerNumber;
	}
}

public class PunchStart : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public Transform PlayerRightHand { get; }
	public PunchStart(GameObject player, int playerNumber, Transform playerRightHand)
	{
		Player = player;
		PlayerNumber = playerNumber;
		PlayerRightHand = playerRightHand;
	}
}

public class PunchHolding : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }
	public Transform PlayerRightHand;

	public PunchHolding(GameObject player, int playerNumber, Transform playerRightHand)
	{
		Player = player;
		PlayerNumber = playerNumber;
		PlayerRightHand = playerRightHand;
	}
}

public class PunchReleased : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }

	public PunchReleased(GameObject _player, int _playernumber)
	{
		Player = _player;
		PlayerNumber = _playernumber;
	}
}

public class PunchDone : GameEvent
{
	public GameObject Player;
	public int PlayerNumber;
	public Transform PlayerRightHand;

	public PunchDone(GameObject player, int playerNumber, Transform playerRightHand)
	{
		Player = player;
		PlayerNumber = playerNumber;
		PlayerRightHand = playerRightHand;
	}
}

public class FootStep : GameEvent
{
	public GameObject PlayerFeet { get; }
	public string GroundTag { get; }

	public FootStep(GameObject _playerfeet, string _groundTag)
	{
		PlayerFeet = _playerfeet;
		GroundTag = _groundTag;
	}
}

public class PlayerJump : GameEvent
{
	public GameObject Player;
	public GameObject PlayerFeet;
	public int PlayerNumber;
	public string GroundTag;

	public PlayerJump(GameObject _player, GameObject _playerfeet, int _playernumber, string _groundtag)
	{
		Player = _player;
		PlayerFeet = _playerfeet;
		PlayerNumber = _playernumber;
		GroundTag = _groundtag;
	}
}

public class PlayerLand : GameEvent
{
	public GameObject Player;
	public GameObject PlayerFeet;
	public int PlayerNumber;
	public string GroundTag;

	public PlayerLand(GameObject _player, GameObject _playerfeet, int _playernumber, string _groundtag)
	{
		Player = _player;
		PlayerFeet = _playerfeet;
		PlayerNumber = _playernumber;
		GroundTag = _groundtag;
	}
}

public class FoodDelivered : GameEvent
{
	public GameObject Food { get; }
	public string FoodTag { get; }
	public int DeliverPlayerNumber { get; }

	public FoodDelivered(GameObject food, string foodTag, int deliverPlayerNumber)
	{
		Food = food;
		FoodTag = foodTag;
		DeliverPlayerNumber = deliverPlayerNumber;
	}
}