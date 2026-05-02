using UnityEngine;

public class WSkill : SkillBase
{
    private void Awake() => Cooldown = 3f;

    protected override void OnUse()
    {
        Debug.Log("W");
    }
}

