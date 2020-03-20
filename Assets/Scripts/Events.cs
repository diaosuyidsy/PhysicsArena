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
    public Vector3 WinnedPosition;
    public GameWinType GameWinType;

    public GameEnd(int winner, Transform winnedObjective, GameWinType gameWinType)
    {
        Winner = winner;
        WinnedObjective = winnedObjective;
        GameWinType = gameWinType;
    }

    public GameEnd(int winner, Vector3 winnedPos, GameWinType gameWinType)
    {
        Winner = winner;
        WinnedPosition = winnedPos;
        GameWinType = gameWinType;
    }
}
#region PLAYER EVENT
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
        if (isABlock)
            EventManager.Instance.TriggerEvent(new Blocked(hiter, hiterPlayerNumber));
    }
}

public class PlayerDied : GameEvent
{
    public GameObject Player { get; }
    public int PlayerNumber { get; }
    public GameObject PlayerHitter { get; }
    public bool HitterIsValid { get; }
    public ImpactType ImpactType { get; }
    public GameObject ImpactObject { get; }
    public PlayerDied(GameObject player, int playerNumber, ImpactMarker impactMarker, GameObject impactObject)
    {
        Player = player;
        PlayerNumber = playerNumber;
        PlayerHitter = impactMarker.EnemyWhoHitPlayer;
        HitterIsValid = Time.time < impactMarker.PlayerMarkedTime + 3f;
        ImpactType = impactMarker.ImpactType;
        ImpactObject = impactObject;
    }
}

public class BagelSent : GameEvent
{
    public GameObject Basket;

    public BagelSent(GameObject obj)
    {
        Basket = obj;
    }
}

public class WeaponGeneratorSwtich : GameEvent
{
    public GameObject Generator;
    public bool Activated;

    public WeaponGeneratorSwtich(GameObject g, bool activated)
    {
        Generator = g;
        Activated = activated;
    }
}


public class PlayerStunned : GameEvent
{
    public PlayerStunned(GameObject player, int playerNumber, Transform playerHead, float stunnedTime)
    {
        Player = player;
        PlayerNumber = playerNumber;
        PlayerHead = playerHead;
        StunnedTime = stunnedTime;
    }

    public GameObject Player { get; }
    public int PlayerNumber { get; }
    public Transform PlayerHead { get; }
    public float StunnedTime { get; }

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

public class PunchInterruptted : GameEvent
{
    public GameObject Player;
    public int PlayerNumber;

    public PunchInterruptted(GameObject player, int playerNumber)
    {
        Player = player;
        PlayerNumber = playerNumber;
    }
}

public class FootStep : GameEvent
{
    public FootStep(GameObject playerFeet, GameObject playerActualFoot, string groundTag, GameObject player, int playerFootLeftRight)
    {
        PlayerFeet = playerFeet;
        PlayerActualFoot = playerActualFoot;
        GroundTag = groundTag;
        Player = player;
        PlayerFootLeftRight = playerFootLeftRight;
    }

    public GameObject PlayerFeet { get; }
    public GameObject PlayerActualFoot { get; }
    public string GroundTag { get; }
    public GameObject Player { get; }
    public int PlayerFootLeftRight { get; }
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
#endregion

public class WeaponHitDeathTrigger : GameEvent
{
}

public class WeaponUsedUp : GameEvent
{
}

public class ObjectDespawned : GameEvent
{
    public GameObject Obj { get; }
    public GameObject Despawner { get; }
    public ObjectDespawned(GameObject _obj)
    {
        Obj = _obj;
    }

