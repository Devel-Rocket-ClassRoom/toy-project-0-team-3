using UnityEngine;

public abstract class SkillBase : MonoBehaviour, ISkill
{
    public float Cooldown { get; protected set; }
    private float _lastUsedTime = -Mathf.Infinity;
    public bool CanUse => Time.time >= _lastUsedTime + Cooldown;

    public float RemainingCooldown => Mathf.Max(0f, _lastUsedTime + Cooldown - Time.time);
    public float CooldownRatio => Cooldown > 0 ? RemainingCooldown / Cooldown : 0f;

    public void Use()
    {
        if (Time.time < _lastUsedTime + Cooldown)
            return;
        _lastUsedTime = Time.time;
        OnUse();
    }

    protected abstract void OnUse();
}
