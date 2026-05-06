using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public GameObject Skills;
    private Animator _animator;
    private PlayerAttack _playerAttack;
    private PlayerInput _input;
    private PlayerMovement _playerMovement;
    private PlayerStatus _playerStatus;
    private QSkill _qSkill;
    private WSkill _wSkill;
    private ESkill _eSkill;
    private RSkill _rSkill;

    public bool IsUsingSkill { get; private set; }

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _playerAttack = GetComponent<PlayerAttack>();
        _playerMovement = GetComponent<PlayerMovement>();
        _qSkill = Skills.GetComponent<QSkill>();
        _wSkill = Skills.GetComponent<WSkill>();
        _eSkill = Skills.GetComponent<ESkill>();
        _rSkill = Skills.GetComponent<RSkill>();
        _playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (_playerStatus.IsHit || _playerStatus.IsDead) return;

        if (_input.SkillQ && _qSkill.CanUse && !IsUsingSkill)
        {
            IsUsingSkill = true;
            _qSkill?.Use();
            _qSkill.Direction = _playerMovement.PlayerDirection;
            _animator.SetTrigger("QSkill");
        }

        if (_input.SkillW && _qSkill.CanUse && !IsUsingSkill)
        {
            IsUsingSkill = true;
            _wSkill?.Use();
            _animator.SetTrigger("WSkill");
        }

        if (_input.SkillE && _qSkill.CanUse && !IsUsingSkill)
        {
            IsUsingSkill = true;
            _eSkill?.Use();
            _animator.SetTrigger("ESkill");
        }

        if (_input.SkillR && _qSkill.CanUse && !IsUsingSkill)
        {
            IsUsingSkill = true;
            _rSkill?.Use();
            _animator.SetTrigger("RSkill");
        }
    }

    public float[] GetRemainingCooldowns()
    {
        return new float[]
        {
        _qSkill.RemainingCooldown,
        _wSkill.RemainingCooldown,
        _eSkill.RemainingCooldown,
        _rSkill.RemainingCooldown,
        };
    }

    public float[] GetCooldownRatios()
    {
        return new float[]
        {
            _qSkill.CooldownRatio,
            _wSkill.CooldownRatio,
            _eSkill.CooldownRatio,
            _rSkill.CooldownRatio,
        };
    }

    public void QDash()
    {
        if (_qSkill.Direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_qSkill.Direction);

        _qSkill.OnDashStart();
    }

    public void OnSkillEnd()
    {
        IsUsingSkill = false;
        _playerAttack.ForceReset();
    }
}
