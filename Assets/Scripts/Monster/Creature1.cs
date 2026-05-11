using UnityEngine;

public class Creature1 : SensoryMonster
{
    private static readonly int HashLocomotion = Animator.StringToHash("Locomotion");
    private static readonly int HashGotHit = Animator.StringToHash("GotHit");
    private static readonly int HashDeath = Animator.StringToHash("Death");

    private static readonly int HashAttack1 = Animator.StringToHash("Attack1");
    private static readonly int HashAttack2 = Animator.StringToHash("Attack2");
    private static readonly int HashAttack3 = Animator.StringToHash("Attack3");

    protected override void PlayIdleAnim()
    {
        anim.SetFloat(HashLocomotion, 0f, 0.1f, Time.deltaTime);
    }

    protected override void PlayMoveAnim()
    {
        anim.SetFloat(HashLocomotion, 1f, 0.1f, Time.deltaTime);
    }

    protected override void PlayAttackAnim()
    {
        FaceTarget(player.position, true);

        float rand = Random.value;

        if (rand < 0.5f)
        {
            SetCurrentAttackDamage(7f);
            anim.SetTrigger(HashAttack1);
        }
        else if (rand < 0.8f)
        {
            SetCurrentAttackDamage(3f);
            ApplyStatusEffect(StatusFlags.Bleed, 3f, 1.5f);
            anim.SetTrigger(HashAttack2);
        }
        else
        {
            SetCurrentAttackDamage(3f);
            ApplyStatusEffect(StatusFlags.Bleed, 3f, 1.5f);
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
