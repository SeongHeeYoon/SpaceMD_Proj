using UnityEngine;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 100;

    private bool isInvincible = false;

    [SerializeField] private float invincibleTime = 1.0f;

    private int currentHP;

    private void Start()
    {
        currentHP = maxHP;

        UIManager.Instance.UpdateHP(currentHP, maxHP);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        currentHP -= damage;

        currentHP = Mathf.Max(currentHP, 0);

        UIManager.Instance.UpdateHP(currentHP, maxHP);

        if (currentHP <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(Invincible());
        }

        IEnumerator Invincible()
        {
            isInvincible = true;

            yield return new WaitForSeconds(invincibleTime);

            isInvincible = false;
        }
    }

    private void GameOver()
    {
        GameManager.Instance.GameOver();
    }


}

