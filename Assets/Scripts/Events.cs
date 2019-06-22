using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStart : GameEvent
{
	public GameStart()
	{
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

	public PlayerHit(GameObject _hiter, GameObject _hitted,
		Vector3 _force)
	{
		Hiter = _hiter;
		Hitted = _hitted;
		Force = _force;
		HiterPlayerNumber = 0;
		HittedPlayerNumber = 0;
	}

	public PlayerHit(GameObject _hiter, GameObject _hitted,
		Vector3 _force, int _hiternum, int _hittednum)
	{
		Hiter = _hiter;
		Hitted = _hitted;
		Force = _force;
		HiterPlayerNumber = _hiternum;
		HittedPlayerNumber = _hittednum;
	}
}

public class PlayerDied : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }

	public PlayerDied(GameObject _player, int _playernum)
	{
		Player = _player;
		PlayerNumber = _playernum;
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

public class PunchHolding : GameEvent
{
	public GameObject Player { get; }
	public int PlayerNumber { get; }

	public PunchHolding(GameObject _player, int _playernumber)
	{
		Player = _player;
		PlayerNumber = _playernumber;
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

public class FootStep : GameEvent
{
	public GameObject PlayerFeet { get; }
	public string GroundType { get; }

	public FootStep(GameObject _playerfeet)
	{
		PlayerFeet = _playerfeet;
		GroundType = "Grass";
	}

	public FootStep(GameObject _playerfeet, string _groundType)
	{
		PlayerFeet = _playerfeet;
		GroundType = _groundType;
	}
}

public class PlayerJump : GameEvent
{
	public GameObject Player;
	public GameObject PlayerFeet;
	public int PlayerNumber;

	public PlayerJump(GameObject _player, GameObject _playerfeet, int _playernumber)
	{
		Player = _player;
		PlayerFeet = _playerfeet;
		PlayerNumber = _playernumber;
	}
}

public class PlayerLand : GameEvent
{
	public GameObject Player;
	public GameObject PlayerFeet;
	public int PlayerNumber;

	public PlayerLand(GameObject _player, GameObject _playerfeet, int _playernumber)
	{
		Player = _player;
		PlayerFeet = _playerfeet;
		PlayerNumber = _playernumber;
	}
}

public class FoodDelivered : GameEvent
{
	public GameObject Food { get; }
	public string FoodTag { get; }

	public FoodDelivered(GameObject _food, string _foodtag)
	{
		Food = _food;
		FoodTag = _foodtag;
	}
}