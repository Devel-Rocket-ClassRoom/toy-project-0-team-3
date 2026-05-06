using UnityEngine;

public abstract class SensoryMonster : BaseMonster
{
    [Header("Sight Type Settings")]
    [SerializeField] protected float viewAngle = 120f;

    [Header("Sight Ranges")]
    [SerializeField] protected float viewAlertRange = 12f;
    [SerializeField] protected float viewTraceRange = 6f;

    [Header("Alert Detection")]
    [SerializeField] protected float detectionRange = 2.5f;
    [SerializeField] protected float alertDuration = 3f;
    [SerializeField] protected float alertArriveDistance = 0.45f;

    [Header("Chase")]
    [SerializeField] protected float chaseRange = 18f;

    private Vector3 alertPosition;
    private float alertTimer;
    private bool hasArrivedAlertPosition;

    protected override void UpdateIdle()
    {
        PlayIdleAnim();

        if (player == null)
        {
            return;
        }


        if (IsPlayerInsideView(viewTraceRange, viewAngle))
        {
            CurrentState = MonsterState.Trace;
            return;
        }

        if (IsPlayerInsideView(viewAlertRange, viewAngle))
        {
            BeginAlert(player.position);
            return;
        }

        float dist = GetDistanceToPlayer();

        if (dist <= attackRange)
        {
            CurrentState = MonsterState.Attack;
            return;
        }
    }

    protected override void UpdateAlert()
    {
        if (player == null)
        {
            CurrentState = MonsterState.Return;
            return;
        }

        float dist = GetDistanceToPlayer();

        if (dist <= detectionRange)
        {
            CurrentState = MonsterState.Trace;
            return;
        }

        if (dist <= viewTraceRange && HasLineOfSight(player.position))
        {
            CurrentState = MonsterState.Trace;
            return;
        }

        if (!hasArrivedAlertPosition)
        {
            if (GetFlatDistance(transform.position, alertPosition) > alertArriveDistance)
            {
                MoveTo(alertPosition);
                return;
            }

            hasArrivedAlertPosition = true;
            StopMoving();
            PlayIdleAnim();
            alertTimer = 0f;
        }

        StopMoving();
        PlayIdleAnim();

        FaceTarget(player.position);

        alertTimer += Time.deltaTime;

        if (alertTimer >= alertDuration)
        {
            CurrentState = MonsterState.Return;
            return;
        }
    }

    private float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
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

    private void BeginAlert(Vector3 targetPosition)
    {
        alertPosition = targetPosition;
        alertTimer = 0f;
        hasArrivedAlertPosition = false;

        CurrentState = MonsterState.Alert;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(0.2f, 0.9f, 1f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Vector3 forward = transform.forward;

        Gizmos.color = Color.yellow;
        DrawViewCone(transform.position, forward, viewAngle, viewAlertRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        DrawViewCone(transform.position, forward, viewAngle, viewTraceRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(alertPosition, 0.15f);
            Gizmos.DrawLine(transform.position, alertPosition);
        }
    }

    private void DrawViewCone(Vector3 position, Vector3 forward, float angle, float range)
    {
        Vector3 left = Quaternion.Euler(0f, -angle * 0.5f, 0f) * forward;
        Vector3 right = Quaternion.Euler(0f, angle * 0.5f, 0f) * forward;

        Gizmos.DrawRay(position, left * range);
        Gizmos.DrawRay(position, right * range);

        int segments = 24;
        Vector3 previous = position + left * range;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -angle * 0.5f + angle * i / segments;
            Vector3 dir = Quaternion.Euler(0f, currentAngle, 0f) * forward;
            Vector3 next = position + dir * range;

            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
#endif
}