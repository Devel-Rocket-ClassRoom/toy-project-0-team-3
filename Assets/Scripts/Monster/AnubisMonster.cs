using UnityEngine;

/**
 * @brief SensoryMonster 기반의 시야 감지형 아누비스 몬스터 구현 클래스입니다.
 *
 * @details
 * BaseMonster와 SensoryMonster가 제공하는 상태 전환, NavMesh 이동, 시야 감지, 경계, 추적, 공격, 복귀, 사망 흐름을 그대로 사용합니다.
 * 이 클래스는 아누비스 전용 Animator 파라미터 이름과 공격별 데미지 선택만 담당합니다.
 * locomotion Blend Tree에서 0은 후진, 0.5는 대기, 1은 전진으로 사용되므로 대기 상태에서 0.5를 명시합니다.
 */

public class AnubisMonster : SensoryMonster
{
    /**
     * @brief Animator의 locomotion 또는 Locomotion 파라미터를 빠르게 접근하기 위해 미리 계산한 해시 값입니다.
     */
    private static readonly int HashLocomotion = Animator.StringToHash("locomotion");

    /**
     * @brief 피격 애니메이션 트리거 파라미터를 빠르게 접근하기 위해 미리 계산한 해시 값입니다.
     */
    private static readonly int HashGotHit = Animator.StringToHash("gotHit");

    /**
     * @brief 첫 번째 공격 애니메이션 트리거 파라미터의 해시 값입니다.
     */
    private static readonly int HashAttack1 = Animator.StringToHash("attack1");

    /**
     * @brief RockMonster 또는 Creature 계열의 두 번째 공격 트리거 해시 값입니다.
     */
    private static readonly int HashAttack2 = Animator.StringToHash("attack2");

    /**
     * @brief 세 번째 공격 애니메이션 트리거 파라미터의 해시 값입니다.
     */
    private static readonly int HashAttack3 = Animator.StringToHash("attack3");

    /**
     * @brief 사망 애니메이션 트리거 파라미터를 빠르게 접근하기 위해 미리 계산한 해시 값입니다.
     */
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

    /**
     * @brief AnubisMonster 활성화 시 공통 몬스터 초기화 후 locomotion을 대기 값으로 보정합니다.
     *
     * @details
     * base.OnEnable로 공통 상태를 초기화한 뒤 Animator가 있으면 locomotion을 IdleValue로 설정하여 후진 애니메이션이 재생되지 않게 합니다.
     */
    protected override void PlayIdleAnim()
    {
        anim.SetFloat(HashLocomotion, IdleValue, 0.1f, Time.deltaTime);
    }

    /**
     * @brief 현재 몬스터의 대기 애니메이션 파라미터를 설정합니다.
     *
     * @details
     * Animator의 locomotion 계열 float 값을 해당 몬스터의 Idle 값으로 보간 설정합니다.
     * RockMonster처럼 은신 상태가 있는 경우에는 Rubble 상태를 깨지 않기 위해 조기 종료할 수 있습니다.
     */
    protected override void PlayMoveAnim()
    {
        anim.SetFloat(HashLocomotion, WalkValue, 0.1f, Time.deltaTime);
    }

    /**
     * @brief 현재 몬스터의 이동 애니메이션 파라미터를 설정합니다.
     *
     * @details
     * 필요한 경우 각성/은신 해제 트리거를 먼저 실행합니다.
     * Animator의 locomotion 계열 float 값을 이동 값으로 보간 설정합니다.
     */
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

    /**
    * @brief 현재 몬스터의 피격 애니메이션을 실행합니다.
    *
    * @details
    * 필요한 경우 대기 locomotion 값을 맞춘 뒤 피격 트리거를 Animator에 전달합니다.
    */
    protected override void PlayHitAnim()
    {
        anim.SetFloat(HashLocomotion, IdleValue);
        anim.SetTrigger(HashGotHit);
    }

    /**
     * @brief 현재 몬스터의 사망 애니메이션을 실행합니다.
     *
     * @details
     * 필요한 경우 대기 locomotion 값을 맞춘 뒤 사망 트리거를 Animator에 전달합니다.
     */
    protected override void PlayDeathAnim()
    {
        anim.SetTrigger(HashDeath);
    }
}
