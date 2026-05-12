using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 10f;

    [SerializeField]
    private float _rotateSpeed = 10f;

    [SerializeField]
    private float _animAcceleration = 2f; // 증가 속도

    [Header("점프")]
    [SerializeField]
    private float _jumpVelocity = 7f;

    [SerializeField]
    private float _gravity = -20f;

    [SerializeField]
    private float _groundCheckDistance = 0.2f;

    [SerializeField]
    private float _groundCheckOriginOffset = 0.1f;

    [SerializeField]
    private LayerMask _groundLayer = ~0;

    private Vector3 _playerDirection;
    public Vector3 PlayerDirection => _playerDirection;

    private float _currentSpeed = 0f;
    private float _verticalVelocity = 0f;
    private bool _wantsJump = false;

    private PlayerInput _playerInput;
    private PlayerAttack _playerAttack;
    private Rigidbody _playerRigidbody;
    private Animator _playerAnimator;
    private PlayerSkill _playerSkill;
    private PlayerStatus _playerStatus;

    private void Awake()
    {
        _playerAttack = GetComponent<PlayerAttack>();
        _playerInput = GetComponent<PlayerInput>();
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerAnimator = GetComponentInChildren<Animator>();
        _playerSkill = GetComponent<PlayerSkill>();
        _playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        _playerDirection = new Vector3(_playerInput.MoveX, 0f, _playerInput.MoveY).normalized;
        Rotate();

        // 점프 입력은 Update에서 캡처 (GetMouseButtonDown은 프레임 단위)
        if (_playerInput.Jump && CanJump())
            _wantsJump = true;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        bool canControl =
            !_playerAttack.IsAttacking
            && !_playerSkill.IsUsingSkill
            && !_playerStatus.IsHit
            && !_playerStatus.IsDead;

        // 수평 이동
        Vector3 horizontalDelta = Vector3.zero;
        if (canControl)
        {
            horizontalDelta = _playerDirection * _moveSpeed * Time.fixedDeltaTime;

            float targetSpeed = _playerDirection.magnitude;
            _currentSpeed = Mathf.MoveTowards(
                _currentSpeed,
                targetSpeed,
                _animAcceleration * Time.fixedDeltaTime
            );
            _playerAnimator.SetFloat("Speed", _currentSpeed);
        }
        else
        {
            _playerAnimator.SetFloat("Speed", 0f);
        }

        // 수직 이동 (중력 + 점프)
        bool grounded = IsGrounded();
        if (_wantsJump)
        {
            _verticalVelocity = _jumpVelocity;
            _wantsJump = false;
        }
        else if (grounded && _verticalVelocity <= 0f)
        {
            _verticalVelocity = -2f; // 지면 밀착용 소량 하향 속도
        }
        else
        {
            _verticalVelocity += _gravity * Time.fixedDeltaTime;
        }

        Vector3 verticalDelta = Vector3.up * _verticalVelocity * Time.fixedDeltaTime;
        _playerRigidbody.MovePosition(_playerRigidbody.position + horizontalDelta + verticalDelta);
    }

    private void Rotate()
    {
        if (
            _playerAttack.IsAttacking
            || _playerSkill.IsUsingSkill
            || _playerStatus.IsHit
            || _playerStatus.IsDead
        )
            return;

        Vector3 direction = new Vector3(_playerInput.MoveX, 0f, _playerInput.MoveY);
        if (direction == Vector3.zero)
            return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            _rotateSpeed * Time.deltaTime
        );
    }

    private bool CanJump()
    {
        if (
            _playerAttack.IsAttacking
            || _playerSkill.IsUsingSkill
            || _playerStatus.IsHit
            || _playerStatus.IsDead
        )
            return false;

        return IsGrounded();
    }

    private bool IsGrounded()
    {
        Vector3 origin = _playerRigidbody.position + Vector3.up * _groundCheckOriginOffset;
        return Physics.Raycast(
            origin,
            Vector3.down,
            _groundCheckOriginOffset + _groundCheckDistance,
            _groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }
}
