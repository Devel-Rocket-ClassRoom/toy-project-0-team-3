using UnityEngine;
using System.Collections;

public class Mushroom : SimpleMonster
{
    private readonly int hashgoMonster = Animator.StringToHash("goMonster");
    private readonly int hashLocomotion = Animator.StringToHash("locomotion");
    private readonly int hashGotHit = Animator.StringToHash("gotHit");
    private readonly int hashDeath = Animator.StringToHash("death");
    private readonly int hashAtk1 = Animator.StringToHash("attack1");
    private readonly int hashAtk2 = Animator.StringToHash("attack2");
    private readonly int hashAtk3 = Animator.StringToHash("attack3");

    private bool hasAwoken = false;

    protected override void PlayIdleAnim()
    {
        anim.SetFloat(hashLocomotion, 0f, 0.1f, Time.deltaTime);
    }

    protected override void PlayMoveAnim()
    {
        if (!hasAwoken)
        {
            anim.SetTrigger(hashgoMonster);
            hasAwoken = true;
        }

        anim.SetFloat(hashLocomotion, 1f);
    }

    protected override void PlayDeathAnim()
    {
        anim.SetTrigger(hashDeath);
    }

    protected override IEnumerator AttackRoutine()
    {
        FaceTarget(player.position);

        float rand = Random.value;

        if (rand < 0.5f)
        {
            yield return StartCoroutine(Attack1());
        }
        else if (rand < 0.8f)
        {
            yield return StartCoroutine(Attack2());
        }
        else
        {
            yield return StartCoroutine(Attack3());
        }
    }

    private IEnumerator Attack1()
    {
        anim.SetTrigger(hashAtk1);

        yield return null;

        AnimatorStateInfo stateInfo;

        if (anim.IsInTransition(0))
        {
            stateInfo = anim.GetNextAnimatorStateInfo(0);
        }
        else
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        }

        float hitTime = 0.4f;

        yield return new WaitForSeconds(hitTime);

        UpdateAttack();

        Debug.Log("공격 1 타격!");

        float remainTime = stateInfo.length - hitTime;

        if (remainTime > 0)
        {
            yield return new WaitForSeconds(remainTime);
        }
    }

    private IEnumerator Attack2()
    {
        anim.SetTrigger(hashAtk2);

        yield return null;

        AnimatorStateInfo stateInfo;

        if (anim.IsInTransition(0))
        {
            stateInfo = anim.GetNextAnimatorStateInfo(0);
        }
        else
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        }

        float hitTime = 0.6f;

        yield return new WaitForSeconds(hitTime);

        UpdateAttack();

        Debug.Log("공격 2");

        float remainTime = stateInfo.length - hitTime;

        if (remainTime > 0)
        {
            yield return new WaitForSeconds(remainTime);
        }
    }

    private IEnumerator Attack3()
    {
        anim.SetTrigger(hashAtk3);
        yield return null;

        AnimatorStateInfo stateInfo;

        if (anim.IsInTransition(0))
        {
            stateInfo = anim.GetNextAnimatorStateInfo(0);
        }
        else
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        }

        float hitTime = 0.6f;

        yield return new WaitForSeconds(hitTime);

        UpdateAttack();

        Debug.Log("공격 3");

        float remainTime = stateInfo.length - hitTime;

        if (remainTime > 0)
        {
            yield return new WaitForSeconds(remainTime);
        }
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.OnDamage(damage, hitPoint, hitNormal);

        if (!IsDead)
        {
            if (isAttacking)
            {
                Debug.Log($"{gameObject.name}이(가) 공격 중이라 경직을 무시합니다.");
                return;
            }

            anim.SetTrigger(hashGotHit);
        }
    }
}