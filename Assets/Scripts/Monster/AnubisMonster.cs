using UnityEngine;

/// <summary>
/// 시야 타입 Anubis 몬스터.
/// SensoryMonster의 시야 감지, 경계, 추적, 복귀 로직을 사용하고,
/// Animator 파라미터는 locomotion, gotHit, attack1, attack2, attack3만 사용한다.
/// </summary>
public class AnubisMonster : SensoryMonster
{
    private static readonly int HashLocomotion = Animator.StringToHash("locomotion");
    private static readonly int HashGotHit = Animator.StringToHash("gotHit");

    private static readonly int HashAttack1 = Animator.StringToHash("attack1");
    private static readonly int HashAttack2 = Animator.StringToHash("attack2");
    private static readonly int HashAttack3 = Animator.StringToHash("attack3");

    private static readonly int HashDeath = Animator.StringToHash("death");

    /// <summary>
    /// Anubis Blend Tree 기준:
    /// 0   = Walk Backwards
    /// 0.5 = Attack Idle
    /// 1   = Walk
    /// </summary>
    private const float BackwardValue = 0f;
    private const float IdleValue = 0.5f;
    private const float WalkValue = 1f;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (anim != null)
        {
            anim.SetFloat(HashLocomotion, IdleValue);
        }
    }

    /// <summary>
    /// Idle / Alert / 공격 대기 상태에서 사용.
    /// locomotion을 0으로 두면 Walk Backwards가 재생되므로 반드시 0.5를 사용한다.
    /// </summary>
    protected override void PlayIdleAnim()
    {
        anim.SetFloat(HashLocomotion, IdleValue, 0.1f, Time.deltaTime);
    }

    /// <summary>
    /// Trace / Return / Alert 위치 이동 중 사용.
    /// </summary>
    protected override void PlayMoveAnim()
    {
        anim.SetFloat(HashLocomotion, WalkValue, 0.1f, Time.deltaTime);
    }

    /// <summary>
    /// attack1 ~ attack3 중 하나를 무작위로 실행한다.
    /// 실제 데미지는 Animation Event에서 Animation_AttackHit()을 호출해서 처리한다.
    /// 공격 종료는 Animation_AttackEnd() 이벤트로 처리한다.
    /// </summary>
    protected override void PlayAttackAnim()
    {
        if (player != null)
        {
            FaceTarget(player.position, true);
        }

        anim.SetFloat(HashLocomotion, IdleValue);

        float rand = Random.value;

        if (rand < 0.5f)
        {
            SetCurrentAttackDamage(10f);
            anim.SetTrigger(HashAttack1);
        }
        else if (rand < 0.8f)
        {
            SetCurrentAttackDamage(12f);
            anim.SetTrigger(HashAttack2);
        }
        else
        {
            SetCurrentAttackDamage(15f);
            anim.SetTrigger(HashAttack3);
        }
    }

    /// <summary>
    /// 피격 애니메이션.
    /// BaseMonster에서 공격 중에는 피격 경직을 무시한다.
    /// </summary>
    protected override void PlayHitAnim()
    {
        anim.SetFloat(HashLocomotion, IdleValue);
        anim.SetTrigger(HashGotHit);
    }

    /// <summary>
    /// death 파라미터를 사용하지 않는 버전.
    /// BaseMonster가 Dead 상태에서 PlayDeathAnim()을 호출하므로 빈 구현은 필요하다.
    /// </summary>
    protected override void PlayDeathAnim()
    {
        anim.SetTrigger(HashDeath);
    }
}