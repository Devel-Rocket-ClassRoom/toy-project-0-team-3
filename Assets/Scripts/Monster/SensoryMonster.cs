using UnityEngine;
using System.Collections;

public abstract class SensoryMonster : BaseMonster
{
    [Header("Sensory Settings")]
    public float detectRange = 4f;
    public float alertRange = 8f;
    public float alertDuration = 3f;
    public GameObject alertMarkPrefab;

    private Vector3 originalPos;
    private Vector3 alertPos;
    private float alertTimer = 0f;

    protected override void Awake()
    {
        base.Awake();

        originalPos = transform.position;
    }

    protected override void AIBehavior()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLOS = HasObstacle(player.position);

        if (distToPlayer <= detectRange && hasLOS)
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
            return;
        }

        switch (currentState)
        {
            case MonsterState.Idle:
                PlayIdleAnim();

                if (distToPlayer <= alertRange && hasLOS)
                {
                    TriggerAlert(player.position);
                }
                break;

            case MonsterState.Alert:
                if (Vector3.Distance(transform.position, alertPos) > 0.5f)
                {
                    Moving(alertPos);
                }
                else
                {
                    PlayIdleAnim();
                    alertTimer += Time.deltaTime;

                    if (alertTimer >= alertDuration)
                    {
                        currentState = MonsterState.Return;
                    }
                }
                break;

            case MonsterState.Return:
                if (Vector3.Distance(transform.position, originalPos) > 0.5f)
                {
                    Moving(originalPos);
                }
                else
                {
                    currentState = MonsterState.Idle;
                    FaceTarget(originalPos + transform.forward);
                }
                break;
        }
    }

    private void TriggerAlert(Vector3 targetPos)
    {
        currentState = MonsterState.Alert;
        alertPos = targetPos;
        alertTimer = 0f;

        if (alertMarkPrefab != null)
        {
            Instantiate(alertMarkPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
        }
    }

    protected abstract override IEnumerator AttackRoutine();

    protected override void ResetBehavior()
    {
        
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        if (Application.isPlaying)
        {
            if (currentState == MonsterState.Alert)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, alertPos);
            }
            else if (currentState == MonsterState.Return)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, originalPos);
            }
        }
    }
#endif
}