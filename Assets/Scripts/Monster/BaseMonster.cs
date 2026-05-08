using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/**
 * @brief 일반 몬스터 AI의 공통 상태 머신, 이동, 전투, 피격, 사망 로직을 제공하는 추상 기반 클래스입니다.
 *
 * @details
 * NavMeshAgent와 Animator를 필수 컴포넌트로 요구하며, LivingEntity의 체력/데미지 처리를 확장합니다.
 * Idle, Alert, Trace, Attack, Return, Dead 상태를 공통으로 관리하고 자식 클래스가 감지 방식과 애니메이션만 재정의하도록 설계되어 있습니다.
 * 공격 데미지는 HitBox에 들어온 Collider 목록을 기준으로 적용하며, 애니메이션 이벤트가 누락되어도 fallback 시간으로 공격/피격 상태가 풀리도록 방어합니다.
 */
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public abstract class BaseMonster : LivingEntity
{
    [Header("Base Stats")]
    [SerializeField]
    protected float moveSpeed = 3f;         //NavMeshAgent가 이동할 때 사용할 기본 이동 속도입니다.
    [SerializeField]                        
    protected float turnSpeed = 720f;       //목표 방향으로 회전할 때 초당 회전 가능한 각도입니다.
    [SerializeField]                        
    protected float attackRange = 1.5f;     //플레이어를 공격 상태로 전환하거나 공격을 유지할 수 있는 거리입니다.
    [SerializeField]                        
    protected float attackCooldown = 1.0f;  //공격 종료 후 다음 공격을 시작하기 전까지 기다려야 하는 시간입니다.

    [Header("Combat")]
    [SerializeField]
    protected float baseAttackDamage = 1f;  //공격별 보정이 적용되기 전 몬스터의 기본 공격 데미지입니다.
    protected float currentAttackDamage;    //현재 재생 중인 공격이 실제로 적용할 데미지 값입니다.
    [SerializeField]
    protected HitBox hitBox; // 공격 판정에 들어온 대상 Collider를 추적하거나 데미지 판정에 사용하는 HitBox 참조입니다.

    [Header("Target")]
    [SerializeField]
    protected string playerTag = "Player"; // 플레이어 오브젝트를 찾고 공격 대상인지 확인할 때 사용하는 태그 이름입니다.
    [SerializeField]
    protected LayerMask obstacleLayer; // 시야 판정 Raycast에서 장애물로 간주할 레이어 마스크입니다.
    [SerializeField]
    protected float eyeHeight = 1.5f; // 몬스터 시야 Raycast 시작점에 더하는 눈높이 오프셋입니다.
    [SerializeField]
    protected float targetEyeHeight = 1.0f; // 대상 시야 Raycast 도착점에 더하는 높이 오프셋입니다.

    [Header("Alert Mark")]
    [SerializeField]
    protected GameObject alertMarkPrefab; // 경계, 추적, 공격 등 인지 상태를 시각적으로 표시할 GameObject 참조입니다.

    [Header("Animation Event Safety")]
    [SerializeField]
    protected float attackFallbackDuration = 2.5f; // 공격 종료 애니메이션 이벤트가 호출되지 않았을 때 강제로 공격을 끝내기 위한 최대 지속 시간입니다.
    [SerializeField]
    protected float hitReactionFallbackDuration = 0.6f; // 피격 종료 애니메이션 이벤트가 호출되지 않았을 때 강제로 피격 상태를 끝내기 위한 시간입니다.

    [Header("Return")]
    [SerializeField] 
    protected float returnArriveDistance = 0.35f; // 스폰 위치 복귀 완료로 인정할 평면 거리 기준입니다.

    [Header("Death")]
    [SerializeField] 
    protected float destroyDelay = 3f; // 사망 처리 후 GameObject를 제거하기 전까지 기다리는 시간입니다.

    /**
     * @brief BaseMonster 계열 몬스터의 공통 행동 상태를 표현하는 내부 열거형입니다.
     *
     * @details
     * Idle, Trace, Attack, Alert, Return, Dead를 통해 감지, 이동, 공격, 복귀, 사망 흐름을 하나의 상태 머신으로 관리합니다.
     */
    protected enum MonsterState
    {
        Idle,
        Trace,
        Attack,
        Alert,
        Return,
        Dead,
    }

    [SerializeField] 
    private MonsterState currentState = MonsterState.Idle; // 현재 몬스터 또는 보스가 수행 중인 상태를 저장하는 디버그/상태 머신 변수입니다.

    protected MonsterState CurrentState
    {
        get => currentState;
        set => ChangeState(value);
    }

    [Header("NavMesh")]
    [SerializeField]
    protected NavMeshAgent agent; // NavMesh 경로 이동과 정지/재개를 담당하는 NavMeshAgent 참조입니다.

    protected Animator anim; // 애니메이션 파라미터와 트리거를 제어하는 Animator 참조입니다.
    protected Transform player; // 현재 추적하거나 바라볼 플레이어 Transform 참조입니다.

    protected Vector3 spawnPosition; // 몬스터가 생성되거나 활성화된 위치로, Return 상태의 복귀 목적지입니다.
    protected Quaternion spawnRotation; // 몬스터가 생성되거나 활성화된 회전값으로, 복귀 완료 후 되돌릴 방향입니다.

    protected bool isAttacking; // 현재 공격 애니메이션/판정이 진행 중인지 나타내는 플래그입니다.
    protected bool isHitReacting; // 현재 피격 반응으로 인해 일반 상태 업데이트를 막아야 하는지 나타내는 플래그입니다.

    private float attackStartedTime; // 현재 공격을 시작한 Time.time 값으로, fallback 종료 판정에 사용됩니다.
    private float lastAttackEndTime; // 마지막 공격이 끝난 Time.time 값으로, 공격 쿨다운 계산에 사용됩니다.

    private Coroutine hitReactionCoroutine; // 피격 종료 이벤트 누락을 대비해 실행 중인 fallback 코루틴 참조입니다.
    private Collider bodyCollider; // 사망 시 비활성화할 몬스터 본체 Collider 참조입니다.

    /**
     * @brief 컴포넌트 참조와 스폰 기준값을 초기화하고 플레이어를 탐색합니다.
     *
     * @details
     * NavMeshAgent, Animator, Collider를 현재 GameObject에서 가져옵니다.
     * 현재 Transform 위치와 회전을 spawnPosition/spawnRotation에 저장하여 복귀 기준으로 사용합니다.
     * FindPlayer를 호출하여 playerTag를 가진 플레이어 Transform을 미리 확보합니다.
     */
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider>();

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        FindPlayer();
    }

    /**
     * @brief 오브젝트 활성화 시 몬스터 또는 보스의 런타임 상태를 초기값으로 재설정합니다.
     *
     * @details
     * 부모 LivingEntity의 활성화 처리를 먼저 실행합니다.
     * 공격/피격 플래그, 쿨다운 기준 시각, 현재 공격 데미지, Collider/HitBox/Agent 상태를 초기화합니다.
     * 인지 표시 오브젝트를 꺼두고 강제로 Idle 상태 진입 로직을 실행합니다.
     * 자식 클래스에서 재정의한 경우 base.OnEnable 이후 자신에게 필요한 추가 Animator/상태 초기화를 수행합니다.
     */
    protected override void OnEnable()
    {
        base.OnEnable();

        isAttacking = false;
        isHitReacting = false;
        lastAttackEndTime = -attackCooldown;

        //spawnPosition = transform.position;
        //spawnRotation = transform.rotation;

        currentAttackDamage = baseAttackDamage;

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

        alertMarkPrefab.SetActive(false);
        ChangeState(MonsterState.Idle, true);
    }

    /**
     * @brief 매 프레임 공통 상태 머신을 갱신합니다.
     *
     * @details
     * LivingEntity의 Update를 먼저 호출해 상태 이상 등 상위 갱신을 진행합니다.
     * 사망 중이면 모든 AI 처리를 중단합니다.
     * 플레이어 참조가 비어 있으면 다시 탐색합니다.
     * 피격 반응 중이면 현재 프레임의 일반 상태 갱신을 막습니다.
     * currentState에 따라 Idle, Alert, Trace, Attack, Return 상태별 갱신 함수를 호출합니다.
     */
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

    /**
     * @brief 현재 상태를 새 상태로 변경하고 해당 상태의 진입 처리를 실행합니다.
     *
     * @details
     * force가 false이고 같은 상태로 변경하려는 경우 중복 진입을 막기 위해 즉시 종료합니다.
     * force가 false이고 이미 Dead 상태인 경우 다른 상태로 되돌아가지 못하게 종료합니다.
     * currentState를 nextState로 바꾸고 디버그 로그를 출력합니다.
     * EnterState를 호출하여 이동 정지, 표시, 애니메이션, 사망 정리 등 상태별 시작 처리를 수행합니다.
     * @param nextState 전환하려는 다음 몬스터 상태입니다.
     * @param force true이면 같은 상태 또는 Dead 보호 조건을 무시하고 진입 처리를 강제로 실행합니다.
     */
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

        currentState = nextState;
        Debug.Log($"[{gameObject.name}] State -> {currentState}");

        EnterState(currentState);
    }

    /**
     * @brief 상태 진입 시 필요한 공통 부가 처리를 수행합니다.
     *
     * @details
     * Idle은 이동을 멈추고 경계 표시를 끈 뒤 대기 애니메이션을 재생합니다.
     * Alert는 이동을 멈추고 경계 표시를 켠 뒤 대기 애니메이션을 재생합니다.
     * Trace는 경계 표시를 켜고 이동을 재개합니다.
     * Attack은 경계 표시를 켜고 공격 중 위치가 밀리지 않도록 이동을 정지합니다.
     * Return은 경계 표시를 끄고 스폰 위치까지 이동할 수 있도록 이동을 재개합니다.
     * Dead는 이동/표시/Collider/HitBox를 정리한 뒤 사망 애니메이션, 드롭, 지연 파괴를 실행합니다.
     * @param state 진입 처리를 수행할 현재 상태입니다.
     */
    private void EnterState(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Idle:
                StopMoving();
                alertMarkPrefab.SetActive(false);
                PlayIdleAnim();
                break;

            case MonsterState.Alert:
                StopMoving();
                ShowAlertMark();
                PlayIdleAnim();
                break;

            case MonsterState.Trace:
                ShowAlertMark();
                ResumeMoving();
                break;

            case MonsterState.Attack:
                ShowAlertMark();
                StopMoving();
                break;

            case MonsterState.Return:
                alertMarkPrefab.SetActive(false);
                ResumeMoving();
                break;

            case MonsterState.Dead:
                StopMoving();
                alertMarkPrefab.SetActive(false);

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

    /**
     * @brief Idle 상태 갱신 로직을 자식 감지 방식에 맞게 구현하기 위한 추상 함수입니다.
     *
     * @details
     * BaseMonster는 공통 상태 호출만 담당하고, 실제 감지 조건은 SimpleMonster 또는 SensoryMonster가 구현합니다.
     */
    protected abstract void UpdateIdle();
    /**
     * @brief Alert 상태 갱신 로직을 자식 감지 방식에 맞게 구현하기 위한 추상 함수입니다.
     *
     * @details
     * 경계 중 플레이어 추적 전환, Idle 복귀, 위치 이동 등은 감지 방식별로 달라집니다.
     */
    protected abstract void UpdateAlert();
    /**
     * @brief Trace 상태 갱신 로직을 자식 감지 방식에 맞게 구현하기 위한 추상 함수입니다.
     *
     * @details
     * 추적 유지 거리와 공격 전환 조건은 자식 클래스가 정의합니다.
     */
    protected abstract void UpdateTrace();

    /**
     * @brief 공격 상태에서 방향 조정, 공격 유지, 거리 이탈, 쿨다운, 공격 시작을 관리합니다.
     *
     * @details
     * 플레이어가 없으면 Return 상태로 전환합니다.
     * 플레이어를 향해 회전합니다.
     * 이미 공격 중이면 fallback 시간이 지났는지 확인하고, 지났다면 Animation_AttackEnd를 강제 호출합니다.
     * 공격 중이 아니라면 플레이어와의 거리로 추적 유지 가능 범위, 공격 범위 이탈 여부를 판단합니다.
     * 공격 범위 안이고 쿨다운이 끝났으면 StartAttack을 실행하고, 아직 쿨다운 중이면 정지 상태로 Idle 애니메이션을 유지합니다.
     */
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

        float dist = GetDistanceToPlayer(); // 현재 몬스터 또는 보스와 플레이어 사이의 평면 거리를 저장하는 지역 변수입니다.

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

    /**
     * @brief 스폰 위치로 복귀하고 도착하면 원래 회전으로 되돌린 뒤 Idle 상태로 전환합니다.
     *
     * @details
     * MoveTo를 통해 spawnPosition을 목적지로 설정합니다.
     * HasReached로 복귀 완료 거리를 검사합니다.
     * 도착하면 이동을 정지하고 생성 시점의 spawnRotation으로 회전을 복구한 뒤 Idle 상태로 전환합니다.
     */
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

    /**
     * @brief 기본 추적 유지 거리를 반환합니다.
     *
     * @details
     * 자식 클래스가 chaseRange를 따로 가진 경우 이 함수를 override하여 다른 값을 반환합니다.
     * BaseMonster 기본값은 attackRange의 2배입니다.
     * @return 추적 상태를 유지할 수 있는 최대 거리입니다.
     */
    protected virtual float GetChaseRange()
    {
        return attackRange * 2f;
    }

    /**
     * @brief 공격 시작 플래그와 공격 데미지를 초기화하고 공격 애니메이션을 재생합니다.
     *
     * @details
     * isAttacking을 true로 설정하고 attackStartedTime에 현재 시간을 저장합니다.
     * currentAttackDamage를 baseAttackDamage로 되돌려 공격별 보정 전 기본값을 준비합니다.
     * 이동을 정지하고 플레이어가 있으면 즉시 플레이어 방향으로 회전합니다.
     * 자식 클래스가 구현한 PlayAttackAnim을 호출하여 실제 애니메이션 트리거와 공격별 데미지를 설정합니다.
     */
    private void StartAttack()
    {
        isAttacking = true;
        attackStartedTime = Time.time;

        currentAttackDamage = baseAttackDamage;

        StopMoving();

        if (player != null)
        {
            FaceTarget(player.position, true);
        }

        PlayAttackAnim();
    }

    /**
    * @brief 공격 애니메이션 이벤트에서 호출되어 현재 공격 데미지를 적용합니다.
    *
    * @details
    * 사망 상태, Attack 상태가 아닌 경우, isAttacking이 false인 경우에는 잘못된 이벤트로 보고 종료합니다.
    * 유효한 공격 이벤트이면 로그를 출력하고 ApplyAttackDamage를 호출합니다.
    * 실제 대상 필터링과 OnDamage 호출은 ApplyAttackDamage에서 수행합니다.
    */
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

        Debug.Log("AttackHit");

        ApplyAttackDamage();
    }

    /**
     * @brief 공격 애니메이션 이벤트 또는 fallback에서 호출되어 공격 상태를 종료합니다.
     *
     * @details
     * 이미 공격 중이 아니면 중복 종료를 막고 반환합니다.
     * isAttacking을 false로 바꾸고 lastAttackEndTime을 현재 시간으로 갱신하여 다음 쿨다운 계산에 사용합니다.
     */
    public void Animation_AttackEnd()
    {
        if (!isAttacking)
        {
            return;
        }

        isAttacking = false;
        lastAttackEndTime = Time.time;
    }

    /**
     * @brief 현재 공격 데미지를 공격별 배율로 설정합니다.
     *
     * @details
     * 전달받은 damage의 절댓값을 baseAttackDamage에 곱해 currentAttackDamage에 저장합니다.
     * 현재 코드 기준으로 damage는 절대 데미지가 아니라 baseAttackDamage에 대한 배율처럼 동작합니다.
     * @param damage 현재 공격에 적용할 데미지 배율로 사용되는 값입니다.
     */
    protected void SetCurrentAttackDamage(float damage)
    {
        currentAttackDamage = baseAttackDamage * Mathf.Abs(damage);
    }

    /**
     * @brief HitBox 안에 들어온 유효한 플레이어 대상에게 현재 공격 데미지를 적용합니다.
     *
     * @details
     * hitBox가 없으면 공격 판정 대상이 없으므로 종료합니다.
     * HitBox의 Collider 목록을 배열로 복사하여 순회 중 목록 변경 영향을 줄입니다.
     * 각 Collider가 null인지, LivingEntity를 가지고 있는지, 자기 자신인지, 플레이어 태그인지 순서대로 필터링합니다.
     * 대상의 가장 가까운 충돌 지점과 수평 방향 hitNormal을 계산합니다.
     * 방향 벡터가 너무 작으면 몬스터의 forward를 대체 normal로 사용합니다.
     * targetEntity.OnDamage를 호출하고 대상의 남은 Health를 디버그 출력합니다.
     */
    protected virtual void ApplyAttackDamage()
    {
        if (hitBox == null)
        {
            return;
        }

        Collider[] targets = hitBox.Colliders.ToArray(); // HitBox Collider 목록을 안전하게 순회하기 위해 배열로 복사한 지역 변수입니다.

        foreach (Collider target in targets)
        {
            if (target == null)
            {
                continue;
            }

            LivingEntity targetEntity = target.GetComponent<LivingEntity>(); // 공격 대상 Collider에서 가져온 LivingEntity 컴포넌트 참조 지역 변수입니다.

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

            Vector3 hitPoint = target.ClosestPoint(transform.position); // 피격 위치 정보를 전달하는 매개변수입니다.
            Vector3 hitNormal = GetFlatDirection(transform.position, target.transform.position).normalized; // 피격 방향 정보를 전달하는 매개변수입니다.

            if (hitNormal.sqrMagnitude <= 0.001f)
            {
                hitNormal = transform.forward;
            }

            targetEntity.OnDamage(currentAttackDamage, hitPoint, hitNormal);

            Debug.Log(targetEntity.Health);
        }
    }

    /**
     * @brief 몬스터가 피해를 받았을 때 어그로 전환과 피격 반응을 처리합니다.
     *
     * @details
     * 이미 사망했다면 피해 처리를 중단합니다.
     * base.OnDamage로 실제 체력 감소와 사망 판정을 수행합니다.
     * 피해 처리 후 사망했다면 추가 행동 전환 없이 종료합니다.
     * HandleDamageAggro를 통해 플레이어를 추적 또는 공격 대상으로 설정합니다.
     * 공격 중이 아니라면 StartHitReaction을 호출해 피격 애니메이션을 재생합니다.
     * @param damage 받은 피해량입니다.
     * @param hitPoint 피격 위치입니다.
     * @param hitNormal 피격 방향 또는 표면 법선 방향입니다.
     */
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

        Debug.Log(Health);

        if (!isAttacking)
        {
            StartHitReaction();
        }
    }

    /**
     * @brief 피해를 받은 뒤 플레이어를 기준으로 공격 또는 추적 상태로 전환합니다.
     *
     * @details
     * 플레이어 참조가 없으면 다시 탐색합니다.
     * 그래도 플레이어가 없으면 상태 전환을 할 수 없으므로 종료합니다.
     * 경계 표시를 끄고 플레이어와의 거리를 계산합니다.
     * 공격 범위 안이면 Attack 상태로, 밖이면 Trace 상태로 전환합니다.
     */
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

        alertMarkPrefab.SetActive(false);

        float dist = GetDistanceToPlayer(); // 현재 몬스터 또는 보스와 플레이어 사이의 평면 거리를 저장하는 지역 변수입니다.

        if (dist <= attackRange)
        {
            CurrentState = MonsterState.Attack;
        }
        else
        {
            CurrentState = MonsterState.Trace;
        }
    }

    /**
     * @brief 피격 반응 애니메이션을 시작하고 fallback 코루틴을 예약합니다.
     *
     * @details
     * 이미 실행 중인 피격 fallback 코루틴이 있으면 중지합니다.
     * isHitReacting을 true로 설정하여 일반 Update 상태 갱신을 막습니다.
     * 이동을 멈추고 자식 클래스의 PlayHitAnim을 호출합니다.
     * fallback 시간이 0보다 크면 HitReactionFallback 코루틴을 시작합니다.
     */
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

    /**
     * @brief 피격 종료 이벤트가 누락되었을 때 일정 시간 후 강제로 피격 반응을 끝냅니다.
     *
     * @details
     * hitReactionFallbackDuration만큼 대기합니다.
     * Animation_HitReactEnd를 호출하여 isHitReacting을 false로 되돌립니다.
     * @return Unity 코루틴 실행을 위한 IEnumerator입니다.
     */
    private IEnumerator HitReactionFallback()
    {
        yield return new WaitForSeconds(hitReactionFallbackDuration);
        Animation_HitReactEnd();
    }

    /**
     * @brief 피격 애니메이션 이벤트 또는 fallback에서 호출되어 피격 반응을 종료합니다.
     *
     * @details
     * isHitReacting을 false로 변경하여 다음 Update부터 상태 머신이 다시 동작할 수 있게 합니다.
     * fallback 코루틴이 남아 있으면 중지하고 참조를 null로 정리합니다.
     */
    public void Animation_HitReactEnd()
    {
        isHitReacting = false;

        if (hitReactionCoroutine != null)
        {
            StopCoroutine(hitReactionCoroutine);
            hitReactionCoroutine = null;
        }
    }

    /**
     * @brief 사망 상태로 전환하여 사망 처리 흐름을 시작합니다.
     *
     * @details
     * 이미 사망했다면 중복 사망 처리를 방지합니다.
     * base.Die로 LivingEntity의 사망 플래그를 설정합니다.
     * Dead 상태로 강제 전환하여 Collider 비활성화, HitBox 정리, 사망 애니메이션, 드롭, Destroy를 실행합니다.
     */
    public override void Die()
    {
        if (IsDead)
        {
            return;
        }

        base.Die();
        ChangeState(MonsterState.Dead, true);
    }

    /**
     * @brief 사망 시 아이템이나 골드를 드롭하기 위한 확장 지점입니다.
     *
     * @details
     * 현재 구현은 비어 있으며, 고기/재료/골드 드롭 로직을 연결할 위치로 남겨져 있습니다.
     */
    protected virtual void DropLoot()
    {
        // TODO:
        // 고기, 재료, 골드 등 드롭 로직 연결 위치.
    }

    /**
    * @brief playerTag를 가진 GameObject를 찾아 player Transform을 저장합니다.
    *
    * @details
    * GameObject.FindGameObjectWithTag를 사용해 플레이어 오브젝트를 검색합니다.
    * 오브젝트를 찾은 경우 transform을 player 필드에 저장합니다.
    */
    protected void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag); // playerTag로 검색된 플레이어 GameObject를 임시로 저장하는 지역 변수입니다.

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    /**
    * @brief 현재 몬스터와 플레이어 사이의 수평 거리를 반환합니다.
    *
    * @details
    * player가 없으면 추적/공격 조건이 성립하지 않도록 양의 무한대를 반환합니다.
    * player가 있으면 GetFlatDistance를 통해 y축을 제거한 평면 거리만 계산합니다.
    * @return 플레이어까지의 수평 거리 또는 플레이어가 없을 때 PositiveInfinity입니다.
    */
    protected float GetDistanceToPlayer()
    {
        if (player == null)
        {
            return float.PositiveInfinity;
        }

        return GetFlatDistance(transform.position, player.position);
    }

    /**
     * @brief 플레이어가 지정 거리 안에 있고 필요 시 시야선까지 확보되는지 검사합니다.
     *
     * @details
     * 플레이어가 없으면 false를 반환합니다.
     * 수평 거리가 range보다 크면 false를 반환합니다.
     * requireLineOfSight가 true이고 HasLineOfSight가 실패하면 false를 반환합니다.
     * 모든 조건을 통과하면 true를 반환합니다.
     * @param range 검사할 감지 거리입니다.
     * @param requireLineOfSight true이면 장애물 시야 차단까지 검사합니다.
     * @return 감지 조건을 만족하면 true, 아니면 false입니다.
     */
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

    /**
     * @brief 플레이어가 지정 시야 거리와 시야각 안에 있으며 장애물에 가려지지 않았는지 검사합니다.
     *
     * @details
     * 플레이어가 없거나 range 밖이면 false를 반환합니다.
     * 플레이어 방향 벡터의 y축을 제거해 수평 시야만 판단합니다.
     * 플레이어가 거의 같은 위치라면 각도 계산 없이 true를 반환합니다.
     * 몬스터 forward와 플레이어 방향의 각도가 viewAngle의 절반보다 크면 false를 반환합니다.
     * 각도 조건을 통과하면 HasLineOfSight 결과를 최종 반환합니다.
     * @param range 시야 판정 거리입니다.
     * @param viewAngle 전체 시야각입니다.
     * @return 플레이어가 시야 안에 있으면 true입니다.
     */
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

        Vector3 dirToPlayer = player.position - transform.position; // 몬스터에서 플레이어로 향하는 수평 방향 벡터 지역 변수입니다.
        dirToPlayer.y = 0f;

        if (dirToPlayer.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        Vector3 forward = transform.forward; // 현재 오브젝트의 정면 방향을 수평 판정용으로 저장한 지역 변수입니다.
        forward.y = 0f;

        float angle = Vector3.Angle(forward.normalized, dirToPlayer.normalized); // 정면 방향과 대상 방향 사이의 각도를 저장하는 지역 변수입니다.

        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        return HasLineOfSight(player.position);
    }

    /**
     * @brief 몬스터 눈높이에서 대상 눈높이까지 Raycast하여 장애물이 없는지 검사합니다.
     *
     * @details
     * start는 몬스터 위치에 eyeHeight를 더한 지점입니다.
     * end는 대상 위치에 targetEyeHeight를 더한 지점입니다.
     * start에서 end 방향으로 obstacleLayer Raycast를 쏘고, 맞은 장애물이 없으면 시야가 열린 것으로 판단합니다.
     * @param targetPosition 시야 확인 대상의 월드 위치입니다.
     * @return 장애물이 없으면 true, 장애물이 있으면 false입니다.
     */
    protected bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 start = transform.position + Vector3.up * eyeHeight; // 시야 Raycast 시작 위치를 저장하는 지역 변수입니다.
        Vector3 end = targetPosition + Vector3.up * targetEyeHeight; // 시야 Raycast 도착 위치를 저장하는 지역 변수입니다.
        Vector3 dir = end - start; // 두 위치 사이의 수평 또는 일반 방향 벡터를 저장하는 지역 변수입니다.

        return !Physics.Raycast(
            start,
            dir.normalized,
            dir.magnitude,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    /**
     * @brief NavMeshAgent 목적지를 설정하고 이동 애니메이션을 재생합니다.
     *
     * @details
     * Agent가 없거나 비활성화되었거나 NavMesh 위에 없으면 경로 이동 없이 이동 애니메이션만 재생하고 종료합니다.
     * Agent 속도를 moveSpeed로 맞추고 정지 상태를 해제합니다.
     * SetDestination으로 목적지를 설정하고 PlayMoveAnim을 호출합니다.
     * @param targetPosition 이동할 월드 좌표입니다.
     */
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

    /**
     * @brief NavMeshAgent 이동을 정지하고 현재 경로를 초기화합니다.
     *
     * @details
     * Agent가 존재하고 활성화되어 있으며 NavMesh 위에 있을 때만 처리합니다.
     * isStopped를 true로 설정하고 ResetPath로 남은 경로를 제거합니다.
     */
    protected void StopMoving()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    /**
     * @brief NavMeshAgent의 정지 상태를 해제합니다.
     *
     * @details
     * Agent가 존재하고 활성화되어 있으며 NavMesh 위에 있을 때만 isStopped를 false로 설정합니다.
     */
    protected void ResumeMoving()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    /**
     * @brief 현재 위치가 목표 위치에 도착했는지 평면 거리와 Agent remainingDistance로 판정합니다.
     *
     * @details
     * 우선 GetFlatDistance로 직접 평면 거리를 검사하여 arriveDistance 이하면 true를 반환합니다.
     * Agent가 없거나 NavMesh 위가 아니면 추가 판정을 할 수 없으므로 false를 반환합니다.
     * 경로 계산 중이면 아직 도착으로 보지 않습니다.
     * Agent remainingDistance가 arriveDistance 이하인지 최종 반환합니다.
     * @param targetPosition 도착 여부를 검사할 목표 위치입니다.
     * @param arriveDistance 도착으로 인정할 거리입니다.
     * @return 도착한 것으로 판단되면 true입니다.
     */
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

    /**
     * @brief 대상 위치를 향해 수평 방향으로 회전합니다.
     *
     * @details
     * 대상 위치의 y값을 현재 몬스터 y값으로 맞춰 수평 회전만 계산합니다.
     * 방향 벡터가 너무 작으면 회전을 생략합니다.
     * 즉시 회전 옵션이면 targetRot을 바로 적용하고, 아니면 RotateTowards로 turnSpeed만큼 부드럽게 회전합니다.
     * @param targetPosition 바라볼 대상 위치입니다.
     * @param instant true이면 보간 없이 즉시 회전합니다.
     */
    protected void FaceTarget(Vector3 targetPosition, bool instant = false)
    {
        Vector3 lookPos = targetPosition; // 수평 회전을 위해 y값을 보정한 목표 위치 지역 변수입니다.
        lookPos.y = transform.position.y;

        Vector3 dir = lookPos - transform.position; // 두 위치 사이의 수평 또는 일반 방향 벡터를 저장하는 지역 변수입니다.

        if (dir.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized); // 목표 방향을 바라보기 위해 계산한 회전값 지역 변수입니다.

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

    /**
     * @brief 인지 표시 GameObject가 꺼져 있을 때만 활성화합니다.
     *
     * @details
     * alertMarkPrefab이 null이면 아무 작업도 하지 않습니다.
     * 이미 activeSelf가 true이면 중복 SetActive 호출을 하지 않습니다.
     * 비활성화 상태일 때 SetActive(true)를 호출하여 표시합니다.
     */
    protected void ShowAlertMark()
    {
        if (alertMarkPrefab != null && !alertMarkPrefab.activeSelf)
        {
            alertMarkPrefab.SetActive(true);
        }
    }

    /**
     * @brief 대기 애니메이션 재생 방식을 자식 몬스터가 구현하도록 요구하는 추상 함수입니다.
     *
     * @details
     * 각 모델의 Animator 파라미터 이름과 Blend Tree 값이 다르기 때문에 자식 클래스에서 구현합니다.
     */
    protected abstract void PlayIdleAnim();
    /**
     * @brief 이동 애니메이션 재생 방식을 자식 몬스터가 구현하도록 요구하는 추상 함수입니다.
     *
     * @details
     * NavMesh 이동 자체는 BaseMonster가 처리하고, 애니메이션 파라미터는 자식 클래스가 처리합니다.
     */
    protected abstract void PlayMoveAnim();
    /**
     * @brief 공격 애니메이션 실행 방식을 자식 몬스터가 구현하도록 요구하는 추상 함수입니다.
     *
     * @details
     * 공격 종류 선택, Animator 트리거, 공격별 데미지 설정은 몬스터마다 다르므로 자식 클래스에서 구현합니다.
     */
    protected abstract void PlayAttackAnim();
    /**
     * @brief 피격 애니메이션 실행 방식을 자식 몬스터가 구현하도록 요구하는 추상 함수입니다.
     *
     * @details
     * 피격 트리거 이름이 모델마다 다르므로 자식 클래스에서 구현합니다.
     */
    protected abstract void PlayHitAnim();
    /**
     * @brief 사망 애니메이션 실행 방식을 자식 몬스터가 구현하도록 요구하는 추상 함수입니다.
     *
     * @details
     * 사망 트리거 이름과 지상/공중 여부가 모델마다 다를 수 있어 자식 클래스에서 구현합니다.
     */
    protected abstract void PlayDeathAnim();

    /**
     * @brief 두 위치의 y축 차이를 제거한 수평 거리를 계산합니다.
     *
     * @details
     * 두 벡터의 y값을 0으로 바꿔 높이 차이를 무시합니다.
     * Vector3.Distance로 xz 평면 거리만 반환합니다.
     * @param a 첫 번째 위치입니다.
     * @param b 두 번째 위치입니다.
     * @return 두 위치 사이의 수평 거리입니다.
     */
    protected float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    /**
     * @brief from에서 to로 향하는 수평 방향 벡터를 계산합니다.
     *
     * @details
     * to - from으로 방향 벡터를 만든 뒤 y값을 0으로 바꿔 높이 성분을 제거합니다.
     * @param from 시작 위치입니다.
     * @param to 도착 위치입니다.
     * @return y축이 제거된 방향 벡터입니다.
     */
    protected Vector3 GetFlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        dir.y = 0f;

        return dir;
    }
}