using System.Collections;
using UnityEngine;

public class Mushroom : SimpleMonster
{
    [Header("Multi Attack Settings")]
    public float attackRange = 2f;      
    public float attackCooldown = 2.0f; 

    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    protected override void UpdateChase()
    {
        if (playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= attackRange && HasLineOfSight(playerTarget))
        {
            currentState = MonsterState.Attack;
            return;
        }

        base.UpdateChase();
    }

    protected override void UpdateAttack()
    {
        if (isAttacking) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            if (distance <= attackRange && HasLineOfSight(playerTarget))
            {
                ChooseAndExecuteAttack();
            }
            else
            {
                currentState = MonsterState.Chase;
            }
        }
        else
        {
            if (distance > attackRange || !HasLineOfSight(playerTarget))
            {
                currentState = MonsterState.Chase;
            }
        }
    }

    private void ChooseAndExecuteAttack()
    {
        lastAttackTime = Time.time; 

        float randomValue = Random.value;

        if (randomValue < 0.5f)
        {
            StartCoroutine(BasicAttack1());
        }
        else if (randomValue < 0.8f)
        {
            StartCoroutine(BasicAttack2());
        }
        else
        {
            StartCoroutine(SpecialAttack3());
        }
    }

    // --- 3가지 공격 패턴 구현부 ---

    private IEnumerator BasicAttack1()
    {
        isAttacking = true;
        Debug.Log("[기본 공격 1] 가벼운 휘두르기!");

        yield return new WaitForSeconds(0.4f); // 선딜레이

        // 데미지 판정 로직
        Debug.Log("휙! 10 데미지");

        yield return new WaitForSeconds(0.6f); // 후딜레이
        isAttacking = false;
    }

    private IEnumerator BasicAttack2()
    {
        isAttacking = true;
        Debug.Log("[기본 공격 2] 묵직한 내려찍기!");

        yield return new WaitForSeconds(1.2f); // 선딜레이

        // 데미지 판정 로직
        Debug.Log("쾅! 25 데미지");

        yield return new WaitForSeconds(1.0f); // 후딜레이 
        isAttacking = false;
    }

    private IEnumerator SpecialAttack3()
    {
        isAttacking = true;
        Debug.Log("[특수 공격 3] 분노의 광역 휠윈드 준비!!!");

        yield return new WaitForSeconds(1.5f); // 선딜레이

        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"휘릭! 광역 {i + 1}타 15 데미지");
            // 데미지 판정 로직
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1.5f); // 후딜레이
        isAttacking = false;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}