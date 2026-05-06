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

        int attackIndex = Random.Range(1, 4);

        switch (attackIndex)
        {
            case 1:
                anim.SetTrigger(HashAttack1);
                break;

            case 2:
                anim.SetTrigger(HashAttack2);
                break;

            case 3:
                anim.SetTrigger(HashAttack3);
                break;

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