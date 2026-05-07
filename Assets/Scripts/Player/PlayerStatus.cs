using System.Collections;
using UnityEngine;

public class PlayerStatus : LivingEntity
{
    [Header("마나")]
    [SerializeField] private float maxMana = 200f;
    [SerializeField] private float manaRegen = 5f;
    [SerializeField] private float invincibleDuration = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f; // 깜빡이는 속도

    public float CurrentMana { get; private set; }
    public float MaxHealth => startingHealth;
    public float MaxMana => maxMana;
    public bool IsHit { get; private set; }
    public bool IsInvincible { get; private set; }

    private Coroutine _invincibleCoroutine;

    private PlayerSkill _playerSkill;
    private Animator _animator;

    private SkinnedMeshRenderer[] _renderers; // 스킨 렌더러

    public float[] GetRemainingCooldowns() => _playerSkill.GetRemainingCooldowns();

    protected override void OnEnable()
    {
        base.OnEnable();  // Health, IsDead 초기화
        CurrentMana = maxMana;
        IsHit = false;
        IsInvincible = false;
    }

    public void Awake()
    {
        _playerSkill = GetComponent<PlayerSkill>();
        _animator = GetComponent<Animator>();
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    protected override void Update()
    {
        base.Update();

        TickManaRegen();

        // 테스트 코드
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnDamage(30f, Vector3.zero, Vector3.zero);
            UseMana(50f);
        }
    }

    public void UseMana(float amount)
    {
        if (IsDead) return;
        CurrentMana = Mathf.Clamp(CurrentMana - amount, 0f, maxMana);
    }

    public float[] GetCooldownRatios() => _playerSkill.GetCooldownRatios();

    private void TickManaRegen()
    {
        if (CurrentMana >= maxMana) return;
        CurrentMana = Mathf.Min(CurrentMana + manaRegen * Time.deltaTime, maxMana);
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead) return;
        if (IsHit) return;
        if (IsInvincible) return;

        base.OnDamage(damage, hitPoint, hitNormal);
        Debug.Log(Health);

        // Die()에서 Dead 트리거를 쏘므로 여기선 살아있을 때만 Damaged 재생
        if (!IsDead)
        {
            IsHit = true;
            _animator.SetTrigger("Damaged");

            GetComponent<PlayerAttack>().ForceReset();
            GetComponent<PlayerSkill>().OnSkillEnd();

            if (_invincibleCoroutine != null)
                StopCoroutine(_invincibleCoroutine);
            _invincibleCoroutine = StartCoroutine(InvincibleCoroutine());
        }
    }

    private IEnumerator InvincibleCoroutine()
    {
        IsInvincible = true;

        Coroutine blinkCoroutine = StartCoroutine(BlinkCoroutine());

        yield return new WaitForSeconds(invincibleDuration);

        // 깜빡임 종료 후 원래대로 복구
        StopCoroutine(blinkCoroutine);
        SetRenderersVisible(true);

        IsInvincible = false;
        _invincibleCoroutine = null;
    }

    private IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            SetRenderersVisible(false);
            yield return new WaitForSeconds(blinkInterval);
            SetRenderersVisible(true);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (var r in _renderers)
            r.enabled = visible;
    }

    public void OnHitEnd()
    {
        IsHit = false;
    }

    public override void Die()
    {
        base.Die();
        IsHit = false;
        IsInvincible = false;

        if (_invincibleCoroutine != null)
        {
            StopCoroutine(_invincibleCoroutine); // 추가
            _invincibleCoroutine = null;
        }

        _animator.SetTrigger("Dead");
    }

    public void SetInvincible(bool value)
    {
        IsInvincible = value;
    }
}