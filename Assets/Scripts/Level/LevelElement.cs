using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Linq;

public class LevelElement : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private GameObject lockedState;
    [SerializeField] private GameObject unlockedState;
    [SerializeField] private GameObject completedState;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Level levelPrefab;
    [HideInInspector] public GameObject mainMenuScreen;

    private string levelId;

    public Level LevelPrefab => levelPrefab;

    public void Initialize(bool isUnlocked)
    {
        if (levelPrefab == null)
        {
            Debug.LogError($"Level prefab not assigned for {gameObject.name}!");
            return;
        }

        levelId = levelPrefab.levelName;
        
        // Set level name
        levelNameText.text = levelId;

        // Setup button
        playButton.onClick.AddListener(() => OnLevelSelected());
        playButton.interactable = isUnlocked;

        UpdateProgress(levelPrefab, isUnlocked);
        
    }

    private void OnLevelSelected()
    {
        mainMenuScreen.SetActive(false);
        LevelManager.Instance.StartLevel(levelId);
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveAllListeners();
    }

    public void UpdateProgress(Level level, bool isUnlocked)
    {
        LevelProgressData progress = LevelManager.Instance.GetLevelProgress(levelId);

        int foundObjects = 0;
        int totalObjects = 0;

        foundObjects = progress.foundItems.Count;
        totalObjects = level.itemGroups.Sum(group => group.totalItems);

        float progressValue = (float)foundObjects / totalObjects;
        progressBar.value = progressValue;
        progressText.text = $"{foundObjects}/{totalObjects}";

        // Show appropriate state
        lockedState.SetActive(!isUnlocked);
        unlockedState.SetActive(isUnlocked && !progress.isCompleted);
        completedState.SetActive(progress.isCompleted);

        if (isUnlocked)
        {
            if (progress.isCompleted)
            {
                playButtonText.text = "Play Again";
            }
            else if (progress.foundItems.Count > 0)
            {
                playButtonText.text = "Continue";
            }
            else
            {
                playButtonText.text = "Play";
            }
            
        }
        
    }
} 