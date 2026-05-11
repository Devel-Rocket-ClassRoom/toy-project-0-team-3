using UnityEngine;

public class QSkillHitBox : MonoBehaviour
{
    private QSkill _qSkill;

    public void Init(QSkill qSkill)
    {
        _qSkill = qSkill;
    }

    private void OnTriggerEnter(Collider other)
    {
        _qSkill.OnHit(other);
    }
}
