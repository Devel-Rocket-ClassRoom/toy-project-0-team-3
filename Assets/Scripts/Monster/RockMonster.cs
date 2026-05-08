using UnityEngine;

/// <summary>
/// 감지 타입 Rock 몬스터.
/// 처음에는 Rubble 상태로 숨어 있다가 플레이어가 감지되면 일어나고,
/// 추적/공격/복귀 후 일정 시간 동안 Idle 상태가 유지되면 다시 Rubble 상태로 돌아간다.
/// </summary>
public class RockMonster : SimpleMonster
{
    private static readonly int HashLocomotion = Animator.StringToHash("locomotion");

    private static readonly int HashRubbleToIdle = Animator.StringToHash("rubbleToIdle");
    private static readonly int HashIdleToRubble = Animator.StringToHash("idleToRubble");

    private static readonly int HashAttack1A = Animator.StringToHash("attack1A");
    private static readonly int HashAttack1B = Animator.StringToHash("attack1B");
    private static readonly int HashAttack2 = Animator.StringToHash("attack2");

    private static readonly int HashGotHit = Animator.StringToHash("gotHit");
    private static readonly int HashDeath = Animator.StringToHash("death");

    [Header("Rock Animation Values")]
    [SerializeField]
    private float idleLocomotionValue = 0f;

    [SerializeField]
    private float moveLocomotionValue = 1f;

    [Header("Rubble Settings")]
    [SerializeField]
    private float returnToRubbleDelay = 3f;

    private bool startAsRubble = true;
    private bool wakeWhenAlerted = true;
    private bool returnToRubbleOnIdle = true;

    [Header("Animator State Names")]
    [SerializeField]
    private bool forcePlayRubbleStateOnEnable = true;

    [SerializeField]
    private string rubbleStateName = "Rubble";

    private bool isRubble;
    private float idleTimer;

    /// <summary>
    /// BaseMonster.OnEnable()이 Idle 진입과 PlayIdleAnim()을 호출하므로,
    /// base.OnEnable()보다 먼저 isRubble 값을 세팅해야 한다.
    /// </summary>
    protected override void OnEnable()
    {
        isRubble = startAsRubble;
        idleTimer = 0f;

        base.OnEnable();

        if (anim == null)
        {
            return;
        }

        if (startAsRubble && forcePlayRubbleStateOnEnable)
        {
            anim.ResetTrigger(HashRubbleToIdle);
            anim.ResetTrigger(HashIdleToRubble);
            anim.Play(rubbleStateName, 0, 0f);
        }
        else
        {
            anim.SetFloat(HashLocomotion, idleLocomotionValue);
        }
    }

    /// <summary>
    /// Rubble 상태에서는 일반 SimpleMonster.UpdateIdle()을 그대로 호출하지 않는다.
    /// SimpleMonster.UpdateIdle()은 PlayIdleAnim()을 먼저 호출하기 때문에
    /// Rubble 상태가 깨질 수 있다.
    /// </summary>
    protected override void UpdateIdle()
    {
        if (player == null)
        {
            PlayIdleAnim();
            return;
        }

        if (isRubble)
        {
            UpdateRubbleIdle();
            return;
        }

        UpdateAwakeIdle();
    }

    /// <summary>
    /// 숨어 있는 상태의 Idle 처리.
    /// 감지는 하되, 감지 전까지는 locomotion이나 idle 애니메이션을 건드리지 않는다.
    /// </summary>
    private void UpdateRubbleIdle()
    {
        if (CanDetectPlayerForRock(traceRange))
        {
            WakeUpFromRubble();

            CurrentState = MonsterState.Trace;
            return;
        }

        if (CanDetectPlayerForRock(alertRange))
        {
            if (wakeWhenAlerted)
            {
                WakeUpFromRubble();
            }

            CurrentState = MonsterState.Alert;
            return;
        }
    }

