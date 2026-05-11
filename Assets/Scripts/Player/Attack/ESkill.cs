using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESkill : SkillBase
{
    [SerializeField]
    private float _jumpHeight = 3f;

    [SerializeField]
    private float _forwardDistance = 3f;

    [SerializeField]
    private float _jumpDuration = 0.5f;

    [SerializeField]
    private float _smashDuration = 0.2f;

    [SerializeField]
    private GameObject _rangeIndicator;

    private PlayerSkill _playerSkill;
    private Rigidbody _rigidbody;

    [SerializeField]
    private float _damage = 30f;

    [SerializeField]
    private Collider _eCollider;

    private HashSet<Collider> _hitTargets = new HashSet<Collider>();

    private void Awake()
    {
        Cooldown = 5f;
        _playerSkill = GetComponentInParent<PlayerSkill>();
        _rigidbody = GetComponentInParent<Rigidbody>();
        _eCollider.GetComponent<ESkillHitBox>().Init(this);
        DisableHit();
    }

    private void EnableHit()
    {
        _hitTargets.Clear();
        _eCollider.enabled = true;
        _rangeIndicator.SetActive(true);
    }

    private void DisableHit()
    {
        _eCollider.enabled = false;
        _rangeIndicator.SetActive(false);
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
                other.transform.position - _eCollider.transform.position
            ).normalized;
            target.OnDamage(_damage, hitPoint, hitNormal);
        }
    }

    protected override void OnUse() { }

    public void JumpShot()
    {
        StartCoroutine(JumpCoroutine());
    }

    public void GroundSmash()
    {
        StartCoroutine(SmashCoroutine());
    }

    private IEnumerator JumpCoroutine()
    {
        Vector3 startPos = _rigidbody.position;
        Vector3 endPos = startPos + _rigidbody.transform.forward * _forwardDistance;

        float elapsed = 0f;
        while (elapsed < _jumpDuration)
        {
            float t = elapsed / _jumpDuration;
            float height = Mathf.Sin(t * Mathf.PI) * _jumpHeight;

            Vector3 nextPos = Vector3.Lerp(startPos, endPos, t);
            nextPos.y = startPos.y + height;

            _rigidbody.MovePosition(nextPos);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _rigidbody.MovePosition(endPos);
    }

    private IEnumerator SmashCoroutine()
    {
        Vector3 startPos = _rigidbody.position;
        // 현재 위치에서 바닥(y=0 또는 시작높이)으로 내려찍기
        Vector3 endPos = new Vector3(startPos.x, 0f, startPos.z);

        float elapsed = 0f;
        while (elapsed < _smashDuration)
        {
            float t = elapsed / _smashDuration;

            // 처음엔 느리게 나중엔 빠르게 (가속하며 내려찍는 느낌)
            float easedT = t * t;

            Vector3 nextPos = Vector3.Lerp(startPos, endPos, easedT);
            _rigidbody.MovePosition(nextPos);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _rigidbody.MovePosition(endPos);
        EnableHit();
        yield return new WaitForSeconds(0.2f);
        DisableHit();
    }
}
