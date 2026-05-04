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
    protected bool isAttacking = false;

    [Header("Death & Loot")]
    public float destroyDelay = 3.0f;

    [Header("Detection Setup")]
    public LayerMask obstacleLayer;
    protected Transform player;
    protected Animator anim;

    protected enum MonsterState
    {
        Idle,
        Move,
        Attack,
        Dead,
        Alert,
        Return,
        Stun,
    }
    protected MonsterState currentState = MonsterState.Idle;

    protected override void OnEnable()
    {
        base.OnEnable();
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

        if (currentState == MonsterState.Stun)
        {
            return;
        }

        if (isAttacking)
        {
            return;
        }

        AIBehavior();
    }

    protected abstract void AIBehavior();
    protected abstract void PlayIdleAnim();
    protected abstract void PlayMoveAnim();
    protected abstract void PlayDeathAnim();
    protected abstract IEnumerator AttackRoutine();

    protected IEnumerator AttackProcess()
    {
        isAttacking = true;
        currentState = MonsterState.Attack;

        yield return StartCoroutine(AttackRoutine());

        isAttacking = false;
        currentState = MonsterState.Idle;
        lastAttackTime = Time.time;
    }

    protected void UpdateAttack()
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

    public virtual void ApplyStun(float duration)
    {
        if (IsDead)
        {
            return;
        }

        StopAllCoroutines();

        isAttacking = false;

        ResetBehavior();

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
        }

        if (anim != null)
        {
            anim.SetFloat("locomotion", 0f);
            anim.SetTrigger("gotHit");
        }

        currentState = MonsterState.Stun;
        StartCoroutine(StunRoutine(duration));
    }

    protected abstract void ResetBehavior();

    protected IEnumerator StunRoutine(float duration)
    {
        Debug.Log($"기절");

        yield return new WaitForSeconds(duration);

        if (!IsDead)
        {
            Debug.Log("기절에서 회복");
            currentState = MonsterState.Idle;
            PlayIdleAnim();
        }
    }
}