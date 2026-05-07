using System.Collections;
using UnityEngine;

public class ESkill : SkillBase
{
    [SerializeField] private float _jumpHeight = 3f;
    [SerializeField] private float _forwardDistance = 3f;
    [SerializeField] private float _jumpDuration = 0.5f;
    [SerializeField] private float _smashDuration = 0.2f;

    private PlayerSkill _playerSkill;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        Cooldown = 3f;
        _playerSkill = GetComponentInParent<PlayerSkill>();
        _rigidbody = GetComponentInParent<Rigidbody>();
    }

    protected override void OnUse() { }

    public void JumpShot()
    {
        StartCoroutine(JumpCoroutine());
    }

    public void GroundSmash()
    {
        StartCoroutine(SmashCoroutine());
    }

    private IEnumerator JumpCoroutine()
    {
        Vector3 startPos = _rigidbody.position;
        Vector3 endPos = startPos + _rigidbody.transform.forward * _forwardDistance;

        float elapsed = 0f;
        while (elapsed < _jumpDuration)
        {
            float t = elapsed / _jumpDuration;
            float height = Mathf.Sin(t * Mathf.PI) * _jumpHeight;

            Vector3 nextPos = Vector3.Lerp(startPos, endPos, t);
            nextPos.y = startPos.y + height;

            _rigidbody.MovePosition(nextPos);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _rigidbody.MovePosition(endPos);
    }

    private IEnumerator SmashCoroutine()
    {
        Vector3 startPos = _rigidbody.position;
        // 현재 위치에서 바닥(y=0 또는 시작높이)으로 내려찍기
        Vector3 endPos = new Vector3(startPos.x, 0f, startPos.z);

        float elapsed = 0f;
        while (elapsed < _smashDuration)
        {
            float t = elapsed / _smashDuration;

            // 처음엔 느리게 나중엔 빠르게 (가속하며 내려찍는 느낌)
            float easedT = t * t;

            Vector3 nextPos = Vector3.Lerp(startPos, endPos, easedT);
            _rigidbody.MovePosition(nextPos);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _rigidbody.MovePosition(endPos);
    }
}