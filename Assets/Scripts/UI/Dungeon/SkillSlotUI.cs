using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;

    public void UpdateCooldown(float ratio, float remaining)
    {
        cooldownOverlay.SetActive(ratio > 0f);

        if (cooldownText != null)
            cooldownText.text = remaining > 0f ? Mathf.Ceil(remaining).ToString() : "";
    }
}