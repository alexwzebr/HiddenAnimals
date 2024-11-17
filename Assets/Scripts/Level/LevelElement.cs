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

        // Get progress data
        LevelProgressData progress = LevelManager.Instance.GetLevelProgress(levelId);

        // Setup button
        playButton.onClick.AddListener(() => OnLevelSelected());
        playButton.interactable = isUnlocked;

        // Show appropriate state
        lockedState.SetActive(!isUnlocked);
        unlockedState.SetActive(isUnlocked && !progress.isCompleted);
        completedState.SetActive(progress.isCompleted);

        // Update progress if level is in progress
        if (isUnlocked)
        {
            int totalObjects = 0;
            int foundObjects = 0;

            foreach (var group in levelPrefab.itemGroups)
            {
                totalObjects += group.totalItems;
            }

            foundObjects = progress.foundItems.Count;

            // progress bar fills up with every object found
            float progressValue = (float)foundObjects / totalObjects;
            progressBar.value = progressValue;
            progressText.text = $"{foundObjects}/{totalObjects}";

            // Set button text based on progress
            if (progress.isCompleted)
            {
                playButtonText.text = "Play Again";
            }
            else if (foundObjects > 0)
            {
                playButtonText.text = "Continue";
            }
            else
            {
                playButtonText.text = "Play";
            }
        }
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
} 