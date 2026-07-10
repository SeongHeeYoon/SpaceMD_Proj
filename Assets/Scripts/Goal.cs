using UnityEngine;

public class Goal : MonoBehaviour
{
    public GameObject clearPanel;

    private void OnTriggerEnter(Collider other)
    {
        ItemCollector collector = other.GetComponentInParent<ItemCollector>();

        if (collector == null)
            return;

        if (collector.IsAllCollected())
        {
            Debug.Log("GAME CLEAR!");

            clearPanel.SetActive(true);

            Time.timeScale = 0;
        }
        else
        {
            Debug.Log("아이템을 모두 모으세요!");
        }
    }
}
