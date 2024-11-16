using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 8f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float zoomSmoothTime = 0.3f;
    [SerializeField] private float minPinchDistance = 50f;

    [Header("Pan Limits")]
    [SerializeField] private Vector2 minZoomPanLimit = new Vector2(2f, 2f); // Smaller area when zoomed in
    [SerializeField] private Vector2 maxZoomPanLimit = new Vector2(10f, 10f); // Larger area when zoomed out
    [SerializeField] private float backgroundWidth = 20f;  // Width of your background in world units
    [SerializeField] private float backgroundHeight = 10f; // Height of your background in world units

    private Camera mainCamera;
    private Vector2 lastPanPosition;
    private float targetZoom;
    private float zoomVelocity;
    private float initialTouchDistance;
    private float initialZoom;
    private bool isPanning = false;
    private bool isZooming = false;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        targetZoom = mainCamera.orthographicSize;
    }

    private void Update()
    {
        if (!IsTouchOverUI())
        {
            HandlePinchZoom();
            if (!isZooming)
            {
                HandlePanning();
            }
            HandleMouseZoom();
        }
        
        // Apply zoom smoothly
        mainCamera.orthographicSize = Mathf.SmoothDamp(
            mainCamera.orthographicSize, 
            targetZoom, 
            ref zoomVelocity, 
            zoomSmoothTime
        );

        // Get interpolated pan limits based on current zoom
        Vector2 currentPanLimit = GetCurrentPanLimit();

        // Enforce position limits
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -currentPanLimit.x, currentPanLimit.x);
        position.y = Mathf.Clamp(position.y, -currentPanLimit.y, currentPanLimit.y);
        transform.position = position;
    }

    private Vector2 GetCurrentPanLimit()
    {
        // Calculate how far we are between min and max zoom (0 = min zoom, 1 = max zoom)
        float zoomProgress = Mathf.InverseLerp(minZoom, maxZoom, mainCamera.orthographicSize);
        
        // Get base pan limits interpolated between min and max zoom
        Vector2 basePanLimit = Vector2.Lerp(minZoomPanLimit, maxZoomPanLimit, zoomProgress);

        // Calculate visible area
        float visibleHeight = mainCamera.orthographicSize * 2;
        float visibleWidth = visibleHeight * mainCamera.aspect;

        // Adjust X limit based on aspect ratio and visible area
        float maxPossibleX = Mathf.Max(0, (backgroundWidth - visibleWidth) * 0.5f);
        float maxPossibleY = Mathf.Max(0, (backgroundHeight - visibleHeight) * 0.5f);

        // Return the minimum between our desired pan limit and what's actually possible
        return new Vector2(
            Mathf.Min(basePanLimit.x, maxPossibleX),
            Mathf.Min(basePanLimit.y, maxPossibleY)
        );
    }

    private bool IsTouchOverUI()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return true;
                }
            }
        }
        else if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        return false;
    }

    private void HandleMouseZoom()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float zoomDelta = Input.mouseScrollDelta.y * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom - zoomDelta, minZoom, maxZoom);
        }
    }

    private void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialTouchDistance = Vector2.Distance(touch1.position, touch0.position);
                
                if (initialTouchDistance >= minPinchDistance)
                {
                    initialZoom = mainCamera.orthographicSize;
                    isZooming = true;
                    isPanning = false;
                }
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (isZooming)
                {
                    float currentTouchDistance = Vector2.Distance(touch1.position, touch0.position);
                    float touchDelta = initialTouchDistance - currentTouchDistance;
                    
                    targetZoom = Mathf.Clamp(
                        initialZoom + (touchDelta * zoomSpeed / 100f),
                        minZoom,
                        maxZoom
                    );
                }
            }
            else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
            {
                isZooming = false;
            }
        }
        else
        {
            isZooming = false;
        }
    }

    private void HandlePanning()
    {
        // Handle touch panning
        if (Input.touchCount == 1 && !isZooming)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                lastPanPosition = (Vector2)touch.position;
                isPanning = true;
            }
            else if (touch.phase == TouchPhase.Moved && isPanning)
            {
                Vector2 currentPosition = touch.position;
                Vector2 delta = currentPosition - (Vector2)lastPanPosition;
                
                // Convert screen delta to world delta based on camera's orthographic size
                float worldUnitsPerPixel = (mainCamera.orthographicSize * 2f) / Screen.height;
                Vector3 worldDelta = new Vector3(
                    -delta.x * worldUnitsPerPixel,
                    -delta.y * worldUnitsPerPixel,
                    0
                );

                transform.position += worldDelta;
                lastPanPosition = currentPosition;
            }
        }
        
        // Handle mouse panning
        if (!isZooming && Input.GetMouseButtonDown(0))
        {
            lastPanPosition = (Vector2)Input.mousePosition;
            isPanning = true;
        }
        else if (!isZooming && Input.GetMouseButton(0) && isPanning)
        {
            Vector2 currentPosition = Input.mousePosition;
            Vector2 delta = currentPosition - (Vector2)lastPanPosition;
            
            // Convert screen delta to world delta based on camera's orthographic size
            float worldUnitsPerPixel = (mainCamera.orthographicSize * 2f) / Screen.height;
            Vector3 worldDelta = new Vector3(
                -delta.x * worldUnitsPerPixel,
                -delta.y * worldUnitsPerPixel,
                0
            );

            transform.position += worldDelta;
            lastPanPosition = currentPosition;
        }
        
        if (Input.GetMouseButtonUp(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            isPanning = false;
        }
    }
} 