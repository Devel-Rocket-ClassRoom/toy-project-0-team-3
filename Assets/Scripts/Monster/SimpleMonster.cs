using UnityEngine;
using System.Collections;

public abstract class SimpleMonster : BaseMonster
{
    [Header("Simple Monster Settings")]
    public float detectRange = 5f;
    public float chaseRange = 10f;
    protected bool isChasing = false;

    protected override void AIBehavior()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isChasing)
        {
            if (distToPlayer <= detectRange && HasObstacle(player.position))
            {
                isChasing = true;
            }
            else
            {
                currentState = MonsterState.Idle;
                PlayIdleAnim();
            }
        }

        if (isChasing)
        {
            if (distToPlayer > chaseRange)
            {
                isChasing = false;
                currentState = MonsterState.Idle;
                PlayIdleAnim();
            }
            else if (distToPlayer <= attackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartCoroutine(AttackProcess());
                }
                else
                {
                    currentState = MonsterState.Idle;
                    PlayIdleAnim();
                }
            }
            else
            {
                currentState = MonsterState.Move;
                Moving(player.position);
            }
        }
    }

    protected abstract override IEnumerator AttackRoutine();

    protected override void ResetBehavior()
    {
        isChasing = false;

        Debug.Log($"{gameObject.name}의 추적 상태가 초기화");
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

        if (Application.isPlaying && isChasing && player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
#endif
}