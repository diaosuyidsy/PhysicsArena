public class GameStart : GameEvent
{
	public GameStart()
	{
	}
}

public class PlayerHit : GameEvent
{
	public UnityEngine.GameObject Hiter { get; }
	public UnityEngine.GameObject Hitted { get; }
	public UnityEngine.Vector3 Force { get; }
	/// <summary>
	/// Represents the Rewired Player Number
	/// </summary>
	public int HiterPlayerNumber { get; }
	public int HittedPlayerNumber { get; }

	public PlayerHit(UnityEngine.GameObject _hiter, UnityEngine.GameObject _hitted,
		UnityEngine.Vector3 _force)
	{
		Hiter = _hiter;
		Hitted = _hitted;
		Force = _force;
		HiterPlayerNumber = 0;
		HittedPlayerNumber = 0;
	}

	public PlayerHit(UnityEngine.GameObject _hiter, UnityEngine.GameObject _hitted,
		UnityEngine.Vector3 _force, int _hiternum, int _hittednum)
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
	public UnityEngine.GameObject Player { get; }
	public int PlayerNumber { get; }

	public PlayerDied(UnityEngine.GameObject _player, int _playernum)
	{
		Player = _player;
		PlayerNumber = _playernum;
	}


}

public class PlayerRespawned : GameEvent
{
	public UnityEngine.GameObject Player { get; }
	public PlayerRespawned(UnityEngine.GameObject _player)
	{
		Player = _player;
	}
}

public class ObjectDespawned : GameEvent
{
	public UnityEngine.GameObject Obj { get; }
	public ObjectDespawned(UnityEngine.GameObject _obj)
	{
		Obj = _obj;
	}
}

public class WeaponSpawned : GameEvent
{
	public UnityEngine.GameObject Weapon { get; }
	public WeaponSpawned(UnityEngine.GameObject _weapon)
	{
		Weapon = _weapon;
	}
}