using UnityEngine;

/// <summary>
/// ���� Ÿ�� Rock ����.
/// ó������ Rubble ���·� ���� �ִٰ� �÷��̾ �����Ǹ� �Ͼ��,
/// ����/����/���� �� ���� �ð� ���� Idle ���°� �����Ǹ� �ٽ� Rubble ���·� ���ư���.
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
    /// BaseMonster.OnEnable()�� Idle ���԰� PlayIdleAnim()�� ȣ���ϹǷ�,
    /// base.OnEnable()���� ���� isRubble ���� �����ؾ� �Ѵ�.
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
    /// Rubble ���¿����� �Ϲ� SimpleMonster.UpdateIdle()�� �״�� ȣ������ �ʴ´�.
    /// SimpleMonster.UpdateIdle()�� PlayIdleAnim()�� ���� ȣ���ϱ� ������
    /// Rubble ���°� ���� �� �ִ�.
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
    /// ���� �ִ� ������ Idle ó��.
    /// ������ �ϵ�, ���� �������� locomotion�̳� idle �ִϸ��̼��� �ǵ帮�� �ʴ´�.
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
    /// ��� ������ Idle ó��.
    /// ���� ����� ������ ���� �ð� �� �ٽ� Rubble�� ���ư���.
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
    /// Rubble ���¿��� �Ϲ� ���� ��� ���·� 1ȸ�� ��ȯ�Ѵ�.
    /// ���� �� �ݺ��ؼ� rubbleToIdle�� ������� �ʵ��� isRubble�� ��� false�� �ٲ۴�.
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
    /// ������ Idle ���¿��� ���� �ð� �̻� ������� ���� Rubble ���·� ���ư���.
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
