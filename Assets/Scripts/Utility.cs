using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CharTween;
using DG.Tweening;

public static class Utility
{
    // This function takes the center position on sreen
    // and calculates the maximum length of its position to the four corners
    public static float GetMaxLengthToCorner(Vector2 centerposition)
    {
        Vector2[] screenVertices = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(Screen.width, 0f),
            new Vector2(Screen.width, Screen.height),
            new Vector2(0f, Screen.height),
        };

        float maxLength = 0f;
        for (int i = 0; i < 4; i++)
        {
            float length = Vector2.Distance(centerposition, screenVertices[i]);
            if (length > maxLength)
            {
                maxLength = length;
            }
        }
        return maxLength;
    }

    // Sequence example, bubbly fade-in + bounce
    public static Sequence BubbleFadeIn(CharTweener _tweener, int start, int end, float amplitude = 100f, float duration = 0.5f)
    {
        for (int i = start; i < end; i++)
        {
            _tweener.DOScale(i, 0f, 0f);
        }
        var sequence = DOTween.Sequence();
        for (var i = start; i <= end; ++i)
        {
            var timeOffset = Mathf.Lerp(0, 1, (i - start) / (float)(end - start + 1));
            var charSequence = DOTween.Sequence();
            charSequence.Append(_tweener.DOLocalMoveY(i, amplitude, duration).SetEase(Ease.InOutCubic))
                .Join(_tweener.DOFade(i, 0, duration).From())
                .Join(_tweener.DOScale(i, 0, duration).From().SetEase(Ease.OutBack, 5))
                .Append(_tweener.DOLocalMoveY(i, 0, duration).SetEase(Ease.OutBounce));
            sequence.Insert(timeOffset, charSequence);
        }

        return sequence;
    }

    // Sequence example, bubbly fade-out + bounce
    public static Sequence BubbleFadeOut(CharTweener _tweener, int start, int end, float amplitude = 100f, float duration = 0.5f)
    {
        var sequence = DOTween.Sequence();

        for (var i = start; i <= end; ++i)
        {
            var timeOffset = Mathf.Lerp(0, 1, (i - start) / (float)(end - start + 1));
            var charSequence = DOTween.Sequence();
            charSequence.Append(_tweener.DOLocalMoveY(i, amplitude, duration).SetEase(Ease.InBounce))
                .Join(_tweener.DOScale(i, 1, duration).From().SetEase(Ease.InBack, 2))
                .Append(_tweener.DOFade(i, 1, duration).From())
            .Append(_tweener.DOLocalMoveY(i, 0, duration).SetEase(Ease.InOutCubic));
            sequence.Insert(timeOffset, charSequence);
        }

        return sequence;
    }

    public static void SelectionSortStatisticRecord(ref StatsTuple[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int min_idx = i;
            for (int j = i + 1; j < n; j++)
                if (arr[j] != null && arr[min_idx] != null && arr[j].WeightData < arr[min_idx].WeightData)
                    min_idx = j;
            var temp = arr[min_idx];
            arr[min_idx] = arr[i];
            arr[i] = temp;
        }
    }
}

public class PunchMessage
{
    public Vector3 Force;
    public float MeleeCharge;
    public GameObject Sender;
    public bool Blockable;

    public PunchMessage()
    {

    }
    public PunchMessage(Vector3 force, float meleeCharge, GameObject sender, bool blockable)
    {
        Force = force;
        MeleeCharge = meleeCharge;
        Sender = sender;
        Blockable = blockable;
    }
}

public class StatsTuple
{
    public int Index;
    public int RewiredID;
    public float RawData;
    public float WeightData;

    public StatsTuple(int index, int rewiredID, float rawData, float weightData)
    {
        Index = index;
        RewiredID = rewiredID;
        RawData = rawData;
        WeightData = weightData;
    }
}

public class StatisticsRecord
{
    public StatisticsRecord(string statisticName, string statisticsInformation, Sprite statisticIcon)
    {
        StatisticName = statisticName;
        StatisticsInformation = statisticsInformation;
        StatisticIcon = statisticIcon;
    }

    public string StatisticName { get; }
    public string StatisticsInformation { get; }
    public Sprite StatisticIcon { get; }

}

