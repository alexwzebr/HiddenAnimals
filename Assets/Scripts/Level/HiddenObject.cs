using UnityEngine;
using DG.Tweening;

public class HiddenObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float touchAreaRadius = 1f;
    [SerializeField] private string groupId;
    [SerializeField] private string itemId;
    
    private PolygonCollider2D touchCollider;
    private bool isFound;

    public bool IsFound => isFound;
    public string GroupId => groupId;
    public float TouchAreaRadius => touchAreaRadius;
    public string ItemId => itemId;

    private Animator animator;
    private Camera mainCamera;

    public void InitHiddenObject(string groupId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = gameObject.name;
        }

        // Get required components
        touchCollider = GetComponent<PolygonCollider2D>();
        if (touchCollider == null)
        {
            Debug.LogError($"Missing PolygonCollider2D on {gameObject.name}");
            return;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"Missing Animator on {gameObject.name}");
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"Missing SpriteRenderer on {gameObject.name}");
                return;
            }
        }

        mainCamera = Camera.main;
        this.groupId = groupId;

        // Check if LevelManager is available
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager.Instance is null!");
            return;
        }

        Level parentLevel = GetComponentInParent<Level>();
        if (parentLevel == null)
        {
            Debug.LogError($"Cannot find parent Level component for {gameObject.name}");
            return;
        }

        // Check if item was previously found
        var progress = LevelManager.Instance.GetLevelProgress(parentLevel.levelName);
        if (progress != null && progress.foundItems.Contains(itemId))
        {
            isFound = true;
            gameObject.SetActive(false); // Use SetActive instead of Destroy for safety
        }
    }

    public bool TryFind()
    {
        if (isFound) return false;
        
        isFound = true;
        if (touchCollider != null)
        {
            touchCollider.enabled = false;
        }

        // Add null checks
        Level level = GetComponentInParent<Level>();
        if (level == null)
        {
            Debug.LogError($"Cannot find parent Level component for {gameObject.name}");
            return true;
        }

        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager.Instance is null!");
            return true;
        }

        var progress = LevelManager.Instance.GetLevelProgress(level.levelName);
        if (progress == null)
        {
            Debug.LogError($"Could not get progress for level: {level.levelName}");
            return true;
        }

        if (!progress.foundItems.Contains(itemId))
        {
            progress.foundItems.Add(itemId);
            LevelManager.Instance.SaveProgress();
        }

        return true;
    }

    public void FlyToTarget(Vector3 targetWorldPosition, System.Action onComplete)
    {
        // Change sorting layer to appear above UI
        spriteRenderer.sortingLayerName = "Canvas";
        spriteRenderer.sortingOrder = 1500;

        animator.enabled = true;

        // Create sequence with initial delay
        Sequence sequence = DOTween.Sequence();
        
        // Add 1 second delay
        sequence.AppendInterval(1f);

        // Store the world position before parenting
        Vector3 startWorldPos = transform.position;
        
        // Parent to camera and convert target to local space
        transform.SetParent(mainCamera.transform);
        Vector3 targetLocalPos = mainCamera.transform.InverseTransformPoint(targetWorldPosition);
        Vector3 startLocalPos = mainCamera.transform.InverseTransformPoint(startWorldPos);
        
        // Calculate control point in local space
        Vector3 controlPoint = Vector3.Lerp(startLocalPos, targetLocalPos, 0.5f);
        controlPoint += new Vector3(0, 2f, 0); // Add some height to the curve

        // Create path in local space
        Vector3[] path = new Vector3[] { startLocalPos, controlPoint, targetLocalPos };

        sequence.AppendCallback(() => {
            animator.enabled = false;
        });

        // Path movement in local space
        sequence.Append(transform.DOLocalPath(path, 0.5f, PathType.CatmullRom)
            .SetEase(Ease.OutQuad));

        // Scale down
        sequence.Join(transform.DOScale(0.1f, 0.5f)
            .SetEase(Ease.InQuad));

        // Fade out
        sequence.Join(spriteRenderer.DOFade(0, 0.5f)
            .SetEase(Ease.InQuad));

        sequence.OnComplete(() => {
            Destroy(gameObject);
            onComplete?.Invoke();
        });
    }
} 