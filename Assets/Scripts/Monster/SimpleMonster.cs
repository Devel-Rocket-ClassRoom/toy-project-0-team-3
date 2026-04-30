using UnityEngine;

public class SimpleMonster : BaseMonster
{
    [Header("Simple Monster Settings")]
    public float detectionRadius = 5f; 
    public float chaseRadius = 10f;    

    protected override void UpdateIdle()
    {
        if (playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= detectionRadius && HasLineOfSight(playerTarget))
        {
            currentState = MonsterState.Chase;
        }
    }

    protected override void UpdateChase()
    {
        if (playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance > chaseRadius || !HasLineOfSight(playerTarget))
        {
            currentState = MonsterState.Idle;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }
}