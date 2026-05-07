using UnityEngine;

public class TestPlayer : LivingEntity
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("마나")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegen = 5f;

    public float CurrentMana { get; private set; }
    public float MaxHealth => startingHealth;
    public float MaxMana => maxMana;

    private StatusFlags currentTestFlag = StatusFlags.Burn;

    protected override void OnEnable()
    {
        base.OnEnable();
        CurrentMana = maxMana;
    }

    protected override void Update()
    {
        base.Update();

        TickManaRegen();

        // 새로 추가된 이동 처리 메서드
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAttack();
            Debug.Log("공격시도");
        }
    }

    // --- 새로 추가된 영역 ---
    private void HandleMovement()
    {
        // Unity의 기본 레거시 Input 설정을 사용하여 방향키 및 WASD 입력을 받습니다.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 이동 벡터 생성 및 정규화
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 프레임률에 독립적인 이동을 위한 Time.deltaTime 적용
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
    // ----------------------

    public void UseMana(float amount)
    {
        if (IsDead)
        {
            return;
        }

        CurrentMana = Mathf.Clamp(CurrentMana - amount, 0f, maxMana);
    }

    private void TickManaRegen()
    {
        if (CurrentMana >= maxMana)
        {
            return;
        }

        CurrentMana = Mathf.Min(CurrentMana + manaRegen * Time.deltaTime, maxMana);
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
            Die();
        }
    }

    private void TestAttack()
    {
        float attackRadius = 8f;
        float attackDamage = 10f;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);

        foreach (Collider hitCol in hitColliders)
        {
            LivingEntity target = hitCol.GetComponentInParent<LivingEntity>();

            if (target == null)
            {
                continue;
            }

            if (target == this)
            {
                continue;
            }

            if (!target.CompareTag("Monster") && !target.transform.root.CompareTag("Monster"))
            {
                continue;
            }

            target.OnDamage(
                attackDamage,
                hitCol.ClosestPoint(transform.position),
                transform.forward
            );

            Debug.Log($"[테스트 공격] {target.name}에게 {attackDamage} 데미지 적용");
        }
    }
    public override void Die()
    {
        base.Die();
        Debug.Log("[PlayerStatus] 플레이어가 사망했습니다!");
    }
}