    public ObjectDespawned(GameObject obj, GameObject despawner) : this(obj)
    {
        Despawner = despawner;
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

public class BagelDespawn : GameEvent
{

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

public class ObjectHitGround : GameEvent
{
    public GameObject Obj;

    public ObjectHitGround(GameObject obj)
    {
        Obj = obj;
    }
}

#region WEAPON EVENT
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

public abstract class HookGunEvent : GameEvent
{
    protected HookGunEvent(GameObject hookGun, GameObject hookGunOwner, int hookGunOwnerPlayerNumber)
    {
        HookGun = hookGun;
        HookGunOwner = hookGunOwner;
        HookGunOwnerPlayerNumber = hookGunOwnerPlayerNumber;
    }

    public GameObject HookGun { get; }
    public GameObject HookGunOwner { get; }
    public int HookGunOwnerPlayerNumber { get; }

}


public class HookGunFired : HookGunEvent
{
    public HookGunFired(GameObject hookGun, GameObject hookGunOwner, int hookGunOwnerPlayerNumber) : base(hookGun, hookGunOwner, hookGunOwnerPlayerNumber)
    {
    }
}

public class HookHit : HookGunEvent
{
    public HookHit(GameObject hookGun, GameObject hookGunOwner, int hookGunOwnerPlayerNumber, GameObject hook, GameObject hookedPlayer, int hookedPlayerNUmber) : base(hookGun, hookGunOwner, hookGunOwnerPlayerNumber)
    {
        Hook = hook;
        Hooked = hookedPlayer;
        HookedPlayerNumber = hookedPlayerNUmber;
    }

    public GameObject Hook { get; }
    public GameObject Hooked { get; }
    public int HookedPlayerNumber { get; }
}

public class HookBlocked : HookGunEvent
{
    public GameObject HookBlocker { get; }
    public int HookBlockerPlayerNumber { get; }
    public GameObject Hook { get; }

    public HookBlocked(GameObject hookGun, GameObject hookGunOwner, int hookGunOwnerPlayerNumber, GameObject hookblocker, int hookblockerplayernumber, GameObject hook) : base(hookGun, hookGunOwner, hookGunOwnerPlayerNumber)
    {
        HookBlocker = hookblocker;
        HookBlockerPlayerNumber = hookblockerplayernumber;
        Hook = hook;
        EventManager.Instance.TriggerEvent(new Blocked(HookBlocker, HookBlockerPlayerNumber));
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

public abstract class FistGunEvent : GameEvent
{
    public GameObject FistGun { get; }
    public GameObject FistGunOwner { get; }
    public int FistGunOwnerPlayerNumber { get; }

    protected FistGunEvent(GameObject fistGun, GameObject fistGunOwner, int fistGunOwnerPlayerNumber)
    {
        FistGun = fistGun;
        FistGunOwner = fistGunOwner;
        FistGunOwnerPlayerNumber = fistGunOwnerPlayerNumber;
    }
}

public class FistGunFired : FistGunEvent
{
    public GameObject Fist { get; }
    public FistGunFired(GameObject _fistGun, GameObject _fistGunOwner, int _fistgunownerplayernumber, GameObject fist) : base(_fistGun, _fistGunOwner, _fistgunownerplayernumber)
    {
        Fist = fist;
    }
}

public class FistGunHit : FistGunEvent
{
    public GameObject Fist { get; }
    public GameObject Hitted { get; }
    public int HittedPlayerNumber { get; }

    public FistGunHit(GameObject _FistGun, GameObject _Fist, GameObject _FistGunUser, GameObject _Hitted, int _UserPlayerNumber, int _HittedPlayerNumber) : base(_FistGun, _FistGunUser, _UserPlayerNumber)
    {
        Fist = _Fist;
        Hitted = _Hitted;
        HittedPlayerNumber = _HittedPlayerNumber;
    }
}

public class FistGunBlocked : FistGunEvent
{
    public GameObject Fist { get; }
    public GameObject Blocker { get; }
    public int BlockerPlayerNumber { get; }
    public FistGunBlocked(GameObject fistGun, GameObject fistGunOwner, int fistGunOwnerPlayerNumber, GameObject fist, GameObject blocker, int blockerPlayerNumber) : base(fistGun, fistGunOwner, fistGunOwnerPlayerNumber)
    {
        Fist = fist;
        Blocker = blocker;
        BlockerPlayerNumber = blockerPlayerNumber;
        EventManager.Instance.TriggerEvent(new Blocked(blocker, blockerPlayerNumber));
    }
}

public class FistGunStartCharging : FistGunEvent
{
    public FistGunStartCharging(GameObject fistGun, GameObject fistGunOwner, int fistGunOwnerPlayerNumber) : base(fistGun, fistGunOwner, fistGunOwnerPlayerNumber)
    {
    }
}

public class FistGunCharged : FistGunEvent
{
    public FistGunCharged(GameObject fistGun, GameObject fistGunOwner, Vector3 fistPos) : base(fistGun, fistGunOwner, fistGunOwner == null ? -1 : fistGunOwner.GetComponent<PlayerController>().PlayerNumber)
    {
        FistPos = fistPos;
    }

    public Vector3 FistPos { get; }

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
    public List<int> HitPlayersNumber;

    public BazookaBombed(GameObject bazookaGun, GameObject bazookaUser, int playerNumber, List<GameObject> hitPlayers)
    {
        BazookaGun = bazookaGun;
        BazookaUser = bazookaUser;
        PlayerNumber = playerNumber;
        HitPlayers = hitPlayers;
        HitPlayersNumber = new List<int>();
        for (int i = 0; i < HitPlayersNumber.Count; i++)
        {
            HitPlayersNumber.Add(hitPlayers[i].GetComponent<PlayerController>().PlayerNumber);
        }
    }
}

#endregion

public class HitStopEvent : GameEvent
{
    public int StopFrames;
    public float TimeScale;

    public HitStopEvent(int stopFrames, float timeScale)
    {
        StopFrames = stopFrames;
        TimeScale = timeScale;
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

public class Blocked : GameEvent
{
    public GameObject Blocker;
    public int BlockerPlayerNumber;

    public Blocked(GameObject blocker, int blockerPlayerNumber)
    {
        Blocker = blocker;
        BlockerPlayerNumber = blockerPlayerNumber;
    }
}

#region MapEvent

public class OnScore : GameEvent
{
    public int Team;

    public OnScore(int team)
    {
        Team = team;
    }
}
#endregion

public class OnAddCameraTargets : GameEvent
{
    public GameObject Target;
    public int Weight;

    public OnAddCameraTargets(GameObject target, int weight)
    {
        Target = target;
        Weight = weight;
    }
}

public class OnRemoveCameraTargets : GameEvent
{
    public GameObject Target;

    public OnRemoveCameraTargets(GameObject target)
    {
        Target = target;
    }
}