using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "NetworkTestMenuData", menuName = "ScriptableObjects/NetworkTestMenuData", order = 1)]
public class NetworkTestMenuData : ScriptableObject
{
    public float CursorMoveSpeed = 10f;
    public Sprite[] PlayerSprites;
    public Color PlayerHoverSpriteAndNameColor = Color.grey;
}
