using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Status
{
	public virtual float Duration { get; set; }
	public virtual float Potency { get; set; }

	public Status(float duration, float potency)
	{
		Duration = duration;
		Potency = potency;
	}
}

public class SlowEffect : Status
{
	public SlowEffect(float duration, float potency) : base(duration, potency)
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