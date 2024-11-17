using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup menuCanvas;
    [SerializeField] private Button playButton;
    [SerializeField] private Button levelsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private LevelSelection levelSelection;
    
    private const string FIRST_TIME_KEY = "IsFirstTime";

    private void Start()
    {
        bool isFirstTime = PlayerPrefs.GetInt(FIRST_TIME_KEY, 1) == 1;

        if (isFirstTime)
        {
            // First time launch - start Level 1 directly
            PlayerPrefs.SetInt(FIRST_TIME_KEY, 0);
            PlayerPrefs.Save();
            StartLevel(LevelSelection.Instance.GetAllLevels()[0].levelName);
        }
        else
        {
            // Show menu
            ShowMenu();
        }

        // Setup button listeners
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        
        if (levelsButton != null)
            levelsButton.onClick.AddListener(OnLevelsClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void ShowMenu()
    {
        menuCanvas.gameObject.SetActive(true);
    }

    private void HideMenu()
    {
        menuCanvas.gameObject.SetActive(false);
    }

    private void OnPlayClicked()
    {
        // Continue from last played level or start from first level
        Level currentLevel = LevelSelection.Instance.GetCurrentLevelPrefab();
        StartLevel(currentLevel.levelName);
    }

    private void OnLevelsClicked()
    {
        
    }

    private void OnSettingsClicked()
    {
        // TODO: Show settings screen
        HideMenu();
    }

    private void StartLevel(string levelName)
    {
        HideMenu();
        LevelSelection.Instance.LoadLevel(levelName);
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);
        
        if (levelsButton != null)
            levelsButton.onClick.RemoveListener(OnLevelsClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }
} 