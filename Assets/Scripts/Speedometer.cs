using TMPro;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    public CarController carController;
    public TextMeshProUGUI speedText;


    void Update()
    {
        float speed = carController.GetSpeed();

        speedText.text =
            Mathf.RoundToInt(speed) + " km/h";
    }
}
