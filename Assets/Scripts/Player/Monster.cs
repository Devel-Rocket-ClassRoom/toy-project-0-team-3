using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Monster : LivingEntity
{
    private enum State { Idle, Move, Attack, Dead }

    [Header("감지")]
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _realAttackRange = 5f;

    [Header("공격")]
    [SerializeField] private float _attackDamage = 10f;

    [Header("넉백")]
    [SerializeField] private float _knockbackForce = 5f;

    private State _currentState = State.Idle;
    private bool _isKnockback = false;
    private bool _isAttacking = false;

    private Transform _player;
    private NavMeshAgent _agent;
    private Animator _animator;
    private Rigidbody _rigidbody;

    private Coroutine _knockbackCoroutine;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _player = GameObject.FindWithTag("Player")?.transform;
        ChangeState(State.Idle);
    }

    protected override void Update()
    {
        base.Update();

        if (_isKnockback) return;

        switch (_currentState)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Move: UpdateMove(); break;
            case State.Attack: UpdateAttack(); break;
        }
    }

    private void ChangeState(State newState)
    {
        _currentState = newState;

        switch (newState)
        {
            case State.Idle:
                _agent.isStopped = true;
                break;
            case State.Move:
                _agent.isStopped = false;
                break;
            case State.Attack:
                _agent.isStopped = true;
                break;
            case State.Dead:
                _agent.isStopped = true;
                StartCoroutine(DestroyAfterDead());
                break;
        }
    }

    private void UpdateIdle()
    {
        if (_player == null) return;

        if (GetDistToPlayer() <= _detectRange)
        {
            _animator.SetTrigger("IdleToMove");
            ChangeState(State.Move);
        }
    }

    private void UpdateMove()
    {
        if (_player == null) return;

        float dist = GetDistToPlayer();

        if (dist <= _attackRange)
        {
            _animator.SetTrigger("MoveToAttack");
            ChangeState(State.Attack);
            return;
        }

        if (dist > _detectRange)
        {
            _animator.SetTrigger("MoveToIdle");
            ChangeState(State.Idle);
            return;
        }

        Vector3 direction = (_player.position - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        _agent.SetDestination(_player.position);
    }

    private void UpdateAttack()
    {
        if (_player == null) return;

        float dist = GetDistToPlayer();
        float currentRange = _isAttacking ? _realAttackRange : _attackRange;
        //Debug.Log($"[Monster] Attack 상태 - 거리: {dist:F1}, 공격범위: {_attackRange}");

        if (dist > currentRange)
        {
            //Debug.Log($"[Monster] AttackToMove 트리거 호출");
            _animator.SetTrigger("AttackToMove");
            ChangeState(State.Move);
            return;
        }

        Vector3 direction = (_player.position - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    public void OnAttackHit()
    {
        if (_player == null) return;

        // 이벤트 호출 시점에 범위 안에 있을 때만 데미지
        if (GetDistToPlayer() <= _realAttackRange)
        {
            if (_player.TryGetComponent<LivingEntity>(out var target))
            {
                Vector3 hitNormal = (_player.position - transform.position).normalized;
                target.OnDamage(_attackDamage, _player.position, hitNormal);
            }
        }
    }

    // 애니메이션 이벤트 - 공격 시작 시점
    public void OnAttackStart()
    {
        _isAttacking = true;
    }

    // 애니메이션 이벤트 - 공격 끝 시점
    public void OnAttackEnd()
    {
        _isAttacking = false;
    }

    public void OnHitEnd()
    {
        //Debug.Log("[Monster] OnHitEnd 호출 - 움직임 재개");
        _isKnockback = false;
        ChangeState(State.Idle);
    }

    private float GetDistToPlayer()
    {
        return Vector3.Distance(transform.position, _player.position);
    }

    private IEnumerator DestroyAfterDead()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.OnDamage(damage, hitPoint, hitNormal);

        if (!IsDead)
        {
            Debug.Log("데미지 받음");
            if (_knockbackCoroutine != null)
                StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = StartCoroutine(KnockbackCoroutine(hitNormal));
        }
    }

    private IEnumerator KnockbackCoroutine(Vector3 hitNormal)
    {
        _isKnockback = true;
        _agent.isStopped = true;
        _agent.ResetPath();

        _animator.ResetTrigger("MoveToAttack");
        _animator.SetTrigger("Damaged");

        float elapsed = 0f;
        float duration = 0.3f;
        float speed = _knockbackForce;

        while (elapsed < duration)
        {
            _rigidbody.MovePosition(_rigidbody.position + hitNormal * speed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _knockbackCoroutine = null;
    }

    public override void Die()
    {
        base.Die();
        _animator.SetTrigger("Dead");
        ChangeState(State.Dead);
    }
}