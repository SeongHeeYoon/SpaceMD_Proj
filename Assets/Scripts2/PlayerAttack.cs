using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform enemyTarget;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (enemyTarget != null)
            {
                ThrowProjectile();
            }
        }
    }

    void ThrowProjectile()
    {
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Projectile projectileScript = projectile.GetComponent<Projectile>();

        // 정규화 해서 속도를 일정하게 만들기
        Vector2 direction =
            (enemyTarget.position - firePoint.position).normalized;

        projectileScript.SetDirection(direction);
    }
}
