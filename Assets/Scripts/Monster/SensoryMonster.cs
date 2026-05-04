using UnityEngine;
using System.Collections;

public abstract class SensoryMonster : BaseMonster
{
    [Header("Sensory Settings")]
    public float maxRange = 15f;
    public float alertRange = 8f;
    public float detectRange = 4f;
    public float minHearingRange = 2f;

    // 💡 추가됨: 몬스터의 시야각 (예: 90도면 좌우 45도씩)
    [Range(0, 360)]
    public float viewAngle = 90f;

    public float alertDuration = 3f;
    public GameObject alertMarkPrefab;

    private Vector3 originalPos;
    private Vector3 alertPos;

    private bool isAlertWaiting = false;
    protected bool isAggroed = false;
    private Coroutine alertCoroutine;

    protected override void Awake()
    {
        base.Awake();
        originalPos = transform.position;
    }

    protected override void AIBehavior()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. 벽 투과 여부 (시야 확보 확인)
        bool hasLOS = HasObstacle(player.position);

        // 2. 💡 시야각(FOV) 판정: 높이(y)를 무시하고 평면상의 2D 각도만 계산합니다.
        Vector3 dirToPlayer = (player.position - transform.position);
        dirToPlayer.y = 0f; // 높이차이로 인한 각도 오차 방지
        dirToPlayer.Normalize();

        Vector3 forwardFlat = transform.forward;
        forwardFlat.y = 0f;
        forwardFlat.Normalize();

        // 몬스터의 정면과 플레이어를 향한 방향 사이의 각도를 구합니다.
        float angleToPlayer = Vector3.Angle(forwardFlat, dirToPlayer);

        // 시야각(viewAngle)의 절반보다 작다면 몬스터의 눈앞(부채꼴 안)에 있는 것입니다.
        bool inViewAngle = false;
        if (angleToPlayer <= viewAngle / 2f)
        {
            inViewAngle = true;
        }

        // 최종적으로 '앞에 있고 + 벽이 없을 때'만 보인 것으로 판정
        bool isVisible = false;
        if (hasLOS && inViewAngle)
        {
            isVisible = true;
        }

        // 3. 무조건 감지 조건 (청각 범위 안이거나, 한 대 맞았을 때)
        bool unconditionalDetect = false;
        if (distToPlayer <= minHearingRange || isAggroed)
        {
            unconditionalDetect = true;
        }

        // === 상태 판정 로직 시작 ===
        if (distToPlayer > maxRange)
        {
            isAggroed = false;

            if (currentState != MonsterState.Return)
            {
                CancelAlert();
                currentState = MonsterState.Return;
            }
        }
        else if (unconditionalDetect)
        {
            // 💡 청각/피격 감지 시: 시야를 무시하고 즉시 감지 (거리 제한도 공격 범위/추적 범위로 판단)
            CancelAlert();
            ExecuteCombatLogic(distToPlayer);
            return;
        }
        else if (isVisible)
        {
            // 💡 시각 감지 시: 부채꼴 시야 안에 들어왔을 때 거리별로 판단
            if (distToPlayer <= detectRange)
            {
                CancelAlert();
                ExecuteCombatLogic(distToPlayer);
                return;
            }
            else if (distToPlayer <= alertRange && currentState == MonsterState.Idle)
            {
                alertCoroutine = StartCoroutine(AlertRoutine(player.position));
            }
        }

        // === 기존 이동 로직 처리 ===
        switch (currentState)
        {
            case MonsterState.Idle:
                PlayIdleAnim();
                break;

            case MonsterState.Alert:
                if (isAlertWaiting)
                {
                    return;
                }

                if (Vector3.Distance(transform.position, alertPos) > 0.5f)
                {
                    Moving(alertPos);
                }
                else
                {
                    PlayIdleAnim();
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

    // 전투 진입 및 추적 로직을 분리하여 코드 중복 제거
    private void ExecuteCombatLogic(float distToPlayer)
    {
        if (distToPlayer <= attackRange)
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
            //currentState = MonsterState.Move;
            Moving(player.position);
        }
    }

    private IEnumerator AlertRoutine(Vector3 targetPos)
    {
        currentState = MonsterState.Alert;
        isAlertWaiting = true;
        alertPos = targetPos;

        if (alertMarkPrefab != null)
        {
            Instantiate(alertMarkPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
        }

        PlayIdleAnim();

        yield return new WaitForSeconds(alertDuration);

        isAlertWaiting = false;
        alertCoroutine = null;
    }

    private void CancelAlert()
    {
        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
            alertCoroutine = null;
        }
        isAlertWaiting = false;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.OnDamage(damage, hitPoint, hitNormal);

        if (!IsDead)
        {
            isAggroed = true;
        }
    }

    protected abstract override IEnumerator AttackRoutine();

    protected override void ResetBehavior()
    {
        CancelAlert();
        isAggroed = false;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        // 1. 청각 범위 (보라색, 전방위 원형)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, minHearingRange);

        // 2. 최대 거리 범위 (회색, 전방위 원형)
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, maxRange);

        // 3. 💡 시야각(FOV) 부채꼴 기즈모 그리기
        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * forward;

        // 경계 시야선 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * alertRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * alertRange);

        // 감지 시야선 (주황색)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectRange);

        // 💡 에디터 환경에서만 지원하는 Handles를 사용하면 완벽한 부채꼴(Arc)을 그릴 수 있습니다.
        UnityEditor.Handles.color = new Color(1f, 0.92f, 0.016f, 0.1f); // 반투명 노란색
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, leftBoundary, viewAngle, alertRange);

        UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.2f); // 반투명 주황색
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, leftBoundary, viewAngle, detectRange);

        // 이동 타겟 라인
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