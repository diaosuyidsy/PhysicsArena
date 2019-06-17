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

	public PlayerHit(UnityEngine.GameObject _hiter, UnityEngine.GameObject _hitted,
		UnityEngine.Vector3 _force)
	{
		Hiter = _hiter;
		Hitted = _hitted;
		Force = _force;
	}
}