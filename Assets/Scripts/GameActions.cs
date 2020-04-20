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
    public ForceMode ForceMode;
    public ImpactType ImpactType;

    public HitAction(GameObject hitTarget, GameObject hiter, Vector3 hitForce, ForceMode forceMode, ImpactType impactType)
    {
        HitTarget = hitTarget;
        Hiter = hiter;
        HitForce = hitForce;
        ForceMode = forceMode;
        ImpactType = impactType;
    }

    public override void Execute()
    {
        base.Execute();
        HitTarget.GetComponent<IHittable>().OnImpact(HitForce, ForceMode, Hiter, ImpactType);
    }
}

public class MeleeHitAction : HitAction
{
    public MeleeHitAction(GameObject hitTarget, GameObject hiter, Vector3 hitForce, ForceMode forceMode, ImpactType impactType) : base(hitTarget, hiter, hitForce, forceMode, impactType)
    {
    }

    public override void Execute()
    {
        bool IsABlock = ImpactType == ImpactType.Block;
        EventManager.Instance.TriggerEvent(new PlayerHit(Hiter, HitTarget, HitForce, Hiter.GetComponent<PlayerController>().PlayerNumber, HitTarget.GetComponent<PlayerController>().PlayerNumber, 1f, IsABlock));
        if (!IsABlock)
            Hiter.GetComponent<PlayerController>().SetVelocity(Vector3.zero);
        base.Execute();
    }
}