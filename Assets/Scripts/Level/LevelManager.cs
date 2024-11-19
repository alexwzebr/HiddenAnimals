using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    private const string LEVEL_PROGRESS_KEY = "LevelProgress";
    private const string FIRST_TIME_KEY = "IsFirstTime";

    [SerializeField] private GridLayoutGroup levelGrid;
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private Transform levelContainer;

    private Dictionary<string, LevelProgressData> levelProgress;
    private Level[] levelPrefabs;
    private Level activeLevel;

    public static LevelManager Instance { get; private set; }

    public GameObject levelCompleteScreen;
    public GameObject levelCompleteConfetti;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeLevels()
    {
        Debug.Log("LOAD>> InitializeLevels");

        // Get all level elements from the grid
        LevelElement[] levelElements = levelGrid.GetComponentsInChildren<LevelElement>();
        levelPrefabs = levelElements.Select(le => le.LevelPrefab).ToArray();
        Debug.Log("LOAD>> Levels count: " + levelPrefabs.Length);

        LoadProgress();

        // Initialize each level element
        for (int i = 0; i < levelElements.Length; i++)
        {
            bool isUnlocked = i == 0 || IsLevelUnlocked(levelElements[i].LevelPrefab.levelName);
            levelElements[i].Initialize(isUnlocked);
            levelElements[i].mainMenuScreen = mainMenuScreen;
        }

        bool isFirstTime = PlayerPrefs.GetInt(FIRST_TIME_KEY, 1) == 1;
        if (isFirstTime)
        {
            PlayerPrefs.SetInt(FIRST_TIME_KEY, 0);
            PlayerPrefs.Save();
            StartLevel(levelPrefabs[0].levelName);
        }
    }

    private void LoadProgress()
    {
        // get progress for each le player prefs
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
        var progressList = new LevelProgressDataList
        {
            levels = levelProgress.Values.ToArray()
        };
        string progressJson = JsonUtility.ToJson(progressList);
        PlayerPrefs.SetString(LEVEL_PROGRESS_KEY, progressJson);
        PlayerPrefs.Save();
    }

    public void StartLevel(string levelId)
    {
        Debug.Log("LOAD>> LoadLevel");
        // Clean up active level if it exists
        if (activeLevel != null)
        {
            Destroy(activeLevel.gameObject);
        }

        mainMenuScreen.SetActive(false);

        // Instantiate new level
        Level levelPrefab = levelPrefabs.FirstOrDefault(l => l.levelName == levelId);
        if (levelPrefab != null)
        {
            activeLevel = Instantiate(levelPrefab, levelContainer);
            activeLevel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            activeLevel.transform.localScale = Vector3.one;
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

    public void UpdateLevelProgress(string levelId, bool completed)
    {
        var progress = GetLevelProgress(levelId);
        progress.isCompleted = completed;
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

    public void ReturnToMainMenu()
    {
        // update main menu level items' progress numbers
        if (activeLevel != null)
        {
            LevelElement[] levelElements = mainMenuScreen.GetComponentsInChildren<LevelElement>();
            for (int i = 0; i < levelElements.Length; i++)
            {
                levelElements[i].UpdateProgress(levelPrefabs[i], IsLevelUnlocked(levelPrefabs[i].levelName));
            }
        }

        // Clean up active level if it exists
        if (activeLevel != null)
        {
            Destroy(activeLevel.gameObject);
            activeLevel = null;
        }

        // Show main menu
        mainMenuScreen.SetActive(true);
        
    }

    public void OnLevelComplete()
    {
        levelCompleteScreen.SetActive(true);
        levelCompleteConfetti.SetActive(true);
        Destroy(levelCompleteConfetti, 3f);
    }

    public void OnNextLevelButtonClicked()
    {
        levelCompleteScreen.SetActive(false);
        levelCompleteConfetti.SetActive(false);
        ReturnToMainMenu();
    }

}

[System.Serializable]
public class LevelProgressDataList
{
    public LevelProgressData[] levels;
} 