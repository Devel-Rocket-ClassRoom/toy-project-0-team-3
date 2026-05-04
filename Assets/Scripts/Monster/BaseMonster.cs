using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseMonster : LivingEntity
{
    [Header("Base Stats")]
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;

    [Header("Combat Settings")]
    public float attackDamage = 10f;

    [Header("HitBox")]
    [SerializeField] 
    protected HitBox hitBox;

    protected float lastAttackTime;

    [Header("Death & Loot")]
    public float destroyDelay = 3.0f;

    [Header("Detection Setup")]
    public LayerMask obstacleLayer;
    protected Transform player;
    protected Animator anim;

    protected enum MonsterState
    {
        Idle,
        Attack,
        Dead,
        Alert,
        Trace,
        Return,
    }
    protected MonsterState currentState = MonsterState.Idle;

    protected MonsterState CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            if (currentState == value)
            {
                return;
            }

            var prevStatus = currentState;
            currentState = value;

            Debug.Log($"[State Change] {prevStatus} -> {currentState}");

            switch (currentState)
            {
                case MonsterState.Idle:
                    break;
                case MonsterState.Attack:
                    break;
                case MonsterState.Dead:
                    PlayDeathAnim();
                    break;
                case MonsterState.Alert:
                    break;
                case MonsterState.Trace:
                    break;
                case MonsterState.Return:
                    break;
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CurrentState = MonsterState.Idle;
    }

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (IsDead || player == null)
        {
            return;
        }

        AIBehavior();
    }

    protected virtual void AIBehavior()
    {
        switch (CurrentState)
        {
            case MonsterState.Idle:
                UpdateIdle();
                break;
            case MonsterState.Attack:
                UpdateAttack();
                break;
            case MonsterState.Dead:
                UpdateDead();
                break;
            case MonsterState.Alert:
                UpdateAlert();
                break;
            case MonsterState.Trace:
                UpdateTrace();
                break;
            case MonsterState.Return:
                UpdateReturn();
                break;
        }
    }
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateAttack() { }
    protected virtual void UpdateDead() { }
    protected virtual void UpdateAlert() { }
    protected virtual void UpdateTrace() { }
    protected virtual void UpdateReturn() { }

    protected abstract void PlayIdleAnim();
    protected abstract void PlayMoveAnim();
    protected abstract void PlayDeathAnim();
    protected abstract IEnumerator AttackRoutine();

    protected IEnumerator AttackProcess()
    {
        currentState = MonsterState.Attack;

        yield return StartCoroutine(AttackRoutine());

        currentState = MonsterState.Idle;
        lastAttackTime = Time.time;
    }

    protected void UpdateDamage()
    {
        if (hitBox == null)
        {
            return;
        }

        Collider[] targets = hitBox.Colliders.ToArray();

        foreach (Collider target in targets)
        {
            if (target.CompareTag("Player"))
            {
                var playerStatus = target.GetComponent<LivingEntity>();

                if (playerStatus != null)
                {
                    playerStatus.OnDamage(attackDamage, target.ClosestPoint(transform.position), transform.forward);
                    Debug.Log($"{playerStatus.Health}");
                }
            }
        }
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead)
        {
            return;
        }

        base.OnDamage(damage, hitPoint, hitNormal);
    }

    public override void Die()
    {
        base.Die();

        currentState = MonsterState.Dead;

        PlayDeathAnim();

        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }

        DropLoot();

        Destroy(gameObject, destroyDelay);
    }

    private void DropLoot()
    {
        // 전리품 로직
    }

    protected bool HasObstacle(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, targetPos);

        return !Physics.Raycast(transform.position, dir, dist, obstacleLayer);
    }

    protected void Moving(Vector3 targetPos)
    {
        FaceTarget(targetPos);
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        PlayMoveAnim();
    }

    protected void FaceTarget(Vector3 targetPos)
    {
        Vector3 lookPos = targetPos;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }
}