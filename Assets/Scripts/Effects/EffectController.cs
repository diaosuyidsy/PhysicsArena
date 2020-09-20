using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collect and apply effects
/// </summary>
[RequireComponent(typeof(IEffectable))]
public class EffectController : MonoBehaviour
{
    public List<Status> EffectList = new List<Status>();
    private IEffectable _effectable;

    private void Awake()
    {
        _effectable = GetComponent<IEffectable>();
    }

    public void OnApplyEffect(Status effect)
    {
        EffectList.Add(effect);
        UpdateAllEffects();
    }

    private void Update()
    {
        for (int i = EffectList.Count - 1; i > -1; i--)
        {
            if (EffectList[i].Perma) continue;
            EffectList[i].Duration -= Time.deltaTime;
            if (EffectList[i].Duration <= 0f)
            {
                EffectList.RemoveAt(i);
                UpdateAllEffects();
            }
        }
    }

    public void OnRemoveEffect(Status effect)
    {
        for (int i = EffectList.Count - 1; i > -1; i--)
        {
            if (EffectList[i].Equals(effect))
            {
                EffectList.RemoveAt(i);
                break;
            }
        }
        UpdateAllEffects();
    }

    /// <summary>
    /// Iterate all effects and decide what to apply to player
    /// </summary>
    private void UpdateAllEffects()
    {
        float maxSlowPotency = 0f;
        float maxRotationSlowPotency = 0f;
        // Deal with slow effect
        // Compare and get the lowest Potency and apply that to the player
        for (int i = 0; i < EffectList.Count - 1; i++)
        {
            Status effect = EffectList[i];
            if (effect.GetType().Equals(typeof(SlowEffect)))
            {
                if (effect.Potency > maxSlowPotency)
                {
                    maxSlowPotency = effect.Potency;
                }
            }
            if (effect.GetType().Equals(typeof(RotationSlowEffect)))
            {
                if (effect.Potency > maxRotationSlowPotency)
                {
                    maxRotationSlowPotency = effect.Potency;
                }
            }
        }
        // Apply Slow Effect
        _effectable.WalkSpeedMultiplier = 1f - maxSlowPotency;
        _effectable.RotationSpeedMultiplier = 1f - maxRotationSlowPotency;
    }
}
