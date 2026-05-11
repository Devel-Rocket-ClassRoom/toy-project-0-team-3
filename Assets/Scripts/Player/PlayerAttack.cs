using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator _animator;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PlayerStatus _playerStatus;

    private int _comboStep = 0;
    private bool _canCombo = false;
    private bool _isAttacking = false;
    public bool IsAttacking => _isAttacking;

    private Rigidbody _rigidbody;

    [SerializeField]
    private float _attackDashSpeed = 8f;

    [SerializeField]
    private float _attackDashDuration = 0.2f;
    private Coroutine _coDash = null;

    [SerializeField]
    private Sword _sword;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (_playerStatus.IsHit || _playerStatus.IsDead)
            return;

        if (_playerInput.Attack)
        {
            if (_comboStep == 0 || _canCombo)
                BasicAttack();
        }
    }

    private void BasicAttack()
    {
        if (_coDash != null)
            StopCoroutine(_coDash);

        _comboStep = (_comboStep % 3) + 1;
        _canCombo = false;
        _isAttacking = true;
        _animator.ResetTrigger("Attack");
        _animator.SetInteger("ComboStep", _comboStep);
        _animator.SetTrigger("Attack");
    }

    public void OnAttackDash()
    {
        if (_coDash != null)
            StopCoroutine(_coDash);
        _coDash = StartCoroutine(DashCoroutine());
    }

    public void OnComboWindowOpen()
    {
        _canCombo = true;
    }

    public void OnAttackEnd()
    {
        if (_comboStep == 0)
            return;
        ForceReset();
    }

    private IEnumerator DashCoroutine()
    {
        Vector3 dashDirection =
            _playerMovement.PlayerDirection != Vector3.zero
                ? _playerMovement.PlayerDirection
                : transform.forward;

        if (dashDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dashDirection);

        float elapsed = 0f;
        while (elapsed < _attackDashDuration)
        {
            _rigidbody.MovePosition(
                _rigidbody.position + dashDirection * _attackDashSpeed * Time.fixedDeltaTime
            );
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        _coDash = null;
    }

    public void OnAttackHitStart()
    {
        _sword.EnableHit();
    }

    public void ForceReset()
    {
        _isAttacking = false;
        _comboStep = 0;
        _canCombo = false;
        _animator.ResetTrigger("Attack");
        _sword.DisableHit();

        if (_coDash != null)
        {
            StopCoroutine(_coDash);
            _coDash = null;
        }
    }
}
