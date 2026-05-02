using UnityEngine;
using System.Collections.Generic;

public class Sword : MonoBehaviour
{
    [SerializeField] private float _damage = 10f;
    private BoxCollider _collider;
    private HashSet<Collider> _hitTargets = new HashSet<Collider>();

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        DisableHit();
    }

    public void EnableHit()
    {
        _hitTargets.Clear();
        _collider.enabled = true;
    }

    public void DisableHit()
    {
        _collider.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"OnTriggerEnter 호출됨: {other.name}");

        if (_hitTargets.Contains(other)) return;

        _hitTargets.Add(other);

        if (other.TryGetComponent<IDamagable>(out var target))
        {
            Vector3 hitPoint = other.transform.position;
            Vector3 hitNormal = (other.transform.position - transform.position).normalized;
            target.OnDamage(_damage, hitPoint, hitNormal);
        }
    }
}
