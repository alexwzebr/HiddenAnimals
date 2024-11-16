using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FlyingIcon : MonoBehaviour
{
    private void Awake()
    {
        // Ensure the flying icon is rendered on top of other UI elements
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        // Add a canvas group for fade effects if needed
        CanvasGroup group = gameObject.AddComponent<CanvasGroup>();
    }
} 