using UnityEngine;

public class QSkill : SkillBase
{
    private void Awake()
    {
        Cooldown = 3f;
    }

    protected override void OnUse()
    {
        Debug.Log("Q");
    }
}
