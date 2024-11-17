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
        public List<string> foundItems = new List<string>();
        public int totalItems => items?.Length ?? 0;
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

    private Vector2 touchStartPosition;
    private bool isTouchValid = false;
    private const float MAX_TOUCH_MOVEMENT = 10f; // Maximum pixels of movement allowed for a valid touch

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager.Instance is null! Ensure LevelManager is initialized before loading levels.");
            return;
        }

        InitializeLevel();

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
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("Level name is not set!");
            return;
        }

        var progress = LevelManager.Instance.GetLevelProgress(levelName);
        if (progress == null)
        {
            Debug.LogError($"Could not get progress for level: {levelName}");
            return;
        }

        // Initialize each group's remaining items count
        foreach (var group in itemGroups)
        {
            if (group == null || group.items == null)
            {
                Debug.LogError("Null item group or items array found!");
                continue;
            }

            foreach (var item in group.items)
            {
                if (item != null)
                {
                    // Initialize all items, not just found ones
                    item.InitHiddenObject(group.groupName);

                    // Check if item was previously found
                    if (progress.foundItems.Contains(item.ItemId))
                    {
                        //add to found list
                        group.foundItems.Add(item.ItemId);
                        item.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void Update()
    {
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
        Collider2D hitCollider = Physics2D.OverlapPoint(position);
        
        if (hitCollider != null)
        {
            HiddenObject item = hitCollider.GetComponent<HiddenObject>();
            if (item != null && item.TryFind())
            {
                HandleItemFound(item);
            }
        }
    }

    private void HandleItemFound(HiddenObject item)
    {
        Debug.Log("GAME>> Item found: " + item.GroupId);
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
                //add item to found list
                group.foundItems.Add(item.ItemId);
                break;
            }
        }
        
        // Notify listeners (UI will handle this)
        onItemFound.Invoke(itemPosition, item.GroupId);
        
        // Check if level is complete
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        bool isComplete = true;

        // Check if all items are found in each group
        foreach (var group in itemGroups)
        {
            Debug.Log($"Group {group.groupName} has {group.totalItems} items, found {group.foundItems.Count}");
            if (group.totalItems > group.foundItems.Count)
            {
                isComplete = false;
                break;
            }
        }

        if (isComplete)
        {
            confettiEffect.Play();
            
            // Save progress
            LevelManager.Instance.UpdateLevelProgress(
                levelName,
                true
            );
            
            onLevelComplete.Invoke();
        }
    }
} 