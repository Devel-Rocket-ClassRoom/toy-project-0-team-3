using System.Collections;
using UnityEngine;

public class QSkill : SkillBase
{
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashDuration = 0.3f;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        Cooldown = 3f;
        // Skills 오브젝트가 아니라 Warrior(부모)에 붙어있으니까
        _rigidbody = GetComponentInParent<Rigidbody>();
    }

    protected override void OnUse()
    {
        // StartCoroutine(DashCoroutine());
    }

    public void OnDashStart()
    {
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < _dashDuration)
        {
            _rigidbody.MovePosition(_rigidbody.position + _rigidbody.transform.forward * _dashSpeed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}