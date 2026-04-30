using UnityEngine;

public class SensoryMonster : BaseMonster
{
    [Header("Vision Settings (시야)")]
    public float warningViewRadius = 12f;  
    public float detectionViewRadius = 5f; 
    [Range(0, 360)]
    public float viewAngle = 110f;         
    public float proximityRadius = 2f;     

    [Header("Hearing Settings (청각)")]
    public float baseHearingRadius = 6f;   

    [Header("UI & State Settings")]
    public GameObject exclamationMarkUI;
    private Vector3 lastKnownPosition;
    private float investigateTimer = 0f;
    public float investigateDuration = 3f;

    protected override void Start()
    {
        base.Start();
        if (exclamationMarkUI != null) exclamationMarkUI.SetActive(false);
    }

    protected override void UpdateIdle()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;

        if (distanceToPlayer <= proximityRadius)
        {
            TriggerChase(playerTarget.position);
            return;
        }

        if (!HasLineOfSight(playerTarget)) return;

        if (distanceToPlayer <= warningViewRadius)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < viewAngle / 2f)
            {
                if (distanceToPlayer <= detectionViewRadius)
                {
                    TriggerChase(playerTarget.position);
                    return;
                }
                else
                {
                    TriggerInvestigate(playerTarget.position);
                    return;
                }
            }
        }

        float playerNoise = 0f; 
        float totalHearingRange = baseHearingRadius + playerNoise;

        if (distanceToPlayer <= totalHearingRange)
        {
            TriggerInvestigate(playerTarget.position);
        }
    }

    private void TriggerChase(Vector3 targetPos)
    {
        if (exclamationMarkUI != null) exclamationMarkUI.SetActive(true);
        currentState = MonsterState.Chase;
    }

    private void TriggerInvestigate(Vector3 soundPos)
    {
        if (exclamationMarkUI != null) exclamationMarkUI.SetActive(true);
        lastKnownPosition = soundPos;
        investigateTimer = 0f;
        currentState = MonsterState.Investigate;
    }

    protected override void UpdateInvestigate()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (HasLineOfSight(playerTarget) && angleToPlayer < viewAngle / 2f)
        {
            if (distanceToPlayer <= detectionViewRadius)
            {
                TriggerChase(playerTarget.position);
                return;
            }
            else if (distanceToPlayer <= warningViewRadius)
            {
                lastKnownPosition = playerTarget.position;
                investigateTimer = 0f;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, lastKnownPosition, (moveSpeed * 0.5f) * Time.deltaTime);

        if (Vector3.Distance(transform.position, lastKnownPosition) < 0.5f)
        {
            investigateTimer += Time.deltaTime;
            if (investigateTimer >= investigateDuration)
            {
                if (exclamationMarkUI != null) exclamationMarkUI.SetActive(false);
                currentState = MonsterState.Idle;
            }
        }
    }

    protected override void UpdateChase()
    {
        if (playerTarget == null) return;

        if (!HasLineOfSight(playerTarget))
        {
            TriggerInvestigate(playerTarget.position);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        // 1. 초근접 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);

        // 2. 청각 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, baseHearingRadius);

        Vector3 rightDir = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;
        Vector3 leftDir = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;

        // 3. 확정 발각 시야 (주황색 부채꼴)
        Gizmos.color = new Color(1f, 0.5f, 0f); // 주황색
        Gizmos.DrawRay(transform.position, rightDir * detectionViewRadius);
        Gizmos.DrawRay(transform.position, leftDir * detectionViewRadius);
        Gizmos.DrawRay(transform.position, transform.forward * detectionViewRadius);

        // 4. 멀리 경계 시야 (파란색 부채꼴)
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + rightDir * detectionViewRadius, rightDir * (warningViewRadius - detectionViewRadius));
        Gizmos.DrawRay(transform.position + leftDir * detectionViewRadius, leftDir * (warningViewRadius - detectionViewRadius));
        Gizmos.DrawRay(transform.position + transform.forward * detectionViewRadius, transform.forward * (warningViewRadius - detectionViewRadius));
    }
}