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

            // 클리어 화면 표시
            clearPanel.SetActive(true);

            // 차량 소리 및 움직임 정지
            CarController car = other.GetComponentInParent<CarController>();

            if (car != null)
            {
                car.StopCarSound();
            }

            // 게임 정지
            Time.timeScale = 0;
        }
        else
        {
            Debug.Log("아이템을 모두 모으세요!");
        }
    }
}
