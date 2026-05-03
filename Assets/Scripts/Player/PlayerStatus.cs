using UnityEngine;

public class PlayerStatus : LivingEntity
{
    [Header("마나")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegen = 5f;

    public float CurrentMana { get; private set; }
    public float MaxHealth => startingHealth;
    public float MaxMana => maxMana;

    private PlayerSkill _playerSkill;

    public float[] GetRemainingCooldowns() => _playerSkill.GetRemainingCooldowns();

    protected override void OnEnable()
    {
        base.OnEnable();  // Health, IsDead 초기화
        CurrentMana = maxMana;
        _playerSkill = GetComponent<PlayerSkill>();
    }

    protected override void Update()
    {
        base.Update();

        TickManaRegen();

        // 테스트 코드
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     OnDamage(30f, Vector3.zero, Vector3.zero);
        //     UseMana(50f);
        // }
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
}