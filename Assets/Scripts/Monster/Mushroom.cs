using UnityEngine;

public class Mushroom : SimpleMonster
{
    private static readonly int HashGoMonster = Animator.StringToHash("goMonster");
    private static readonly int HashLocomotion = Animator.StringToHash("locomotion");
    private static readonly int HashGotHit = Animator.StringToHash("gotHit");
    private static readonly int HashDeath = Animator.StringToHash("death");
    private static readonly int HashAttack1 = Animator.StringToHash("attack1");
    private static readonly int HashAttack2 = Animator.StringToHash("attack2");
    private static readonly int HashAttack3 = Animator.StringToHash("attack3");

    private bool hasAwoken;

    protected override void OnEnable()
    {
        base.OnEnable();
        hasAwoken = false;
    }

    protected override void PlayIdleAnim()
    {
        anim.SetFloat(HashLocomotion, 0f, 0.1f, Time.deltaTime);
    }

    protected override void PlayMoveAnim()
    {
        if (!hasAwoken)
        {
            anim.SetTrigger(HashGoMonster);
            hasAwoken = true;
        }

        anim.SetFloat(HashLocomotion, 1f, 0.1f, Time.deltaTime);
    }

    protected override void PlayAttackAnim()
    {
        FaceTarget(player.position, true);

        float rand = Random.value;

        if (rand < 0.5f)
        {
            anim.SetTrigger(HashAttack1);
        }
        else if (rand < 0.8f)
        {
            anim.SetTrigger(HashAttack2);
        }
        else
        {
            anim.SetTrigger(HashAttack3);
        }
    }

    protected override void PlayHitAnim()
    {
        anim.SetTrigger(HashGotHit);
    }

    protected override void PlayDeathAnim()
    {
        anim.SetTrigger(HashDeath);
    }
}