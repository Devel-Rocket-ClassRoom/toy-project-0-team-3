using UnityEngine;

/// <summary>
/// Dragon 보스 몬스터.
/// BaseMonster의 공통 상태 머신을 사용하고,
/// 지상 추적 / 근접 공격 / 브레스 공격 / 복귀를 처리한다.
/// </summary>
public class DragonBoss : BaseMonster
{
    private static readonly int HashLocomotion = Animator.StringToHash("locomotion");

    private static readonly int HashAttack1 = Animator.StringToHash("attack1");
    private static readonly int HashAttack2 = Animator.StringToHash("attack2");
    private static readonly int HashBreatheFire = Animator.StringToHash("breatheFire");

    private static readonly int HashGotHit1 = Animator.StringToHash("gotHit1");
    private static readonly int HashGotHit2 = Animator.StringToHash("gotHit2");
    private static readonly int HashDeath = Animator.StringToHash("death");

    private const float LocomotionBackward = 0f;
    private const float LocomotionIdle = 0.5f;
    private const float LocomotionWalk = 0.75f;
    private const float LocomotionRun = 1f;

    [Header("Dragon Boss Ranges")]
    [SerializeField] private float activationRange = 18f;
    [SerializeField] private float leashRange = 35f;
    [SerializeField] private float meleeRange = 4f;
    [SerializeField] private float breathRange = 12f;

    [Header("Dragon Move")]
    [SerializeField] private bool useRunAnimation = true;

    protected override void UpdateIdle()
    {
        PlayIdleAnim();

        if (player == null)
        {
            return;
        }

        float dist = GetDistanceToPlayer();

        if (dist <= activationRange)
        {
            CurrentState = MonsterState.Trace;
        }
    }

    protected override void UpdateAlert()
    {
        CurrentState = MonsterState.Trace;
    }

    protected override void UpdateTrace()
    {
        if (player == null)
        {
            CurrentState = MonsterState.Return;
            return;
        }

        float dist = GetDistanceToPlayer();

        if (dist > leashRange)
        {
            CurrentState = MonsterState.Return;
            return;
        }

        if (dist <= attackRange)
        {
            CurrentState = MonsterState.Attack;
            return;
        }

        MoveTo(player.position);
    }

    protected override float GetChaseRange()
    {
        return leashRange;
    }

    protected override void PlayIdleAnim()
    {
        anim.SetFloat(HashLocomotion, LocomotionIdle, 0.1f, Time.deltaTime);
    }

    protected override void PlayMoveAnim()
    {
        float value = useRunAnimation ? LocomotionRun : LocomotionWalk;
        anim.SetFloat(HashLocomotion, value, 0.1f, Time.deltaTime);
    }

    protected override void PlayAttackAnim()
    {
        if (player != null)
        {
            FaceTarget(player.position, true);
        }

        anim.SetFloat(HashLocomotion, LocomotionIdle);

        float dist = GetDistanceToPlayer();

        if (dist <= meleeRange)
        {
            int attackIndex = Random.Range(0, 2);

            if (attackIndex == 0)
            {
                anim.SetTrigger(HashAttack1);
            }
            else
            {
                anim.SetTrigger(HashAttack2);
            }

            return;
        }

        if (dist <= breathRange)
        {
            anim.SetTrigger(HashBreatheFire);
            return;
        }

        CurrentState = MonsterState.Trace;
    }

    protected override void PlayHitAnim()
    {
        anim.SetFloat(HashLocomotion, LocomotionIdle);

        int hitIndex = Random.Range(0, 2);

        if (hitIndex == 0)
        {
            anim.SetTrigger(HashGotHit1);
        }
        else
        {
            anim.SetTrigger(HashGotHit2);
        }
    }

    protected override void PlayDeathAnim()
    {
        StopMoving();
        anim.SetFloat(HashLocomotion, LocomotionIdle);
        anim.SetTrigger(HashDeath);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, leashRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, breathRange);

        if (Application.isPlaying && player != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
#endif
}