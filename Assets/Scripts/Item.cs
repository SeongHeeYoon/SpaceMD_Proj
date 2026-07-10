using UnityEngine;

public class Item : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ItemCollector collector = other.GetComponentInParent<ItemCollector>();

        if (collector != null)
        {
            collector.CollectItem();
            Destroy(gameObject);
        }
    }
}
