using UnityEngine;

public class Initializer : MonoBehaviour
{
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject mainMenu;
    public LevelManager levelManager;

    void Awake()
    {
        Debug.Log("LOAD>> Initializer Awake");
        Application.targetFrameRate = 60;
        loading.SetActive(true);
        MainInit();
    }

    //init ads library, when ads inited, init analytics, on analytics inited, init game
    // ADS -> ANALYTICS -> GAME
    // ADS (MAX SDK -> Check GDPR region -> show consent dialog -> ads inited)

    private void MainInit()
    {
        Debug.Log("LOAD>> MainInit");
        InitializeAds();
    }

    private void InitializeAds()
    {
        //for now, skip.
        Debug.Log("LOAD>> InitializeAds");
        OnAdsInitialized();
    }

    public void OnAdsInitialized()
    {
        Debug.Log("LOAD>> OnAdsInitialized");
        InitializeAnalytics();
    }

    private void OnAnalyticsInitialized()
    {
        Debug.Log("LOAD>> OnAnalyticsInitialized");
        loading.SetActive(false);
        mainMenu.SetActive(true);
        levelManager.InitializeLevels();
    }



    private void InitializeAnalytics()
    {
        //for now, skip.
        Debug.Log("LOAD>> InitializeAnalytics");
        OnAnalyticsInitialized();
    }
}
