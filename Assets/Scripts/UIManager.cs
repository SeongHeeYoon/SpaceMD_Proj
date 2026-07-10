using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateHP(int currentHP, int maxHP)
    {
        hpSlider.maxValue = maxHP;
        hpSlider.value = currentHP;

        hpText.text = "HP : " + currentHP;
    }
}
