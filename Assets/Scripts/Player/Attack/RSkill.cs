using UnityEngine;

public class RSkill : SkillBase
{
    private void Awake()
    {
        Cooldown = 3f;
    }

    protected override void OnUse()
    {
        Debug.Log("R");
    }
}