public class ImpactMarker
{
    public GameObject EnemyWhoHitPlayer { get; private set; }
    public float PlayerMarkedTime { get; private set; }
    public ImpactType ImpactType { get; private set; }

    public ImpactMarker(GameObject enemyWhoHitPlayer, float playerMarkedTime, ImpactType impactType)
    {
        SetValue(enemyWhoHitPlayer, playerMarkedTime, impactType);
    }

    public void SetValue(GameObject enemyWhoHitPlayer, float playerMarkedTime, ImpactType impactType)
    {
        EnemyWhoHitPlayer = enemyWhoHitPlayer;
        PlayerMarkedTime = playerMarkedTime;
        ImpactType = impactType;
    }
}

[Serializable]
public class StatisticsInformation
{
    public string StatisticsTitle;
    public string StatisticsIntro1;
    public string StatisticsIntro2;
    public Sprite StatisticIcon;
    public float Weight;
    public bool ExcludeFromMVPCalculation = false;
}

[Serializable]
public class WeaponInformation
{
    public string WeaponName;
    public int WeaponSetNumber;
    public GameObject WeaponPrefab;
}


[Serializable]
public sealed class PlayerInformation
{
    public int[] RewiredID;
    public int[] GamePlayerID;
    public int[] ColorIndex;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rewiredID"></param>
    /// <param name="gamePlayerID"></param>
    /// <param name="colorIndex">The Index of the Color of player, which is always the same set in Config</param>
    public PlayerInformation(int[] rewiredID, int[] gamePlayerID, int[] colorIndex)
    {
        RewiredID = rewiredID;
        GamePlayerID = gamePlayerID;
        ColorIndex = colorIndex;
    }
}
[Serializable]
public class ViberationInformation
{
    public float Duration = 0.15f;
    public float MotorLevel = 1f;
}

[Serializable]
public class HitStopInformation
{
    public int Frames;
    public float TimeScale = 0.1f;
    public Vector3 Viberation = Vector3.one;
    public int Vibrato = 50;
    public float Randomness = 90f;
}

[Serializable]
public class SushiRespawnInformation
{
    public GameObject RespawnTriggerGameObject;
    public Vector3 RespawnPoint;
}

public enum GameMapMode
{
    FoodMode,
    CartMode,
    FoodCartMode,
    ObjectiveMode,
    BrawlMode,
    DeathMode,
    RaceMode,
    SoccerMode,
}

public enum TeamNum
{
    Team1,
    Team2,
}

public enum GameWinType
{
    CartWin,
    FoodWin,
    ScoreWin,
    RaceWin,
}

public enum ImpactType
{
    WaterGun,
    HookGun,
    SuckGun,
    FistGun,
    BazookaGun,
    SmallBaz,
    HammerGun,
    Boomerang,
    Melee,
    Block,
    Self,
}

[Serializable]
public enum RuleType
{
    Punch,
    Gravity,
}

public interface IAimable
{

}

public interface IHittable
{
    /// <summary>
    /// Can Block The attack or not
    /// </summary>
    /// <param name="forwardAngle"></param>
    /// <returns></returns>
    bool CanBlock(Vector3 forwardAngle);
    /// <summary>
    /// A method to call when hitting a blockable object
    /// </summary>
    /// <param name="force"></param>
    /// <param name="_meleeCharge"></param>
    /// <param name="sender"></param>
    /// <param name="_blockable"></param>
    void OnImpact(Vector3 force, float _meleeCharge, GameObject sender, bool _blockable);
    /// <summary>
    /// A method to call when directly dealing impact with no blocking possiblity
    /// </summary>
    /// <param name="force"></param>
    /// <param name="forcemode"></param>
    /// <param name="enforcer"></param>
    /// <param name="impactType"></param>
    void OnImpact(Vector3 force, ForceMode forcemode, GameObject enforcer, ImpactType impactType);
    /// <summary>
    /// A method simply mark the object with the enforcer
    /// </summary>
    /// <param name="enforcer"></param>
    /// <param name="impactType"></param>
    void OnImpact(GameObject enforcer, ImpactType impactType);
    /// <summary>
    /// A impact method with status effect
    /// </summary>
    /// <param name="status"></param>
    void OnImpact(Status status);
}