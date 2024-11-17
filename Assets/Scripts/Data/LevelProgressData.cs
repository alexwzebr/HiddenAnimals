using System;
using System.Collections.Generic;

[Serializable]
public class LevelProgressData
{
    public string levelId;
    public bool isCompleted;
    public List<string> foundItems;

    public LevelProgressData(string levelId)
    {
        this.levelId = levelId;
        isCompleted = false;
        foundItems = new List<string>();
    }
} 