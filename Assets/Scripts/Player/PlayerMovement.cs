using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotateSpeed = 10f;
    [SerializeField] private float _animAcceleration = 2f; // 증가 속도

    private float _currentSpeed = 0f;

    private PlayerInput _playerInput;
    private PlayerAttack _playerAttack;
    private Rigidbody _playerRigidbody;
    private Animator _playerAnimator;

    private void Awake()
    {
        _playerAttack = GetComponent<PlayerAttack>();
        _playerInput = GetComponent<PlayerInput>();
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Rotate();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (_playerAttack.IsAttacking)
        {
            _playerAnimator.SetFloat("Speed", 0f);
            return;
        }

        Vector3 direction = new Vector3(_playerInput.MoveX, 0f, _playerInput.MoveY).normalized;
        _playerRigidbody.MovePosition(_playerRigidbody.position + direction * _moveSpeed * Time.fixedDeltaTime);

        float targetSpeed = direction.magnitude;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _animAcceleration * Time.fixedDeltaTime);
        _playerAnimator.SetFloat("Speed", _currentSpeed);
    }

    private void Rotate()
    {
        if (_playerAttack.IsAttacking) return;

        Vector3 direction = new Vector3(_playerInput.MoveX, 0f, _playerInput.MoveY);
        if (direction == Vector3.zero) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            _rotateSpeed * Time.deltaTime
        );
    }
}