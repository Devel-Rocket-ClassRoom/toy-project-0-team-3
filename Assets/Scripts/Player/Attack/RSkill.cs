using System.Collections;
using UnityEngine;

public class RSkill : SkillBase
{
    [SerializeField] private GameObject _lightningPrefab;  // 번개 파티클 프리펩
    [SerializeField] private float _radius = 3f;           // 원 반지름
    [SerializeField] private int _strikeCount = 3;         // 쾅쾅쾅 횟수
    [SerializeField] private int _lightningPerStrike = 5;  // 한 번에 떨어지는 번개 수
    [SerializeField] private float _strikeCooldown = 0.4f; // 쾅쾅 사이 간격
    [SerializeField] private float _damage = 30f;
    [SerializeField] private float _thickness = 0.5f;

    private PlayerSkill _playerSkill;

    private void Awake()
    {
        Cooldown = 6f;
        _playerSkill = GetComponentInParent<PlayerSkill>();
    }

    protected override void OnUse()
    {
        StartCoroutine(LightningCoroutine());
    }

    private IEnumerator LightningCoroutine()
    {
        float currentRadius = 0f;
        float radiusStep = _radius / _strikeCount; // 매 strike마다 반지름 증가량

        for (int i = 0; i < _strikeCount; i++)
        {
            currentRadius += radiusStep;
            SpawnLightningCircle(currentRadius);
            StrikeDamage(currentRadius);
            yield return new WaitForSeconds(_strikeCooldown);
        }

        _playerSkill.OnSkillEnd();
    }

    private void SpawnLightningCircle(float radius)
    {
        for (int i = 0; i < _lightningPerStrike; i++)
        {
            float angle = i * (360f / _lightningPerStrike);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
            Vector3 spawnPos = transform.position + offset;

            GameObject lightning = Instantiate(_lightningPrefab, spawnPos, Quaternion.identity);
            ParticleSystem ps = lightning.GetComponent<ParticleSystem>();
            ps.Play();

            Destroy(lightning, ps.main.duration);
        }
    }

    private void StrikeDamage(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius + _thickness, LayerMask.GetMask("Monster"));

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < radius - _thickness) continue;

            if (hit.TryGetComponent<IDamagable>(out var target))
            {
                Vector3 hitNormal = (hit.transform.position - transform.position).normalized;
                target.OnDamage(_damage, hit.transform.position, hitNormal);
            }
        }
    }
}