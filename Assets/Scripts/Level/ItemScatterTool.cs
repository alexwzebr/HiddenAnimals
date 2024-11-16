using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemScatterTool : MonoBehaviour
{
    public GameObject itemPrefab;
    public Sprite[] sprites;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    [ContextMenu("Scatter")]
    public void Scatter()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("Please assign an item prefab!");
            return;
        }

        // Clear existing items if any
        Transform existingItems = transform.Find("Items");
        if (existingItems != null)
        {
            DestroyImmediate(existingItems.gameObject);
        }

        // Create items parent
        GameObject itemsParent = new GameObject("Items");
        itemsParent.transform.SetParent(transform);
        itemsParent.transform.localPosition = Vector3.zero;

#if UNITY_EDITOR
        foreach (var sprite in sprites)
        {
            if (sprite == null) continue;

            // Create prefab instance
            GameObject itemObj = PrefabUtility.InstantiatePrefab(itemPrefab, itemsParent.transform) as GameObject;
            itemObj.name = sprite.name;

            // Set sprite
            var spriteRenderer = itemObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }

            // Position randomly
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            itemObj.transform.localPosition = new Vector3(x, y, 0);
        }
#endif
    }
} 