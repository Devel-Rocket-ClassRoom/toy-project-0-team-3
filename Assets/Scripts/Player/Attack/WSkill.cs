using System.Collections;
using UnityEngine;

public class WSkill : SkillBase
{
    [SerializeField] private float _dashSpeed = 8f;
    [SerializeField] private float _dashDuration = 0.3f;
    [SerializeField] private float _rotateSpeed = 5f; // 높을수록 빠르게 회전

    [SerializeField] private WSkillWall _wSkillWall;

    public float DashSpeed => _dashSpeed;

    public Vector3 Direction { get; set; } = Vector3.zero;

    private PlayerSkill _playerSkill;
    private PlayerStatus _playerStatus;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        Cooldown = 6f;
        _playerSkill = GetComponentInParent<PlayerSkill>();
        _playerStatus = GetComponentInParent<PlayerStatus>();
        _rigidbody = GetComponentInParent<Rigidbody>();
    }

    protected override void OnUse()
    {
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        _wSkillWall.EnableWall();
        _playerStatus.SetInvincible(true);

        float elapsed = 0f;

        // 시작 방향 고정
        Vector3 currentDir = Direction != Vector3.zero
            ? Direction.normalized
            : _rigidbody.transform.forward;

        while (elapsed < _dashDuration)
        {
            // 목표 방향
            Vector3 targetDir = Direction != Vector3.zero
                ? Direction.normalized
                : currentDir;

            // 보간으로 천천히 방향 전환
            currentDir = Vector3.Slerp(currentDir, targetDir, _rotateSpeed * Time.fixedDeltaTime).normalized;

            // 회전 적용
            _rigidbody.transform.rotation = Quaternion.LookRotation(currentDir);

            _rigidbody.MovePosition(_rigidbody.position + currentDir * _dashSpeed * Time.fixedDeltaTime);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Direction = Vector3.zero;
        _wSkillWall.DisableWall();
        _playerStatus.SetInvincible(false);
        _playerSkill.OnSkillEnd();
        _playerSkill.WToMove();
    }
}