using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Status
{
    public virtual float Duration { get; set; }
    public virtual float Potency { get; set; }
    public virtual bool Perma { get; set; }
    public Status(float duration, float potency, bool perma = false)
    {
        Duration = duration;
        Potency = potency;
        Perma = perma;
    }

    public override bool Equals(object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            Status p = (Status)obj;
            return Mathf.Abs(Potency - p.Potency) <= 0.001f && Perma == p.Perma;
        }
    }
}

public class SlowEffect : Status
{
    public SlowEffect(float duration, float potency) : base(duration, potency)
    {
    }

    public SlowEffect(float duration, float potency, bool perma = false) : base(duration, potency, perma)
    {
    }
}

public class RotationSlowEffect : Status
{
    public RotationSlowEffect(float duration, float potency, bool perma = false) : base(duration, potency, perma)
    {
    }
}

public class PermaSlowEffect : Status
{
    public PermaSlowEffect(float duration, float potency) : base(duration, potency)
    {
    }
}

public class RemovePermaSlowEffect : Status
{
    public RemovePermaSlowEffect(float duration, float potency) : base(duration, potency)
    {
    }
}

public class StunEffect : Status
{
    public StunEffect(float duration, float potency) : base(duration, potency)
    {
    }
}