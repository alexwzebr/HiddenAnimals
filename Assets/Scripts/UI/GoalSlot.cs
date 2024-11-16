using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private GameObject completeMark;

    public void Setup(Sprite groupIcon)
    {
        icon.sprite = groupIcon;
        completeMark.SetActive(false);
    }

    public void UpdateCount(int found, int total)
    {
        countText.text = $"{found}/{total}";
        completeMark.SetActive(found == total);
    }
} 