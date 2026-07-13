using TMPro;
using UnityEngine;

public class ItemCollector : MonoBehaviour
{

    public GameObject goal;
    // 전체 아이템 개수
    public int totalItemCount;

    // 먹은 아이템 개수
    public int currentItemCount;

    // UI 텍스트
    public TextMeshProUGUI itemText;

    void Start()
    {

        goal.SetActive(false);
        UpdateUI();
    }

    public void CollectItem()
    {
        currentItemCount++;
        UpdateUI();

        if (currentItemCount >= totalItemCount)
        {
            goal.SetActive(true);
        }
    }

    void UpdateUI()
    {
        itemText.text = currentItemCount + " / " + totalItemCount;
    }

    public bool IsAllCollected()
    {
        return currentItemCount >= totalItemCount;
    }
}