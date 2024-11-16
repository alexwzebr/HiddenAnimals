using System;

[Serializable]
public class LevelProgressData
{
    public string levelId;
    public bool isCompleted;
    public int starsEarned;
    public float bestTime;
    public int coinsCollected;

    public LevelProgressData(string levelId)
    {
        this.levelId = levelId;
        isCompleted = false;
        starsEarned = 0;
        bestTime = 0f;
        coinsCollected = 0;
    }
} 