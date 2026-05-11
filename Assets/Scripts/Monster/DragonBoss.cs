using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonBoss : LivingEntity
{
    [SerializeField]
    private GameObject portalPrefab;

    private enum BossState
    {
        Idle,
        Chase,
        GroundAttack,
        TakeOff,
        AirIdle,
        AirBreathFire,
        Landing,
        HitReaction,
        Dead,
    }

    [System.Serializable]
    private class AttackData
    {
        public string triggerName;
        public string stateName;
        public HitBox hitBox;
        public float damage;
        public bool applyBurn;
        public Vector2 hitWindow;
        public int TriggerHash => Animator.StringToHash(triggerName);
    }

    private readonly int HashLocomotion = Animator.StringToHash("locomotion");
    private readonly int HashFlyGlide = Animator.StringToHash("flyGlide");
    private readonly int HashIdleTakeoff = Animator.StringToHash("idleTakeoff");
    private readonly int HashIdleLand = Animator.StringToHash("idleLand");
    private readonly int HashGotHit1 = Animator.StringToHash("gotHit1");
    private readonly int HashDeath = Animator.StringToHash("death");
    private readonly int HashFlyDeath = Animator.StringToHash("flyDeath");
    private const float LocomotionIdle = 0.5f;
    private const float LocomotionRun = 1f;

    [Header("Target")]
    [SerializeField]
    private string playerTag = "Player";

    [SerializeField]
    private LayerMask targetLayers = ~0;

    [Header("Movement")]
    [SerializeField]
    private float activationRange = 25f;

    [SerializeField]
    private float attackRange = 10f;

    [SerializeField]
    private float stopDistance = 7.5f;

    [SerializeField]
    private float groundMoveSpeed = 4f;

    [SerializeField]
    private float airMoveSpeed = 8f;

    [SerializeField]
    private float turnSpeed = 360f;

    [Header("Air Movement")]
    [SerializeField]
    private float airHeight = 6f;

    [SerializeField]
    private float airIdleDuration = 2f;

    [SerializeField]
    private AnimationCurve takeOffHeightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    private float landingSpeedMultiplier = 2f;

    [Header("Phase")]
    [SerializeField]
    private float phase2HealthRatio = 0.5f;

    [Header("Burn")]
    [SerializeField]
    private float burnDuration = 3f;

    [SerializeField]
    private float burnTickDamage = 2f;

    [Header("Attack Probability")]
    [SerializeField]
    private int specialAttackInterval = 5;

    [SerializeField]
    private float attack1Near = 0.25f;

    [SerializeField]
    private float attack1Far = 0.75f;

    [SerializeField]
    private float attack2Near = 0.75f;

    [SerializeField]
    private float attack2Far = 0.15f;

    [SerializeField]
    private float baseSpecialWeight = 0.08f;

    [SerializeField]
    private float specialChanceIncrease = 0.08f;

    [SerializeField]
    private float maxSpecialWeight = 0.55f;

    [SerializeField]
    private float attackCooldown = 1.5f;

    [Header("Attack Boxes")]
    [SerializeField]
    private AttackData attack1 = new AttackData
    {
        triggerName = "attack1",
        stateName = "Attack01",
        damage = 18f,
        applyBurn = false,
        hitWindow = new Vector2(0.35f, 0.55f),
    };

    [SerializeField]
    private AttackData attack2 = new AttackData
    {
        triggerName = "attack2",
        stateName = "Attack02",
        damage = 24f,
        applyBurn = false,
        hitWindow = new Vector2(0.45f, 0.65f),
    };

    [SerializeField]
    private AttackData groundBreath = new AttackData
    {
        triggerName = "breatheFire",
        stateName = "BreatheFire",
        damage = 12f,
        applyBurn = true,
        hitWindow = new Vector2(0.25f, 0.85f),
    };

    [SerializeField]
    private AttackData airBreath = new AttackData
    {
        triggerName = "flyBreatheFire",
        stateName = "FlyBreatheFire",
        damage = 16f,
        applyBurn = true,
        hitWindow = new Vector2(0.20f, 0.90f),
    };

    [Header("Animator")]
    [SerializeField]
    private int animatorLayerIndex = 0;

    [SerializeField]
    private string takeOffStateName = "Idle Takeoff";

    [SerializeField]
    private string landingStateName = "Idle Landing";

    [SerializeField]
    private string gotHitStateName = "Hit01";

    [Header("Hit Reaction / Death")]
    [SerializeField]
    private float hitReactionStepRatio = 0.05f;

    [SerializeField]
    private float destroyDelay = 5f;

    [Header("Gizmos")]
    [SerializeField]
    private bool drawDetectionGizmos = true;

    [Header("Debug")]
    [SerializeField]
    private BossState currentState = BossState.Idle;

    private Transform player;
    private Animator anim;
    private Rigidbody rigidBody;
    private AttackData[] attacks;
    private Coroutine currentRoutine;
    private bool isPhase2;
    private bool isAirborne;
    private bool isActionLocked;
    private bool isChangingPhase;
    private bool phase2Requested;
    private int normalAttackCount;
    private float groundY;
    private Vector3 bossPosition;
    private Quaternion bossRotation;
    private float nextAttackTime;
    private float nextHitReactionHealth;
    private readonly HashSet<LivingEntity> damagedTargets = new HashSet<LivingEntity>();

    protected override void OnEnable()
    {
        base.OnEnable();

        anim = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        attacks = new[] { attack1, attack2, groundBreath, airBreath };

        SetupExternalDrivers();
        FindPlayer();

        groundY = transform.position.y;
        bossPosition = transform.position;
        bossRotation = transform.rotation;

        ApplyPose();

        isPhase2 = false;
        isAirborne = false;
        isActionLocked = false;
        isChangingPhase = false;
        phase2Requested = false;

        normalAttackCount = 0;
        nextAttackTime = 0f;
        nextHitReactionHealth = startingHealth * (1f - hitReactionStepRatio);
        currentState = BossState.Idle;

        ClearAttackData();
        PlayIdle();
    }

    protected override void Update()
    {
        base.Update();

        if (IsDead || isActionLocked)
        {
            return;
        }

        if (player == null)
        {
            FindPlayer();
            if (player == null)
            {
                return;
            }
        }

        if (currentState == BossState.Idle)
        {
            UpdateIdle();
        }
        else if (currentState == BossState.Chase)
        {
            UpdateChase();
        }
    }

    private void LateUpdate()
    {
        if (!IsDead)
            ApplyPose();
    }

    private void OnAnimatorMove()
    {
        // Root Motion이 Transform을 되돌리는 것을 막기 위해 비워둔다.
    }

    private void SetupExternalDrivers()
    {
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }
        if (rigidBody == null)
        {
            return;
        }
        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }

    private void UpdateIdle()
    {
        PlayIdle();

        if (FlatDistance(bossPosition, player.position) <= activationRange)
        {
            currentState = BossState.Chase;
        }
    }

    private void UpdateChase()
    {
        if (!isPhase2 && Health <= startingHealth * phase2HealthRatio)
        {
            RequestPhase2Transition();

            return;
        }
        float dist = FlatDistance(bossPosition, player.position);

        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            StartSelectedAttack();

            return;
        }

        if (dist > stopDistance)
        {
            MoveTowardPlayer();
        }
        else
        {
            PlayIdle();
            FaceTarget(player.position);
        }
    }

    private void StartSelectedAttack()
    {
        int index = SelectAttackIndex();

        if (index == 2)
        {
            normalAttackCount = 0;
            StartAction(isPhase2 ? AirSequence(false) : GroundAttack(groundBreath));
            return;
        }

        normalAttackCount++;
        StartAction(GroundAttack(index == 0 ? attack1 : attack2));
    }

    private int SelectAttackIndex()
    {
        if (normalAttackCount >= specialAttackInterval)
        {
            return 2;
        }

        float distance01 = Mathf.Clamp01(
            FlatDistance(bossPosition, player.position) / Mathf.Max(attackRange, 0.01f)
        );
        float w1 = Mathf.Lerp(attack1Near, attack1Far, distance01);
        float w2 = Mathf.Lerp(attack2Near, attack2Far, distance01);
        float ws = Mathf.Clamp(
            baseSpecialWeight + normalAttackCount * specialChanceIncrease,
            0f,
            maxSpecialWeight
        );
        float random = Random.value * (w1 + w2 + ws);

        if (random < w1)
        {
            return 0;
        }

        return random - w1 < w2 ? 1 : 2;
    }

    private IEnumerator GroundAttack(AttackData attack)
    {
        BeginAction(BossState.GroundAttack);
        PlayIdle();
        FaceTarget(player.position, true);

        Vector3 lockedPosition = bossPosition;
        lockedPosition.y = groundY;
        Quaternion lockedRotation = bossRotation;

        anim.SetTrigger(attack.TriggerHash);

        yield return WaitForState(attack.stateName);

        bool active = false;

        while (TryGetStateTime(attack.stateName, out float t))
        {
            SetPose(lockedPosition, lockedRotation);
            active = UpdateAttackWindow(attack, t, active);

            if (t >= 1f)
            {
                break;
            }

            yield return null;
        }

        FinishAction();
    }

    private IEnumerator AirSequence(bool isPhaseTransition)
    {
        isChangingPhase = isPhaseTransition;

        yield return TakeOff();
        yield return AirIdle();
        yield return AirBreathFire();
        yield return Landing();

        isChangingPhase = false;

        FinishAction();
    }

    private IEnumerator TakeOff()
    {
        BeginAction(BossState.TakeOff);

        isAirborne = true;

        PlayIdle();
        FaceTarget(player.position, true);

        float startY = bossPosition.y;
        float targetY = groundY + airHeight;

        anim.SetTrigger(HashIdleTakeoff);

        yield return WaitForState(takeOffStateName);

        while (TryGetStateTime(takeOffStateName, out float t))
        {
            SetY(Mathf.Lerp(startY, targetY, takeOffHeightCurve.Evaluate(Mathf.Clamp01(t))));

            if (t >= 1f)
            {
                break;
            }

            yield return null;
        }

        SetY(targetY);
        anim.SetTrigger(HashFlyGlide);
    }

    private IEnumerator AirIdle()
    {
        currentState = BossState.AirIdle;

        float timer = 0f;

        while (timer < airIdleDuration)
        {
            FaceTarget(player.position);
            SetY(groundY + airHeight);
            timer += Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator AirBreathFire()
    {
        BeginAction(BossState.AirBreathFire);
        Vector3 direction = FlatDirection(bossPosition, player.position);

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = bossRotation * Vector3.forward;
        }

        direction.Normalize();
        FaceDirection(direction, true);
        Quaternion lockedRotation = bossRotation;
        anim.SetTrigger(airBreath.TriggerHash);

        yield return WaitForState(airBreath.stateName);

        bool active = false;

        while (TryGetStateTime(airBreath.stateName, out float t))
        {
            bossRotation = lockedRotation;
            Vector3 nextPosition = bossPosition + direction * airMoveSpeed * Time.deltaTime;
            nextPosition.y = groundY + airHeight;

            SetPose(nextPosition, lockedRotation);

            active = UpdateAttackWindow(airBreath, t, active);

            if (t >= 1f)
            {
                break;
            }

            yield return null;
        }
    }

    private IEnumerator Landing()
    {
        currentState = BossState.Landing;

        FaceTarget(player.position, true);

        float startY = bossPosition.y;

        anim.ResetTrigger(airBreath.TriggerHash);
        anim.SetTrigger(HashIdleLand);

        yield return null;
        yield return WaitForState(landingStateName);

        while (TryGetStateTime(landingStateName, out float t))
        {
            FaceTarget(player.position);
            SetY(Mathf.Lerp(startY, groundY, Mathf.Clamp01(t) * landingSpeedMultiplier));
            if (t >= 1f)
                break;
            yield return null;
        }

        SetY(groundY);
        isAirborne = false;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead)
        {
            return;
        }

        base.OnDamage(damage, hitPoint, hitNormal);

        if (IsDead)
        {
            return;
        }

        if (!isPhase2 && Health <= startingHealth * phase2HealthRatio)
        {
            RequestPhase2Transition();

            return;
        }
        if (!CrossedHitReactionStep(Health))
        {
            return;
        }

        if (ShouldDelayHitReaction())
        {
            return;
        }

        StopAction();
        StartAction(HitReaction());
    }

    private bool CrossedHitReactionStep(float currentHealth)
    {
        bool crossed = false;
        float step = startingHealth * hitReactionStepRatio;

        while (currentHealth <= nextHitReactionHealth && nextHitReactionHealth > 0f)
        {
            crossed = true;
            nextHitReactionHealth -= step;
        }

        return crossed;
    }

    private bool ShouldDelayHitReaction()
    {
        return isAirborne
            || isChangingPhase
            || currentState == BossState.TakeOff
            || currentState == BossState.AirIdle
            || currentState == BossState.AirBreathFire
            || currentState == BossState.Landing;
    }

    private IEnumerator HitReaction()
    {
        BeginAction(BossState.HitReaction);
        PlayIdle();
        ResetCombatTriggers();

        anim.SetTrigger(HashGotHit1);

        yield return WaitForState(gotHitStateName);

        while (TryGetStateTime(gotHitStateName, out float t))
        {
            PlayIdle();

            if (t >= 1f)
            {
                break;
            }

            yield return null;
        }
        FinishAction();
    }

    private void RequestPhase2Transition()
    {
        if (isPhase2 || phase2Requested)
        {
            return;
        }

        phase2Requested = true;

        if (!isActionLocked)
        {
            StartPhase2TransitionNow();
        }
    }

    private void StartPhase2TransitionNow()
    {
        phase2Requested = false;
        isPhase2 = true;
        normalAttackCount = 0;

        StopAction();
        StartAction(AirSequence(true));
    }

    public override void Die()
    {
        if (IsDead)
        {
            return;
        }

        base.Die();

        StopAction();
        ClearAttackData();

        currentState = BossState.Dead;
        isActionLocked = true;

        if (anim != null)
        {
            anim.SetTrigger(isAirborne ? HashFlyDeath : HashDeath);
        }

        Destroy(gameObject, destroyDelay);
        Instantiate(portalPrefab, transform.position, Quaternion.identity);
    }

    private void BeginAction(BossState state)
    {
        currentState = state;
        isActionLocked = true;

        ClearAttackData();
    }

    private void StartAction(IEnumerator routine)
    {
        StopAction();
        currentRoutine = StartCoroutine(routine);
    }

    private void StopAction()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        ClearAttackData();
        isActionLocked = false;
    }

    private void FinishAction()
    {
        ClearAttackData();

        currentRoutine = null;
        isActionLocked = false;
        nextAttackTime = Time.time + attackCooldown;

        if (phase2Requested)
        {
            StartPhase2TransitionNow();
            return;
        }

        currentState = BossState.Chase;
    }

    private bool UpdateAttackWindow(AttackData attack, float normalizedTime, bool active)
    {
        if (attack == null)
        {
            return false;
        }

        if (!active && normalizedTime >= attack.hitWindow.x)
        {
            active = true;
            SetAttackHitBoxEnabled(attack, true);
        }

        if (active)
        {
            ApplyDamageFromHitBox(attack);
        }

        if (active && normalizedTime > attack.hitWindow.y)
        {
            active = false;
            SetAttackHitBoxEnabled(attack, false);
        }

        return active;
    }

    private void ApplyDamageFromHitBox(AttackData attack)
    {
        if (attack.hitBox == null)
        {
            return;
        }

        BoxCollider[] boxes = attack.hitBox.GetComponentsInChildren<BoxCollider>(true);

        foreach (BoxCollider box in boxes)
        {
            if (box == null)
            {
                continue;
            }

            Vector3 center = box.transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
            Collider[] colliders = Physics.OverlapBox(
                center,
                halfExtents,
                box.transform.rotation,
                targetLayers,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider targetCollider in colliders)
            {
                LivingEntity target =
                    targetCollider != null
                        ? targetCollider.GetComponentInParent<LivingEntity>()
                        : null;
                if (
                    target == null
                    || target == this
                    || target.IsDead
                    || damagedTargets.Contains(target)
                )
                {
                    continue;
                }

                bool isPlayer =
                    target.CompareTag(playerTag)
                    || targetCollider.CompareTag(playerTag)
                    || target.transform.root.CompareTag(playerTag);

                if (!isPlayer)
                {
                    continue;
                }

                damagedTargets.Add(target);

                Vector3 hitPoint = targetCollider.ClosestPoint(center);
                Vector3 hitNormal = FlatDirection(bossPosition, target.transform.position);

                if (hitNormal.sqrMagnitude <= 0.001f)
                {
                    hitNormal = bossRotation * Vector3.forward;
                }

                target.OnDamage(attack.damage, hitPoint, hitNormal.normalized);

                if (attack.applyBurn)
                {
                    target.ApplyStatusEffect(StatusFlags.Burn, burnDuration, burnTickDamage);
                }
            }
        }
    }

    private void SetAttackHitBoxEnabled(AttackData attack, bool enabled)
    {
        if (attack == null || attack.hitBox == null)
        {
            return;
        }

        Collider[] colliders = attack.hitBox.GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
        {
            collider.enabled = enabled;
        }

        attack.hitBox.Colliders.Clear();
    }

    private void DisableAllAttackHitBoxes()
    {
        if (attacks == null)
        {
            return;
        }

        foreach (AttackData attack in attacks)
        {
            SetAttackHitBoxEnabled(attack, false);
        }
    }

    private void ClearAttackData()
    {
        damagedTargets.Clear();

        DisableAllAttackHitBoxes();
    }

    private void ResetCombatTriggers()
    {
        if (anim == null)
        {
            return;
        }

        if (attacks != null)
        {
            foreach (AttackData attack in attacks)
            {
                if (attack != null)
                {
                    anim.ResetTrigger(attack.TriggerHash);
                }
            }
        }

        anim.ResetTrigger(HashIdleTakeoff);
        anim.ResetTrigger(HashIdleLand);
    }

    private IEnumerator WaitForState(string stateName)
    {
        float timeout = 2f;

        while (timeout > 0f)
        {
            AnimatorStateInfo info = anim.IsInTransition(animatorLayerIndex)
                ? anim.GetNextAnimatorStateInfo(animatorLayerIndex)
                : anim.GetCurrentAnimatorStateInfo(animatorLayerIndex);

            if (IsState(info, stateName) && !anim.IsInTransition(animatorLayerIndex))
            {
                yield break;
            }

            timeout -= Time.deltaTime;

            yield return null;
        }

        Debug.LogWarning(
            $"[{gameObject.name}] Animator state '{stateName}'에 진입하지 못했습니다."
        );
    }

    private bool TryGetStateTime(string stateName, out float normalizedTime)
    {
        normalizedTime = 0f;

        if (anim == null || anim.IsInTransition(animatorLayerIndex))
        {
            return false;
        }

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(animatorLayerIndex);

        if (!IsState(info, stateName))
        {
            return false;
        }

        normalizedTime = info.normalizedTime;

        return true;
    }

    private bool IsState(AnimatorStateInfo info, string stateName)
    {
        return info.IsName(stateName) || info.shortNameHash == Animator.StringToHash(stateName);
    }

    private void MoveTowardPlayer()
    {
        Vector3 dir = FlatDirection(bossPosition, player.position);

        if (dir.sqrMagnitude <= 0.001f)
        {
            PlayIdle();
            return;
        }

        FaceDirection(dir, false);

        Vector3 nextPosition = bossPosition + dir.normalized * groundMoveSpeed * Time.deltaTime;
        nextPosition.y = groundY;

        SetPose(nextPosition);

        anim.SetFloat(HashLocomotion, LocomotionRun, 0.1f, Time.deltaTime);
    }

    private void PlayIdle()
    {
        if (anim != null)
        {
            anim.SetFloat(HashLocomotion, LocomotionIdle, 0.1f, Time.deltaTime);
        }
    }

    private void FaceTarget(Vector3 targetPosition, bool instant = false)
    {
        FaceDirection(FlatDirection(bossPosition, targetPosition), instant);
    }

    private void FaceDirection(Vector3 direction, bool instant)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        bossRotation = instant
            ? targetRotation
            : Quaternion.RotateTowards(bossRotation, targetRotation, turnSpeed * Time.deltaTime);

        ApplyPose();
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        player = playerObject != null ? playerObject.transform : null;
    }

    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    private Vector3 FlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        dir.y = 0f;

        return dir;
    }

    private void SetY(float y)
    {
        bossPosition.y = y;

        ApplyPose();
    }

    private void SetPose(Vector3 position)
    {
        bossPosition = position;

        ApplyPose();
    }

    private void SetPose(Vector3 position, Quaternion rotation)
    {
        bossPosition = position;
        bossRotation = rotation;

        ApplyPose();
    }

    private void ApplyPose()
    {
        if (rigidBody != null)
        {
            rigidBody.position = bossPosition;
            rigidBody.rotation = bossRotation;
        }

        transform.SetPositionAndRotation(bossPosition, bossRotation);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (drawDetectionGizmos)
        {
            DrawDetectionGizmos();
        }
    }

    private void DrawDetectionGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.cyan;
        Vector3 airPosition = transform.position + Vector3.up * airHeight;
        Gizmos.DrawLine(transform.position, airPosition);
        Gizmos.DrawWireSphere(airPosition, 0.5f);
    }

#endif
}
