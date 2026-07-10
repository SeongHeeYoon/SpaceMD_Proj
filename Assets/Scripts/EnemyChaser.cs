using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyChaser : MonoBehaviour
{
    public Transform player;

    private NavMeshAgent agent;
    private CarController playerCar;

    // 충돌 후 잠시 추적을 멈추기 위한 변수
    private bool isPaused = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player != null)
        {
            playerCar = player.GetComponent<CarController>();
        }
    }

    private void Update()
    {
        if (player == null || agent == null)
            return;

        // NavMesh 밖으로 벗어났으면 복구
        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            return;
        }

        // 플레이어가 고스트 모드거나 충돌 후 대기 중이면 정지
        if ((playerCar != null && playerCar.IsGhostMode) || isPaused)
        {
            agent.isStopped = true;
            return;
        }

        // 플레이어 추적
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Debug.Log("플레이어와 충돌!");

        // 이미 충돌 대기 중이면 무시
        if (isPaused)
            return;

        // 플레이어 HP 감소
        PlayerHP hp = collision.gameObject.GetComponent<PlayerHP>();

        if (hp != null)
        {
            hp.TakeDamage(10);
        }

        // 잠시 추적 중지
        StartCoroutine(PauseChase());
    }

    private IEnumerator PauseChase()
    {
        isPaused = true;

        agent.isStopped = true;

        yield return new WaitForSeconds(3f);

        isPaused = false;

        agent.isStopped = false;
    }
}
