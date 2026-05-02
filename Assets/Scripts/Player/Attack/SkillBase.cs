using UnityEngine;

public abstract class SkillBase : MonoBehaviour, ISkill
{
    public float Cooldown { get; protected set; }
    private float _lastUsedTime = -Mathf.Infinity;
    public bool CanUse => Time.time >= _lastUsedTime + Cooldown;

    public void Use()
    {
        if (Time.time < _lastUsedTime + Cooldown) return;
        _lastUsedTime = Time.time;
        OnUse();
    }

    protected abstract void OnUse();
}
