using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Monster : LivingEntity
{
    private enum State { Idle, Move, Attack, Dead }

    [Header("감지")]
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private float _attackRange = 2f;

    [Header("공격")]
    [SerializeField] private float _attackDamage = 10f;

    [Header("넉백")]
    [SerializeField] private float _knockbackForce = 5f;

    private State _currentState = State.Idle;
    private bool _isKnockback = false;

    private Transform _player;
    private NavMeshAgent _agent;
    private Animator _animator;
    private Rigidbody _rigidbody;

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

        _agent.SetDestination(_player.position);
    }

    private void UpdateAttack()
    {
        if (_player == null) return;

        float dist = GetDistToPlayer();

        if (dist > _attackRange)
        {
            _animator.SetTrigger("AttackToMove");
            ChangeState(State.Move);
            return;
        }
    }

    public void OnAttackHit()
    {
        if (_player == null) return;

        // 이벤트 호출 시점에 범위 안에 있을 때만 데미지
        if (GetDistToPlayer() <= _attackRange)
        {
            if (_player.TryGetComponent<LivingEntity>(out var target))
            {
                Vector3 hitNormal = (_player.position - transform.position).normalized;
                target.OnDamage(_attackDamage, _player.position, hitNormal);
            }
        }
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
            StartCoroutine(KnockbackCoroutine(hitNormal));
        }
    }

    private IEnumerator KnockbackCoroutine(Vector3 hitNormal)
    {
        _isKnockback = true;
        _agent.ResetPath();
        _agent.enabled = false;

        float elapsed = 0f;
        float duration = 0.3f;
        float speed = _knockbackForce;

        while (elapsed < duration)
        {
            _rigidbody.MovePosition(_rigidbody.position + hitNormal * speed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _agent.enabled = true;
        _isKnockback = false;

        if (_currentState == State.Move)
            _agent.isStopped = false;
    }

    public override void Die()
    {
        base.Die();
        _animator.SetTrigger("Dead");
        ChangeState(State.Dead);
    }
}