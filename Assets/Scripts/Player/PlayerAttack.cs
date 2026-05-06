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
    [SerializeField] private float _attackDashSpeed = 8f;
    [SerializeField] private float _attackDashDuration = 0.2f;
    private Coroutine _coDash = null;

    [SerializeField] private Sword _sword;


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
        if (_playerStatus.IsHit || _playerStatus.IsDead) return;

        if (_playerInput.Attack)
        {
            //Debug.Log($"클릭 {_comboStep} {_canCombo}");
            if (_comboStep == 0)
                StartAttack();
            else if (_canCombo)
                NextCombo();
        }
    }

    private void StartAttack()
    {
        if (_coDash != null)
        {
            StopCoroutine(_coDash);
        }
        _isAttacking = true;
        _comboStep = 1;
        _animator.SetInteger("ComboStep", _comboStep);
        _animator.SetTrigger("Attack");
        _coDash = StartCoroutine(DashCoroutine());
    }

    private void NextCombo()
    {
        if (_coDash != null)
        {
            StopCoroutine(_coDash);
        }

        _comboStep++;
        _canCombo = false;
        _isAttacking = true;
        _animator.SetInteger("ComboStep", _comboStep);
        _animator.SetTrigger("Attack");
        _coDash = StartCoroutine(DashCoroutine());
    }

    // Animation Event - 콤보 입력 가능 구간 시작 (애니메이션 중간에 설정)
    public void OnComboWindowOpen()
    {
        //Debug.Log($"윈도우 {_comboStep} {_canCombo}");
        _canCombo = true;
    }

    // Animation Event - 애니메이션 끝날 때 호출
    public void OnAttackEnd()
    {
        //Debug.Log($"OnAttack {_comboStep} {_canCombo}");
        ResetCombo();

        _isAttacking = false;
        _sword.DisableHit();
        //Debug.Log("OnAttackHitEnd 호출됨");
    }

    private void ResetCombo()
    {
        //Debug.Log("리셋");
        _comboStep = 0;
        _canCombo = false;

        if (_coDash != null)
        {
            StopCoroutine(_coDash);
            _coDash = null;
        }

    }

    private IEnumerator DashCoroutine()
    {
        Vector3 dashDirection = _playerMovement.PlayerDirection != Vector3.zero
       ? _playerMovement.PlayerDirection
       : transform.forward;

        Debug.Log($"대시 {_comboStep}");

        if (dashDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dashDirection);

        float elapsed = 0f;
        while (elapsed < _attackDashDuration)
        {
            _rigidbody.MovePosition(_rigidbody.position + dashDirection * _attackDashSpeed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        _coDash = null;
    }

    public void OnAttackHitStart()
    {
        //Debug.Log("OnAttackHitStart 호출됨");
        _sword.EnableHit();
    }

    public void ForceReset()
    {
        _isAttacking = false;
        _comboStep = 0;
        _canCombo = false;
        if (_coDash != null)
        {
            StopCoroutine(_coDash);
            _coDash = null;
        }
        _sword.DisableHit();
    }
}
