using UnityEngine;
using System;

[Serializable]
public class ItemGroupData
{
    public string groupName;
    public Sprite groupIcon;
    public int totalItems;
    public int remainingItems { get; set; }
} 