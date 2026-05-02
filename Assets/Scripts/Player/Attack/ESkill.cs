using UnityEngine;

public class ESkill : SkillBase
{
    private void Awake() => Cooldown = 3f;

    protected override void OnUse()
    {
        Debug.Log("E");
    }
}
