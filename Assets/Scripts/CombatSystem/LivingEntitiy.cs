using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class LivingEntity : MonoBehaviour, IDamagable
{
    public float startingHealth = 100f;
    public float Health { get; private set; }
    public bool IsDead { get; private set; }

    public UnityEvent OnDead;

    public StatusFlags currentStatusMask { get; private set; } = StatusFlags.None;
    private Dictionary<StatusFlags, StatusEffectData> activeEffects = new Dictionary<StatusFlags, StatusEffectData>();


    protected virtual void OnEnable()
    {
        IsDead = false;
        Health = startingHealth;
        currentStatusMask = StatusFlags.None;
        activeEffects.Clear();
    }

    protected virtual void Update()
    {
        if (IsDead)
        {
            return;
        }

        UpdateStatusEffects();
    }

    public void ApplyStatusEffect(StatusFlags newFlag, float duration, float tickDamage = 0f)
    {
        if (IsDead)
        {
            return;
        }

        if ((StatusFlags.ElementalGroup & newFlag) != 0)
        {
            currentStatusMask &= ~StatusFlags.ElementalGroup;

            activeEffects.Remove(StatusFlags.Electric);
            activeEffects.Remove(StatusFlags.Burn);
            activeEffects.Remove(StatusFlags.Frostbite);
        }

        currentStatusMask |= newFlag;

        activeEffects[newFlag] = new StatusEffectData(duration, tickDamage);
    }

    private void UpdateStatusEffects()
    {
        List<StatusFlags> keysList = new List<StatusFlags>(activeEffects.Keys);

        foreach (StatusFlags flag in keysList)
        {
            StatusEffectData data = activeEffects[flag];

            if (data.tickDamage > 0)
            {
                data.tickTimer += Time.deltaTime;

                if (data.tickTimer >= 1f)
                {
                    OnDamage(data.tickDamage, transform.position, Vector3.zero);
                    data.tickTimer -= 1f;
                    Debug.Log($"{flag}");

                    if (IsDead)
                    {
                        return;
                    }
                }
            }

            data.duration -= Time.deltaTime;

            if (data.duration <= 0)
            {
                currentStatusMask &= ~flag;
                activeEffects.Remove(flag);
                Debug.Log($"{flag} 상태이상이 종료되었습니다.");
            }
        }
    }

    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    public virtual void Heal(float amount)
    {
        if (IsDead)
        {
            return;
        }

        Health = Mathf.Clamp(Health + amount, 0f, startingHealth);
    }

    public virtual void Die()
    {
        IsDead = true;
        OnDead?.Invoke();
    }
}