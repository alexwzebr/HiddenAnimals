using UnityEngine;
using DG.Tweening;

public class HiddenObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float touchAreaRadius = 1f;
    [SerializeField] private string groupId;
    
    private CircleCollider2D touchCollider;
    private bool isFound;

    public bool IsFound => isFound;
    public string GroupId => groupId;
    public float TouchAreaRadius => touchAreaRadius;

    private Animator animator;

    private void Awake()
    {
        touchCollider = gameObject.AddComponent<CircleCollider2D>();
        touchCollider.isTrigger = true;
        UpdateTouchArea(touchAreaRadius);

        animator = GetComponent<Animator>();
    }

    public void Initialize(Sprite sprite, string groupId, float touchArea)
    {
        spriteRenderer.sprite = sprite;
        this.groupId = groupId;
        UpdateTouchArea(touchArea);
    }

    public void UpdateTouchArea(float radius)
    {
        touchAreaRadius = radius;
        if (touchCollider != null)
        {
            touchCollider.radius = radius;
        }
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
        
        // Calculate a control point for curved path
        Vector3 startPos = transform.position;
        Vector3 controlPoint = Vector3.Lerp(startPos, targetWorldPosition, 0.5f);
        controlPoint += new Vector3(0, 2f, 0); // Add some height to the curve

        // Create path
        Vector3[] path = new Vector3[] { startPos, controlPoint, targetWorldPosition };

        sequence.AppendCallback(() => {
            animator.enabled = false;
        });

        // Path movement
        sequence.Append(transform.DOPath(path, 0.5f, PathType.CatmullRom)
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