using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private PlayerStatus playerStatus;

    [Header("UI 컴포넌트")]
    [SerializeField]
    private HealthBarUI healthBar;

    [SerializeField]
    private ManaBarUI manaBar;

    [SerializeField]
    private SkillSlotUI[] skillSlots = new SkillSlotUI[4];

    private void Awake()
    {
        playerStatus.OnDead.AddListener(OnPlayerDied);
    }

    private void Update()
    {
        healthBar.UpdateBar(playerStatus.Health, playerStatus.MaxHealth);
        manaBar.UpdateBar(playerStatus.CurrentMana, playerStatus.MaxMana);

        float[] ratios = playerStatus.GetCooldownRatios();
        float[] remaining = playerStatus.GetRemainingCooldowns();
        for (int i = 0; i < skillSlots.Length; i++)
            skillSlots[i].UpdateCooldown(ratios[i], remaining[i]);
    }

    private void OnPlayerDied() { }

    private void OnDestroy()
    {
        playerStatus.OnDead.RemoveListener(OnPlayerDied);
    }
}
