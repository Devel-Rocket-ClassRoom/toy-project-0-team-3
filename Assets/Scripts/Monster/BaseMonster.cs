using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public abstract class BaseMonster : LivingEntity
{
    [Header("Base Stats")]
    [SerializeField]
    protected float moveSpeed = 3f;
    [SerializeField]
    protected float turnSpeed = 720f;
    [SerializeField]
    protected float attackRange = 1.5f;
    [SerializeField]
    protected float attackCooldown = 1.0f;


    [Header("Combat")]
    [SerializeField]
    protected float attackDamage = 10f;
    protected float currentAttackDamage;
    [SerializeField]
    protected HitBox hitBox;
    [SerializeField]
    protected bool allowMultipleDamageEventsPerAttack = false;

    [Header("Target")]
    [SerializeField]
    protected string playerTag = "Player";
    [SerializeField]
    protected LayerMask obstacleLayer;
    [SerializeField]
    protected float eyeHeight = 1.5f;
    [SerializeField]
    protected float targetEyeHeight = 1.0f;

    [Header("Alert Mark")]
    [SerializeField]
    protected GameObject alertMarkPrefab;
    [SerializeField]
    protected Vector3 alertMarkLocalOffset = new Vector3(0f, 2.2f, 0f);

    [Header("Animation Event Safety")]
    [SerializeField]
    protected float attackFallbackDuration = 2.5f;
    [SerializeField]
    protected float hitReactionFallbackDuration = 0.6f;

    [Header("Return")]
    [SerializeField] protected float returnArriveDistance = 0.35f;

    [Header("Death")]
    [SerializeField] protected float destroyDelay = 3f;

    protected enum MonsterState
    {
        Idle,
        Trace,
        Attack,
        Alert,
        Return,
        Dead,
    }

    [SerializeField] private MonsterState currentState = MonsterState.Idle;

    protected MonsterState CurrentState
    {
        get => currentState;
        set => ChangeState(value);
    }

    [Header("NavMesh")]
    [SerializeField]
    protected float destinationSampleRadius = 2f;
    protected NavMeshAgent agent;

    protected Animator anim;
    protected Transform player;

    protected Vector3 spawnPosition;
    protected Quaternion spawnRotation;

    protected bool isAttacking;
    protected bool isHitReacting;

    private bool hasAppliedDamageThisAttack;
    private float attackStartedTime;
    private float lastAttackEndTime;

    private GameObject alertMarkInstance;
    private Coroutine hitReactionCoroutine;
    private Collider bodyCollider;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider>();

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        FindPlayer();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        isAttacking = false;
        isHitReacting = false;
        hasAppliedDamageThisAttack = false;
        lastAttackEndTime = -attackCooldown;

        //spawnPosition = transform.position;
        //spawnRotation = transform.rotation;

        currentAttackDamage = attackDamage;

        if (bodyCollider != null)
        {
            bodyCollider.enabled = true;
        }

        if (hitBox != null)
        {
            hitBox.Colliders.Clear();
        }

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.updateRotation = true;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        ClearAlertMark();
        ChangeState(MonsterState.Idle, true);
    }

    protected override void Update()
    {
        base.Update();

        if (IsDead)
        {
            return;
        }

        if (player == null)
        {
            FindPlayer();
        }

        if (isHitReacting)
        {
            return;
        }

        switch (currentState)
        {
            case MonsterState.Idle:
                UpdateIdle();
                break;

            case MonsterState.Alert:
                UpdateAlert();
                break;

            case MonsterState.Trace:
                UpdateTrace();
                break;

            case MonsterState.Attack:
                UpdateAttackState();
                break;

            case MonsterState.Return:
                UpdateReturn();
                break;

            case MonsterState.Dead:
                break;
        }
    }

    private void ChangeState(MonsterState nextState, bool force = false)
    {
        if (!force && currentState == nextState)
        {
            return;
        }

        if (!force && currentState == MonsterState.Dead)
        {
            return;
        }

        ExitState(currentState);

        currentState = nextState;
        Debug.Log($"[{gameObject.name}] State -> {currentState}");

        EnterState(currentState);
    }

    private void EnterState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Idle:
                StopMoving();
                ClearAlertMark();
                PlayIdleAnim();
                break;

            case MonsterState.Alert:
                StopMoving();
                ShowAlertMark();
                PlayIdleAnim();
                break;

            case MonsterState.Trace:
                ClearAlertMark();
                ResumeMoving();
                break;

            case MonsterState.Attack:
                ClearAlertMark();
                StopMoving();
                break;

            case MonsterState.Return:
                ClearAlertMark();
                ResumeMoving();
                break;

            case MonsterState.Dead:
                StopMoving();
                ClearAlertMark();

                if (bodyCollider != null)
                {
                    bodyCollider.enabled = false;
                }

                if (hitBox != null)
                {
                    hitBox.Colliders.Clear();
                    hitBox.gameObject.SetActive(false);
                }

                PlayDeathAnim();
                DropLoot();
                Destroy(gameObject, destroyDelay);
                break;
        }
    }

    private void ExitState(MonsterState state)
    {
        if (state == MonsterState.Alert)
        {
            ClearAlertMark();
        }
    }

    protected abstract void UpdateIdle();
    protected abstract void UpdateAlert();
    protected abstract void UpdateTrace();

    protected virtual void UpdateAttackState()
    {
        if (player == null)
        {
            CurrentState = MonsterState.Return;
            return;
        }

        FaceTarget(player.position);

        if (isAttacking)
        {
            if (attackFallbackDuration > 0f &&
                Time.time >= attackStartedTime + attackFallbackDuration)
            {
                Animation_AttackEnd();
            }

            return;
        }

        float dist = GetDistanceToPlayer();

        if (dist > GetChaseRange())
        {
            CurrentState = MonsterState.Return;
            return;
        }

        if (dist > attackRange)
        {
            CurrentState = MonsterState.Trace;
            return;
        }

        if (Time.time >= lastAttackEndTime + attackCooldown)
        {
            StartAttack();
        }
        else
        {
            StopMoving();
            PlayIdleAnim();
        }
    }

    protected virtual void UpdateReturn()
    {
        MoveTo(spawnPosition);

        if (HasReached(spawnPosition, returnArriveDistance))
        {
            StopMoving();
            transform.rotation = spawnRotation;
            CurrentState = MonsterState.Idle;
        }
    }

    protected virtual float GetChaseRange()
    {
        return attackRange * 2f;
    }

    private void StartAttack()
    {
        isAttacking = true;
        hasAppliedDamageThisAttack = false;
        attackStartedTime = Time.time;

        currentAttackDamage = attackDamage;

        StopMoving();

        if (player != null)
        {
            FaceTarget(player.position, true);
        }

        PlayAttackAnim();
    }

    public void Animation_AttackHit()
    {
        if (IsDead)
        {
            return;
        }

        if (CurrentState != MonsterState.Attack)
        {
            return;
        }

        if (!isAttacking)
        {
            return;
        }

        if (!allowMultipleDamageEventsPerAttack && hasAppliedDamageThisAttack)
        {
            return;
        }

        Debug.Log("AttackHit");

        hasAppliedDamageThisAttack = true;
        ApplyAttackDamage();
    }

    public void Animation_AttackEnd()
    {
        if (!isAttacking)
        {
            return;
        }

        isAttacking = false;
        lastAttackEndTime = Time.time;
    }

    protected void SetCurrentAttackDamage(float damage)
    {
        currentAttackDamage = Mathf.Max(0f, damage);
    }

    protected virtual void ApplyAttackDamage()
    {
        if (hitBox == null)
        {
            return;
        }

        Collider[] targets = hitBox.Colliders.ToArray();

        foreach (Collider target in targets)
        {
            if (target == null)
            {
                continue;
            }

            LivingEntity targetEntity = target.GetComponent<LivingEntity>();

            if (targetEntity == null)
            {
                continue;
            }

            if (targetEntity == this)
            {
                continue;
            }

            if (!targetEntity.CompareTag(playerTag))
            {
                continue;
            }

            Vector3 hitPoint = target.ClosestPoint(transform.position);
            Vector3 hitNormal = GetFlatDirection(transform.position, target.transform.position).normalized;

            if (hitNormal.sqrMagnitude <= 0.001f)
            {
                hitNormal = transform.forward;
            }

            targetEntity.OnDamage(currentAttackDamage, hitPoint, hitNormal);

            Debug.Log(targetEntity.Health);
        }
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead)
        {
            return;
        }

        base.OnDamage(damage, hitPoint, hitNormal);

        if (IsDead)
        {
            return;
        }

        HandleDamageAggro();

        if (!isAttacking)
        {
            StartHitReaction();
        }
    }

    protected virtual void HandleDamageAggro()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
        {
            return;
        }

        ClearAlertMark();

        float dist = GetDistanceToPlayer();

        if (dist <= attackRange)
        {
            CurrentState = MonsterState.Attack;
        }
        else
        {
            CurrentState = MonsterState.Trace;
        }
    }

    private void StartHitReaction()
    {
        if (hitReactionCoroutine != null)
        {
            StopCoroutine(hitReactionCoroutine);
        }

        isHitReacting = true;
        StopMoving();
        PlayHitAnim();

        if (hitReactionFallbackDuration > 0f)
        {
            hitReactionCoroutine = StartCoroutine(HitReactionFallback());
        }
    }

    private IEnumerator HitReactionFallback()
    {
        yield return new WaitForSeconds(hitReactionFallbackDuration);
        Animation_HitReactEnd();
    }

    public void Animation_HitReactEnd()
    {
        isHitReacting = false;

        if (hitReactionCoroutine != null)
        {
            StopCoroutine(hitReactionCoroutine);
            hitReactionCoroutine = null;
        }
    }

    public override void Die()
    {
        if (IsDead)
        {
            return;
        }

        base.Die();
        ChangeState(MonsterState.Dead, true);
    }

    protected virtual void DropLoot()
    {
        // TODO:
        // 고기, 재료, 골드 등 드롭 로직 연결 위치.
    }

    protected void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    protected float GetDistanceToPlayer()
    {
        if (player == null)
        {
            return float.PositiveInfinity;
        }

        return GetFlatDistance(transform.position, player.position);
    }

    protected bool IsPlayerInsideRange(float range, bool requireLineOfSight)
    {
        if (player == null)
        {
            return false;
        }

        if (GetDistanceToPlayer() > range)
        {
            return false;
        }

        if (requireLineOfSight && !HasLineOfSight(player.position))
        {
            return false;
        }

        return true;
    }

    protected bool IsPlayerInsideView(float range, float viewAngle)
    {
        //Debug.Log("IsPlayerInsideView");
        if (player == null)
        {
            return false;
        }

        if (GetDistanceToPlayer() > range)
        {
            return false;
        }

        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float angle = Vector3.Angle(forward.normalized, dirToPlayer.normalized);

        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        return HasLineOfSight(player.position);
    }

    protected bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 start = transform.position + Vector3.up * eyeHeight;
        Vector3 end = targetPosition + Vector3.up * targetEyeHeight;
        Vector3 dir = end - start;

        return !Physics.Raycast(
            start,
            dir.normalized,
            dir.magnitude,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    protected void MoveTo(Vector3 targetPosition)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            PlayMoveAnim();
            return;
        }

        agent.speed = moveSpeed;
        agent.isStopped = false;
        agent.SetDestination(targetPosition);

        PlayMoveAnim();
    }

    protected void StopMoving()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    protected void ResumeMoving()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    protected bool HasReached(Vector3 targetPosition, float arriveDistance)
    {
        if (GetFlatDistance(transform.position, targetPosition) <= arriveDistance)
        {
            return true;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return false;
        }

        if (agent.pathPending)
        {
            return false;
        }

        return agent.remainingDistance <= arriveDistance;
    }

    protected void FaceTarget(Vector3 targetPosition, bool instant = false)
    {
        Vector3 lookPos = targetPosition;
        lookPos.y = transform.position.y;

        Vector3 dir = lookPos - transform.position;

        if (dir.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        if (instant)
        {
            transform.rotation = targetRot;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }
    }

    protected void ShowAlertMark()
    {
        if (alertMarkPrefab == null)
        {
            return;
        }

        if (alertMarkInstance != null)
        {
            return;
        }

        alertMarkInstance = Instantiate(alertMarkPrefab, transform);
        alertMarkInstance.transform.localPosition = alertMarkLocalOffset;
        alertMarkInstance.transform.localRotation = Quaternion.identity;
    }

    protected void ClearAlertMark()
    {
        if (alertMarkInstance == null)
        {
            return;
        }

        Destroy(alertMarkInstance);
        alertMarkInstance = null;
    }

    protected abstract void PlayIdleAnim();
    protected abstract void PlayMoveAnim();
    protected abstract void PlayAttackAnim();
    protected abstract void PlayHitAnim();
    protected abstract void PlayDeathAnim();

    protected float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    protected Vector3 GetFlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        dir.y = 0f;

        return dir;
    }
}