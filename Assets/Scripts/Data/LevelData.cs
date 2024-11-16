using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level", menuName = "Hidden Object Game/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public Sprite backgroundImage;
    public ItemGroupData[] itemGroups;
    public int coinsReward = 100;
} 