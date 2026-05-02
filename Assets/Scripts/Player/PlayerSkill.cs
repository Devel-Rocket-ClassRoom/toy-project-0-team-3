using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public GameObject Skills;
    private Animator _animator;

    private PlayerInput _input;
    private QSkill _qSkill;
    private WSkill _wSkill;
    private ESkill _eSkill;
    private RSkill _rSkill;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _qSkill = Skills.GetComponent<QSkill>();
        _wSkill = Skills.GetComponent<WSkill>();
        _eSkill = Skills.GetComponent<ESkill>();
        _rSkill = Skills.GetComponent<RSkill>();
    }

    private void Update()
    {
        if (_input.SkillQ)
        {
            _qSkill?.Use();
            _animator.SetTrigger("QSkill");
        }
        if (_input.SkillW)
        {
            _wSkill?.Use();
            _animator.SetTrigger("WSkill");
        }

        if (_input.SkillE)
        {
            _eSkill?.Use();
            _animator.SetTrigger("ESkill");
        }
        if (_input.SkillR)
        {
            _rSkill?.Use();
            _animator.SetTrigger("RSkill");
        }
    }
}
