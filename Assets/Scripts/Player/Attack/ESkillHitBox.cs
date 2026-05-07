using UnityEngine;

public class ESkillHitBox : MonoBehaviour
{
    private ESkill _eSkill;

    public void Init(ESkill eSkill)
    {
        _eSkill = eSkill;
    }

    private void OnTriggerEnter(Collider other)
    {
        _eSkill.OnHit(other);
    }
}