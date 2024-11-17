using UnityEngine;
using DG.Tweening;

public class HiddenObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float touchAreaRadius = 1f;
    [SerializeField] private string groupId;
    
    private PolygonCollider2D touchCollider;
    private bool isFound;

    public bool IsFound => isFound;
    public string GroupId => groupId;
    public float TouchAreaRadius => touchAreaRadius;

    private Animator animator;
    private Camera mainCamera;

    private void Awake()
    {
        touchCollider = gameObject.GetComponent<PolygonCollider2D>();
        

        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    public void Initialize(Sprite sprite, string groupId, float touchArea)
    {
        spriteRenderer.sprite = sprite;
        this.groupId = groupId;
    }

    public bool TryFind()
    {
        if (isFound) return false;
        
        isFound = true;
        touchCollider.enabled = false;

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