using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator _animator;
    private PlayerInput _playerInput;

    private int _comboStep = 0;
    private bool _canCombo = false;
    private bool _inputBuffered = false;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (_playerInput.Attack)
        {
            if (_comboStep == 0)
                StartAttack();
            else if (_canCombo)
                NextCombo();
            else
                _inputBuffered = true; // 타이밍 놓쳤을 때 버퍼에 저장
        }
    }

    private void StartAttack()
    {
        _comboStep = 1;
        _animator.SetInteger("ComboStep", _comboStep);
        _animator.SetTrigger("Attack");
    }

    private void NextCombo()
    {
        _comboStep++;
        _inputBuffered = false;
        _canCombo = false;
        _animator.SetInteger("ComboStep", _comboStep);
        _animator.SetTrigger("Attack");
    }

    // Animation Event - 콤보 입력 가능 구간 시작 (애니메이션 중간에 설정)
    public void OnComboWindowOpen()
    {
        _canCombo = true;
        if (_inputBuffered) NextCombo();
    }

    // Animation Event - 애니메이션 끝날 때 호출
    public void OnAttackEnd()
    {
        if (_comboStep >= 3 || !_inputBuffered)
            ResetCombo();
    }

    private void ResetCombo()
    {
        _comboStep = 0;
        _canCombo = false;
        _inputBuffered = false;
    }
}
