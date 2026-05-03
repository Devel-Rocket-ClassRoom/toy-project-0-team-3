using UnityEngine;

public class TestPlayer : LivingEntity
{
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAttack();
        }
    }

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

        //Debug.Log($"[PlayerStatus] 플레이어가 {damage}의 데미지를 받았습니다. 남은 체력: {Health}");

        if (IsDead)
        {
            Die();
        }
    }

    private void TestAttack()
    {
        float attackRadius = 5f;
        float attackDamage = 10f; 
        float stunDuration = 3f;  

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);

        foreach (Collider hitCol in hitColliders)
        {
            if (hitCol.CompareTag("Monster"))
            {
                BaseMonster monster = hitCol.GetComponent<BaseMonster>();

                if (monster != null)
                {
                    monster.OnDamage(attackDamage, hitCol.ClosestPoint(transform.position), transform.forward);

                    monster.ApplyStun(stunDuration);

                    Debug.Log($"[테스트 공격] {hitCol.name}에게 {attackDamage}의 데미지와 {stunDuration}초 스턴을 부여했습니다!");
                }
            }
        }
    }

    public override void Die()
    {
        base.Die();
        Debug.Log("[PlayerStatus] 플레이어가 사망했습니다!");
    }
}