    /// <summary>
    /// 깨어난 상태의 Idle 처리.
    /// 감지 대상이 없으면 일정 시간 후 다시 Rubble로 돌아간다.
    /// </summary>
    private void UpdateAwakeIdle()
    {
        PlayIdleAnim();

        if (CanDetectPlayerForRock(traceRange))
        {
            ResetRubbleTimer();
            CurrentState = MonsterState.Trace;
            return;
        }

        if (CanDetectPlayerForRock(alertRange))
        {
            ResetRubbleTimer();
            CurrentState = MonsterState.Alert;
            return;
        }

        if (!returnToRubbleOnIdle)
        {
            return;
        }

        if (isAttacking || isHitReacting)
        {
            ResetRubbleTimer();
            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= returnToRubbleDelay)
        {
            ReturnToRubbleIfNeeded();
        }
    }

    protected override void UpdateAlert()
    {
        WakeUpFromRubble();
        ResetRubbleTimer();
        base.UpdateAlert();
    }

    protected override void UpdateTrace()
    {
        WakeUpFromRubble();
        ResetRubbleTimer();
        base.UpdateTrace();
    }

    protected override void PlayIdleAnim()
    {
        if (isRubble)
        {
            return;
        }

        anim.SetFloat(HashLocomotion, idleLocomotionValue, 0.1f, Time.deltaTime);
    }

    protected override void PlayMoveAnim()
    {
        WakeUpFromRubble();
        ResetRubbleTimer();

        anim.SetFloat(HashLocomotion, moveLocomotionValue, 0.1f, Time.deltaTime);
    }

    protected override void PlayAttackAnim()
    {
        WakeUpFromRubble();
        ResetRubbleTimer();

        if (player != null)
        {
            FaceTarget(player.position, true);
        }

        anim.SetFloat(HashLocomotion, idleLocomotionValue);

        float rand = Random.value;

        if (rand < 0.5f)
        {
            SetCurrentAttackDamage(10f);
            anim.SetTrigger(HashAttack1A);
        }
        else if (rand < 0.8f)
        {
            SetCurrentAttackDamage(10f);
            anim.SetTrigger(HashAttack1B);
        }
        else
        {
            SetCurrentAttackDamage(20f);
            anim.SetTrigger(HashAttack2);
        }
    }

    protected override void PlayHitAnim()
    {
        WakeUpFromRubble();
        ResetRubbleTimer();

        anim.SetFloat(HashLocomotion, idleLocomotionValue);
        anim.SetTrigger(HashGotHit);
    }

    protected override void PlayDeathAnim()
    {
        WakeUpFromRubble();
        ResetRubbleTimer();

        anim.SetFloat(HashLocomotion, idleLocomotionValue);
        anim.SetTrigger(HashDeath);
    }

    /// <summary>
    /// Rubble 상태에서 일반 전투 대기 상태로 1회만 전환한다.
    /// 추적 중 반복해서 rubbleToIdle이 실행되지 않도록 isRubble을 즉시 false로 바꾼다.
    /// </summary>
    private void WakeUpFromRubble()
    {
        if (!isRubble)
        {
            return;
        }

        isRubble = false;
        idleTimer = 0f;

        anim.ResetTrigger(HashIdleToRubble);
        anim.SetTrigger(HashRubbleToIdle);
    }

    /// <summary>
    /// 완전히 Idle 상태에서 일정 시간 이상 대기했을 때만 Rubble 상태로 돌아간다.
    /// </summary>
    private void ReturnToRubbleIfNeeded()
    {
        if (!returnToRubbleOnIdle)
        {
            return;
        }

        if (isRubble)
        {
            return;
        }

        if (CurrentState != MonsterState.Idle)
        {
            return;
        }

        if (isAttacking || isHitReacting)
        {
            return;
        }

        isRubble = true;
        idleTimer = 0f;

        anim.ResetTrigger(HashRubbleToIdle);
        anim.SetFloat(HashLocomotion, idleLocomotionValue);
        anim.SetTrigger(HashIdleToRubble);
    }

    private void ResetRubbleTimer()
    {
        idleTimer = 0f;
    }

    private bool CanDetectPlayerForRock(float range)
    {
        return IsPlayerInsideRange(range, requireLineOfSight);
    }
}