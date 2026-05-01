using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public GameObject Skills;

    private PlayerInput _input;
    private QSkill _qSkill;
    private WSkill _wSkill;
    private ESkill _eSkill;
    private RSkill _rSkill;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _qSkill = Skills.GetComponent<QSkill>();
        _wSkill = Skills.GetComponent<WSkill>();
        _eSkill = Skills.GetComponent<ESkill>();
        _rSkill = Skills.GetComponent<RSkill>();
    }

    private void Update()
    {
        if (_input.SkillQ) _qSkill?.Use();
        if (_input.SkillW) _wSkill?.Use();
        if (_input.SkillE) _eSkill?.Use();
        if (_input.SkillR) _rSkill?.Use();
    }
}
