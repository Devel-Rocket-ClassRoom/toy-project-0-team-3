using UnityEngine;

public abstract class SimpleMonster : BaseMonster
{
    [Header("Detection Type Ranges")]
    [SerializeField]
    protected float alertRange = 8f;

    [SerializeField]
    protected float traceRange = 5f;

    [SerializeField]
    protected float chaseRange = 14f;

    [Header("Detection Option")]
    [SerializeField]
    protected bool requireLineOfSight = true;

    protected override void UpdateIdle()
    {
        PlayIdleAnim();

        if (player == null)
        {
            return;
        }

        if (CanDetectPlayer(traceRange))
        {
            CurrentState = MonsterState.Trace;
            return;
        }

        if (CanDetectPlayer(alertRange))
        {
            CurrentState = MonsterState.Alert;
            return;
        }
    }

    protected override void UpdateAlert()
    {
        StopMoving();
        PlayIdleAnim();

        if (player == null)
        {
            CurrentState = MonsterState.Idle;
            return;
        }

        FaceTarget(player.position);

        if (CanDetectPlayer(traceRange))
        {
            CurrentState = MonsterState.Trace;
            return;
        }

        if (!CanDetectPlayer(alertRange))
        {
            CurrentState = MonsterState.Idle;
            return;
        }
    }

    protected override void UpdateTrace()
    {
        if (player == null)
        {
            CurrentState = MonsterState.Return;
            return;
        }

        float dist = GetDistanceToPlayer();

        if (dist > chaseRange)
        {
            CurrentState = MonsterState.Return;
            return;
        }

        if (dist <= attackRange)
        {
            CurrentState = MonsterState.Attack;
            return;
        }

        MoveTo(player.position);
    }

    protected override float GetChaseRange()
    {
        return chaseRange;
    }

    private bool CanDetectPlayer(float range)
    {
        return IsPlayerInsideRange(range, requireLineOfSight);
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, traceRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
#endif
}
