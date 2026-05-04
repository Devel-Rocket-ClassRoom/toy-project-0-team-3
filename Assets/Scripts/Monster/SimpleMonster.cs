using UnityEngine;
using System.Collections;

public abstract class SimpleMonster : BaseMonster
{
    [Header("Simple Monster Settings")]
    public float detectRange = 5f;
    public float chaseRange = 10f;

    protected override void UpdateIdle()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= detectRange && HasObstacle(player.position))
        {
            CurrentState = MonsterState.Trace;
            return;
        }

        // 전이되지 않았다면 매 프레임 대기 애니메이션 갱신
        PlayIdleAnim();
    }

    // 💡 2. 추적 상태 로직 (UpdateTrace 오버라이드)
    protected override void UpdateTrace()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer > chaseRange)
        {
            CurrentState = MonsterState.Idle;
            return;
        }

        if (distToPlayer <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackProcess());
            }
            else
            {
                CurrentState = MonsterState.Idle;
            }
            return;
        }

        // 거리가 닿지 않으면 계속 이동
        Moving(player.position);
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // CurrentState가 Trace일 때만 추적 선을 그림
        if (Application.isPlaying && CurrentState == MonsterState.Trace && player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
#endif
}