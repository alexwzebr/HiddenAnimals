using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Transform levelContainer;

    private void Awake()
    {
        if (LevelsManager.Instance != null && levelContainer != null)
        {
            var containerField = typeof(LevelsManager).GetField("levelContainer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            containerField?.SetValue(LevelsManager.Instance, levelContainer);
        }
    }

    private void Start()
    {
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        if (LevelsManager.Instance == null)
        {
            Debug.LogError("LevelsManager instance is missing!");
            return;
        }

        Level levelPrefab = LevelsManager.Instance.GetCurrentLevelPrefab();
        if (levelPrefab != null)
        {
            LevelsManager.Instance.LoadLevel(levelPrefab.levelName);
        }
        else
        {
            Debug.LogError("Could not find level prefab for current level!");
        }
    }
} 