using System.Collections;
using UnityEngine;

public class WSkill : SkillBase
{
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashDuration = 0.3f;
    [SerializeField] private float _rotateSpeed = 5f;

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
        _playerStatus.SetInvincible(true);

        float elapsed = 0f;
        Vector3 currentDir = Direction != Vector3.zero
            ? Direction.normalized
            : _rigidbody.transform.forward;

        while (elapsed < _dashDuration)
        {
            Vector3 targetDir = Direction != Vector3.zero
                ? Direction.normalized
                : currentDir;

            currentDir = Vector3.Slerp(currentDir, targetDir, _rotateSpeed * Time.fixedDeltaTime).normalized;

            _rigidbody.transform.rotation = Quaternion.LookRotation(currentDir);
            _rigidbody.MovePosition(_rigidbody.position + currentDir * _dashSpeed * Time.fixedDeltaTime);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Direction = Vector3.zero;
        _playerStatus.SetInvincible(false);
        _playerSkill.OnSkillEnd();
        _playerSkill.WToMove();
    }
}