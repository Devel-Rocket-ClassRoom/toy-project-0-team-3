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

    [SerializeField]
    private float _qManaCost = 20f;

    [SerializeField]
    private float _wManaCost = 30f;

    [SerializeField]
    private float _eManaCost = 30f;

    [SerializeField]
    private float _rManaCost = 50f;

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
        if (_playerStatus.IsHit || _playerStatus.IsDead)
            return;

        if (
            _input.SkillQ
            && _qSkill.CanUse
            && !IsUsingSkill
            && _playerStatus.CurrentMana >= _qManaCost
        )
        {
            _playerStatus.SetInvincible(true);
            IsUsingSkill = true;
            _playerStatus.UseMana(_qManaCost); // 추가
            _qSkill?.Use();
            _qSkill.Direction = _playerMovement.PlayerDirection;
            _animator.SetTrigger("QSkill");
        }

        if (
            _input.SkillW
            && _wSkill.CanUse
            && !IsUsingSkill
            && _playerStatus.CurrentMana >= _wManaCost
        )
        {
            _playerStatus.SetInvincible(true);
            IsUsingSkill = true;
            _playerStatus.UseMana(_wManaCost); // 추가
            _wSkill?.Use();
            _wSkill.Direction = _playerMovement.PlayerDirection;
            _animator.SetTrigger("WSkill");
        }

        if (
            _input.SkillE
            && _eSkill.CanUse
            && !IsUsingSkill
            && _playerStatus.CurrentMana >= _eManaCost
        )
        {
            _playerStatus.SetInvincible(true);
            IsUsingSkill = true;
            _playerStatus.UseMana(_eManaCost); // 추가
            _eSkill?.Use();
            _animator.SetTrigger("ESkill");
        }

        if (
            _input.SkillR
            && _rSkill.CanUse
            && !IsUsingSkill
            && _playerStatus.CurrentMana >= _rManaCost
        )
        {
            _playerStatus.SetInvincible(true);
            IsUsingSkill = true;
            _playerStatus.UseMana(_rManaCost); // 추가
            _rSkill?.Use();
            _animator.SetTrigger("RSkill");
        }

        if (IsUsingSkill)
        {
            _wSkill.Direction = _playerMovement.PlayerDirection;
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

    public void WToMove()
    {
        _animator.SetTrigger("WToMove");
    }

    public void EJumpShot()
    {
        _eSkill.JumpShot();
    }

    public void EGroundSmash()
    {
        _eSkill.GroundSmash();
    }

    public void OnSkillEnd()
    {
        _playerStatus.SetInvincible(false);
        IsUsingSkill = false;
        _playerAttack.ForceReset();
    }
}
