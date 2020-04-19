using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class GameActions
{
    public virtual void Execute()
    {

    }
}

public class HitAction : GameActions
{
    public GameObject HitTarget;
    public GameObject Hiter;
    public Vector3 HitForce;

    public HitAction(GameObject hitTarget, GameObject hiter, Vector3 hitForce)
    {
        HitTarget = hitTarget;
        Hiter = hiter;
        HitForce = hitForce;
    }
}

public class MeleeHitAction : HitAction
{
    public bool IsABlock;

    public MeleeHitAction(GameObject hitTarget, GameObject hiter, Vector3 hitForce, bool isABlock) : base(hitTarget, hiter, hitForce)
    {
        IsABlock = isABlock;
    }

    public override void Execute()
    {
        base.Execute();
        EventManager.Instance.TriggerEvent(new PlayerHit(Hiter, HitTarget, HitForce, Hiter.GetComponent<PlayerController>().PlayerNumber, HitTarget.GetComponent<PlayerController>().PlayerNumber, 1f, IsABlock));
        if (!IsABlock)
            Hiter.GetComponent<PlayerController>().SetVelocity(Vector3.zero);
        HitTarget.GetComponent<IHittable>().OnImpact(HitForce, ForceMode.Impulse, Hiter, IsABlock ? ImpactType.Block : ImpactType.Melee);
    }
}