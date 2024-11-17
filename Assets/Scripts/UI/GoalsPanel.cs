using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System.Linq;

public class GoalsPanel : MonoBehaviour
{
    [SerializeField] private GameObject goalSlotPrefab;
    [SerializeField] private RectTransform goalsContainer;
    [SerializeField] private GameObject flyingIconPrefab;
    private GoalSlot[] goalSlots;
    private Level currentLevel;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize(Level level)
    {
        if (level == null)
        {
            Debug.LogError("Trying to initialize GoalsPanel with null level!");
            return;
        }

        Debug.Log($"Initializing GoalsPanel with level: {level.levelName}"); // Debug log

        currentLevel = level;
        
        // Clear existing goal slots
        if (goalSlots != null)
        {
            foreach (var slot in goalSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
        }

        // Create new array based on number of item groups
        goalSlots = new GoalSlot[level.itemGroups.Length];

        Debug.Log($"Creating {level.itemGroups.Length} goal slots"); // Debug log

        // Create goal slots dynamically
        for (int i = 0; i < level.itemGroups.Length; i++)
        {
            var group = level.itemGroups[i];
            
            // Instantiate new goal slot
            GameObject slotObj = Instantiate(goalSlotPrefab, goalsContainer);
            GoalSlot slot = slotObj.GetComponent<GoalSlot>();
            
            if (slot == null)
            {
                Debug.LogError("GoalSlot component not found on prefab!");
                continue;
            }

            goalSlots[i] = slot;
            
            // Setup slot
            slot.Setup(group.groupIcon);
            UpdateGoalCount(i);
        }

        // Subscribe to events
        level.onItemFound.AddListener(OnItemFound);
    }

    private void OnDestroy()
    {
        if (currentLevel != null)
        {
            currentLevel.onItemFound.RemoveListener(OnItemFound);
        }
    }

    private void OnItemFound(Vector3 worldPosition, string groupId)
    {
        // Find the corresponding goal slot
        for (int i = 0; i < currentLevel.itemGroups.Length && i < goalSlots.Length; i++)
        {
            if (currentLevel.itemGroups[i].groupName == groupId)
            {
                // Find the hidden object at the world position
                HiddenObject hiddenObject = currentLevel.itemGroups[i].items.FirstOrDefault(
                    item => item != null && 
                    Vector3.Distance(item.transform.position, worldPosition) < 0.1f
                );

                if (hiddenObject != null)
                {
                    // Get the screen position of the goal slot
                    RectTransform slotRect = goalSlots[i].GetComponent<RectTransform>();
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, slotRect.position);
                    
                    // Convert screen position to world position
                    Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 10f));
                    
                    hiddenObject.FlyToTarget(targetWorldPos, () => {
                        UpdateGoalCount(i);
                    });
                }
                break;
            }
        }
    }

    private void UpdateGoalCount(int index)
    {
        var group = currentLevel.itemGroups[index];
        var slot = goalSlots[index];

        slot.UpdateCount(
            group.foundItems.Count,
            group.totalItems
        );
    }
} 