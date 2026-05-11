using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QSkill : SkillBase
{
    [SerializeField]
    private float _dashSpeed = 15f;

    [SerializeField]
    private float _dashDuration = 0.3f;

    [SerializeField]
    private float _damage = 20f;

    public Vector3 Direction { get; set; } = Vector3.zero;

    private Rigidbody _rigidbody;
    private HashSet<Collider> _hitTargets = new HashSet<Collider>();

    [SerializeField]
    private Collider _qCollider;

    private void Awake()
    {
        Cooldown = 3f;
        // Skills 오브젝트가 아니라 Warrior(부모)에 붙어있으니까
        _rigidbody = GetComponentInParent<Rigidbody>();
        _qCollider.GetComponent<QSkillHitBox>().Init(this);
        DisableHit();
    }

    protected override void OnUse()
    {
        // StartCoroutine(DashCoroutine());
    }

    public void OnDashStart()
    {
        StartCoroutine(DashCoroutine());
    }

    private void EnableHit()
    {
        _hitTargets.Clear();
        _qCollider.enabled = true;
    }

    private void DisableHit()
    {
        _qCollider.enabled = false;
    }

    private IEnumerator DashCoroutine()
    {
        Vector3 QDirection = Direction != Vector3.zero ? Direction : transform.forward;

        EnableHit();

        float elapsed = 0f;
        while (elapsed < _dashDuration)
        {
            _rigidbody.MovePosition(
                _rigidbody.position + QDirection * _dashSpeed * Time.fixedDeltaTime
            );
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        DisableHit();
        Direction = Vector3.zero;
    }

    public void OnHit(Collider other)
    {
        if (_hitTargets.Contains(other))
            return;
        _hitTargets.Add(other);

        if (other.TryGetComponent<IDamagable>(out var target))
        {
            Vector3 hitPoint = other.transform.position;
            Vector3 hitNormal = (
                other.transform.position - _qCollider.transform.position
            ).normalized;
            target.OnDamage(_damage, hitPoint, hitNormal);
        }
    }
}
