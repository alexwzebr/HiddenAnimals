using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class Level : MonoBehaviour
{
    [System.Serializable]
    public class ItemGroup
    {
        public string groupName;
        public Sprite groupIcon;
        public HiddenObject[] items;
        public int totalItems => items?.Length ?? 0;
        public int remainingItems { get; set; }
    }

    [Header("Level Data")]
    public string levelName;
    public int coinsReward = 100;
    public ItemGroup[] itemGroups;

    [Header("References")]
    [SerializeField] private GameObject itemFoundEffectPrefab;
    [SerializeField] private ParticleSystem confettiEffect;
    
    private Camera mainCamera;
    
    public UnityEvent<Vector3, string> onItemFound = new UnityEvent<Vector3, string>();
    public UnityEvent onLevelComplete = new UnityEvent();

    [Header("Level Settings")]
    [SerializeField] private float levelTimer;
    private float currentTime;
    private int starsEarned;

    private Vector2 touchStartPosition;
    private bool isTouchValid = false;
    private const float MAX_TOUCH_MOVEMENT = 10f; // Maximum pixels of movement allowed for a valid touch

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        InitializeLevel();
        currentTime = 0f;

        // Find and initialize the GoalsPanel
        GoalsPanel goalsPanel = FindObjectOfType<GoalsPanel>();
        if (goalsPanel != null)
        {
            goalsPanel.Initialize(this);
        }
        else
        {
            Debug.LogError("GoalsPanel not found in the scene!");
        }
    }

    private void InitializeLevel()
    {
        // Initialize each group's remaining items count
        foreach (var group in itemGroups)
        {
            group.remainingItems = group.totalItems;
            
            // Set the group ID for each item
            foreach (var item in group.items)
            {
                if (item != null)
                {
                    item.Initialize(item.GetComponent<SpriteRenderer>().sprite, group.groupName, item.TouchAreaRadius);
                }
            }
        }
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        HandleInput();
    }

    private void HandleInput()
    {
        // Cancel touch if second finger is added
        if (Input.touchCount > 1)
        {
            isTouchValid = false;
            return;
        }

        // Handle touch input
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    isTouchValid = true;
                    break;

                case TouchPhase.Moved:
                    // If finger moved too much, invalidate the touch
                    if (isTouchValid && Vector2.Distance(touchStartPosition, touch.position) > MAX_TOUCH_MOVEMENT)
                    {
                        isTouchValid = false;
                    }
                    break;

                case TouchPhase.Ended:
                    if (isTouchValid)
                    {
                        Vector2 touchPosition = mainCamera.ScreenToWorldPoint(touch.position);
                        CheckItemHit(touchPosition);
                    }
                    isTouchValid = false;
                    break;
            }
        }
        // Handle mouse input for editor/desktop
        else if (Application.isEditor || !Application.isMobilePlatform)
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPosition = Input.mousePosition;
                isTouchValid = true;
            }
            else if (Input.GetMouseButton(0))
            {
                if (isTouchValid && Vector2.Distance(touchStartPosition, (Vector2)Input.mousePosition) > MAX_TOUCH_MOVEMENT)
                {
                    isTouchValid = false;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (isTouchValid)
                {
                    Vector2 touchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    CheckItemHit(touchPosition);
                }
                isTouchValid = false;
            }
        }
    }

    private void CheckItemHit(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.CircleCast(position, 0.1f, Vector2.zero);
        
        if (hit.collider != null)
        {
            HiddenObject item = hit.collider.GetComponent<HiddenObject>();
            if (item != null && item.TryFind())
            {
                HandleItemFound(item);
            }
        }
    }

    private void HandleItemFound(HiddenObject item)
    {
        Vector3 itemPosition = item.transform.position;
        
        // Spawn effect
        if (itemFoundEffectPrefab != null)
        {
            Instantiate(itemFoundEffectPrefab, itemPosition, Quaternion.identity);
        }
        
        // Update group counter
        foreach (var group in itemGroups)
        {
            if (group.groupName == item.GroupId)
            {
                group.remainingItems--;
                break;
            }
        }
        
        // Notify listeners (UI will handle this)
        onItemFound.Invoke(itemPosition, item.GroupId);
        
        // Calculate stars based on time and completion
        if (currentTime <= levelTimer * 0.5f)
            starsEarned = 3;
        else if (currentTime <= levelTimer * 0.75f)
            starsEarned = 2;
        else
            starsEarned = 1;
        
        // Check if level is complete
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        bool isComplete = true;
        foreach (var group in itemGroups)
        {
            if (group.remainingItems > 0)
            {
                isComplete = false;
                break;
            }
        }

        if (isComplete)
        {
            confettiEffect.Play();
            
            // Save progress
            LevelsManager.Instance.UpdateLevelProgress(
                levelName,
                true,
                starsEarned,
                currentTime,
                coinsReward
            );
            
            onLevelComplete.Invoke();
        }
    }
} 