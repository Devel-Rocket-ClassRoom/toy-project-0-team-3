using UnityEngine;
using System.Collections.Generic;

public abstract class BaseMonster : MonoBehaviour
{
    [Header("Base Stats")]
    public string monsterName = "Unknown";
    public float maxHp = 100f;
    protected float currentHp;
    public float moveSpeed = 3f;
    public float attackDamage = 10f;

    [Header("Layer Settings")]
    public LayerMask wallLayer; 

    [Header("Extraction Loot")]
    public List<GameObject> dropItems;

    protected Transform playerTarget;

    public enum MonsterState { Idle, Investigate, Chase, Attack, Dead }
    protected MonsterState currentState = MonsterState.Idle;

    protected virtual void Start()
    {
        currentHp = maxHp;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
        else
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }
    }

    protected virtual void Update()
    {
        if (currentState == MonsterState.Dead || playerTarget == null) return;

        switch (currentState)
        {
            case MonsterState.Idle:
                UpdateIdle();
                break;
            case MonsterState.Investigate:
                UpdateInvestigate();
                break;
            case MonsterState.Chase:
                UpdateChase();
                break;
            case MonsterState.Attack:
                UpdateAttack();
                break;
        }
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateInvestigate() { }
    protected virtual void UpdateChase() { }
    protected virtual void UpdateAttack() { }

    protected bool HasLineOfSight(Transform target)
    {
        if (target == null) return false;

        Vector3 directionToTarget = target.position - transform.position;
        float distanceToTarget = directionToTarget.magnitude;


        if (Physics.Raycast(transform.position, directionToTarget.normalized, distanceToTarget, wallLayer))
        {
            return false; 
        }
        return true; 
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentState == MonsterState.Dead) return;

        currentHp -= damage;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        currentState = MonsterState.Dead;
        DropLoot();
        Destroy(gameObject, 2f); 
    }

    protected virtual void DropLoot()
    {
        // 전리품 드랍 로직 구현부
    }
}