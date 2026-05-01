using UnityEngine;
using UnityEngine.Events;

public class LivingEntity : MonoBehaviour, IDamagable
{
    public float startingHealth = 100f;
    public float Health { get; private set; }
    public bool IsDead { get; private set; }

    public UnityEvent OnDead;

    protected virtual void OnEnable()
    {
        IsDead = false;
        Health = startingHealth;
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

    public virtual void Die()
    {
        IsDead = true;
        OnDead?.Invoke();
    }
}