using UnityEngine;

public class Initializer : MonoBehaviour
{
    [SerializeField] private GameObject loading;
    [SerializeField] private GameObject mainMenu;
    void Awake()
    {
        Application.targetFrameRate = 60;
        loading.SetActive(true);
        InitializeGame();
    }

    private void InitializeGame()
    {
        InitializeAds();
    }

    public void OnAdsInitialized()
    {
        OnAllInitialized();
    }

    private void OnAllInitialized()
    {
        loading.SetActive(false);
        mainMenu.SetActive(true);
    }

    private void InitializeAds()
    {
        //for now, skip.
        OnAdsInitialized();
    }
}