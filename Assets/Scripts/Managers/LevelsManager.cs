using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelsManager : MonoBehaviour
{
    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    private const string LEVEL_PROGRESS_KEY = "LevelProgress";

    [SerializeField] private Level[] levelPrefabs;
    [SerializeField] private Transform levelContainer;
    
    private Dictionary<string, LevelProgressData> levelProgress;
    private string currentLevelId;
    private Level currentLevel;

    public static LevelsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
            
            // Load initial level
            if (levelPrefabs != null && levelPrefabs.Length > 0)
            {
                LoadLevel(currentLevelId);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadProgress()
    {
        // Load current level
        currentLevelId = PlayerPrefs.GetString(CURRENT_LEVEL_KEY, levelPrefabs[0].levelName);

        // Load level progress
        string progressJson = PlayerPrefs.GetString(LEVEL_PROGRESS_KEY, "");
        if (string.IsNullOrEmpty(progressJson))
        {
            levelProgress = new Dictionary<string, LevelProgressData>();
        }
        else
        {
            var progressList = JsonUtility.FromJson<LevelProgressDataList>(progressJson);
            levelProgress = progressList.levels.ToDictionary(p => p.levelId, p => p);
        }
    }

    public void SaveProgress()
    {
        // Save current level
        PlayerPrefs.SetString(CURRENT_LEVEL_KEY, currentLevelId);

        // Save level progress
        var progressList = new LevelProgressDataList
        {
            levels = levelProgress.Values.ToArray()
        };
        string progressJson = JsonUtility.ToJson(progressList);
        PlayerPrefs.SetString(LEVEL_PROGRESS_KEY, progressJson);
        PlayerPrefs.Save();
    }

    public Level GetCurrentLevelPrefab()
    {
        return levelPrefabs.FirstOrDefault(l => l.levelName == currentLevelId);
    }

    public void LoadLevel(string levelId)
    {
        // Clean up current level if it exists
        if (currentLevel != null)
        {
            Destroy(currentLevel.gameObject);
        }

        // Ensure we have a container
        if (levelContainer == null)
        {
            Debug.LogWarning("Level container not set, creating one");
            GameObject containerObj = new GameObject("LevelContainer");
            levelContainer = containerObj.transform;
        }

        currentLevelId = levelId;
        SaveProgress();

        // Instantiate new level
        Level levelPrefab = GetCurrentLevelPrefab();
        if (levelPrefab != null)
        {
            currentLevel = Instantiate(levelPrefab, levelContainer);
            currentLevel.transform.localPosition = Vector3.zero;
            currentLevel.transform.localRotation = Quaternion.identity;
            currentLevel.transform.localScale = Vector3.one;
        }
        else
        {
            Debug.LogError($"Could not find level prefab for level ID: {levelId}");
        }
    }

    public LevelProgressData GetLevelProgress(string levelId)
    {
        if (!levelProgress.ContainsKey(levelId))
        {
            levelProgress[levelId] = new LevelProgressData(levelId);
        }
        return levelProgress[levelId];
    }

    public void UpdateLevelProgress(string levelId, bool completed, int stars, float time, int coins)
    {
        var progress = GetLevelProgress(levelId);
        progress.isCompleted = completed;
        progress.starsEarned = Mathf.Max(progress.starsEarned, stars);
        progress.bestTime = progress.bestTime == 0 ? time : Mathf.Min(progress.bestTime, time);
        progress.coinsCollected += coins;
        SaveProgress();
    }

    public bool IsLevelUnlocked(string levelId)
    {
        int levelIndex = System.Array.FindIndex(levelPrefabs, l => l.levelName == levelId);
        if (levelIndex == 0) return true;
        
        string previousLevelId = levelPrefabs[levelIndex - 1].levelName;
        return GetLevelProgress(previousLevelId).isCompleted;
    }

    public Level[] GetAllLevels()
    {
        return levelPrefabs;
    }
}

[System.Serializable]
public class LevelProgressDataList
{
    public LevelProgressData[] levels;
} 