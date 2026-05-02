using UnityEngine;

public class MonsterTest : LivingEntity
{
    [SerializeField] private float _knockbackForce = 5f;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.OnDamage(damage, hitPoint, hitNormal);
        Debug.Log($"데미지 받음: {damage}, 현재 체력: {Health}");

        // hitNormal 방향으로 밀기
        _rigidbody.AddForce(hitNormal * _knockbackForce, ForceMode.Impulse);
    }

    public override void Die()
    {
        base.Die();
        Debug.Log("몬스터 사망!");
        Destroy(gameObject);
    }
}