using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 독립형 드래곤 보스.
/// BaseMonster, SimpleMonster, SensoryMonster를 상속하지 않는다.
/// NavMesh를 사용하지 않고 transform 좌표 기반으로 직접 이동한다.
/// </summary>
public class DragonBoss : LivingEntity
{
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

    private enum BossPhase
    {
        Phase1,
        Phase2,
    }

    private enum DragonAttackType
    {
        Attack1,
        Attack2,
        Special,
    }

    private static readonly int HashLocomotion = Animator.StringToHash("locomotion");

    private static readonly int HashAttack1 = Animator.StringToHash("attack1");
    private static readonly int HashAttack2 = Animator.StringToHash("attack2");
    private static readonly int HashBreatheFire = Animator.StringToHash("breatheFire");

    private static readonly int HashIdleTakeoff = Animator.StringToHash("idleTakeoff");
    private static readonly int HashFlyGlide = Animator.StringToHash("flyGlide");
    private static readonly int HashFlyBreatheFire = Animator.StringToHash("flyBreatheFire");
    private static readonly int HashIdleLand = Animator.StringToHash("idleLand");

    private static readonly int HashGotHit1 = Animator.StringToHash("gotHit1");
    private static readonly int HashDeath = Animator.StringToHash("death");
    private static readonly int HashFlyDeath = Animator.StringToHash("flyDeath");

    private const float LocomotionBackward = 0f;
    private const float LocomotionIdle = 0.5f;
    private const float LocomotionWalk = 0.75f;
    private const float LocomotionRun = 1f;

    [Header("Target")]
    [SerializeField]
    private string playerTag = "Player";

    private Transform player;

    [Header("Components")]
    [SerializeField]
    private Animator anim;

    [Header("External Movement Driver Safety")]
    [SerializeField]
    private bool disableNavMeshAgentOnEnable = true;

    [SerializeField]
    private bool disableRootMotionOnEnable = true;

    [SerializeField]
    private bool forceKinematicRigidbody = true;

    private NavMeshAgent navMeshAgent;
    private Rigidbody rigidBody;

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
    private AnimationCurve landingHeightCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private float groundY;
    private Vector3 airMoveDirection;

    [Header("Phase")]
    [SerializeField]
    private float phase2HealthRatio = 0.5f;

    private BossPhase currentPhase = BossPhase.Phase1;
    private bool hasEnteredPhase2;
    private bool pendingPhase2Transition;

    [Header("Damage")]
    [SerializeField]
    private float attack1Damage = 18f;

    [SerializeField]
    private float attack2Damage = 24f;

    [SerializeField]
    private float breathDamage = 12f;

    [SerializeField]
    private float airBreathDamage = 16f;

    [Header("Burn Status")]
    [SerializeField]
    private float burnDuration = 3f;

    [SerializeField]
    private float burnTickDamage = 2f;

    [Header("Attack Probability")]
    [SerializeField]
    private int forceSpecialAfterMeleeCount = 5;

    [SerializeField]
    private float attack1WeightNear = 0.25f;

    [SerializeField]
    private float attack1WeightFar = 0.75f;

    [SerializeField]
    private float attack2WeightNear = 0.75f;

    [SerializeField]
    private float attack2WeightFar = 0.15f;

    [SerializeField]
    private float baseSpecialWeight = 0.08f;

    [SerializeField]
    private float specialWeightPerMeleeAttack = 0.08f;

    [SerializeField]
    private float maxSpecialWeight = 0.55f;

    private int meleeAttackChainCount;

    [Header("Attack Cooldown")]
    [SerializeField]
    private float attackCooldown = 1.5f;

    private float nextAttackTime;

    [Header("HitBoxes - Existing HitBox.cs")]
    [SerializeField]
    private HitBox headBiteHitBox;

    [SerializeField]
    private HitBox footStompHitBox;

    [SerializeField]
    private HitBox groundBreathHitBox;

    [SerializeField]
    private HitBox airBreathHitBox;

    [Header("HitBox Timing - Normalized Time")]
    [SerializeField]
    private Vector2 attack1HitWindow = new Vector2(0.35f, 0.55f);

    [SerializeField]
    private Vector2 attack2HitWindow = new Vector2(0.45f, 0.65f);

    [SerializeField]
    private Vector2 groundBreathHitWindow = new Vector2(0.25f, 0.85f);

    [SerializeField]
    private Vector2 airBreathHitWindow = new Vector2(0.20f, 0.90f);

    [Header("Animator State Names")]
    [SerializeField]
    private int animatorLayerIndex = 0;

    [SerializeField]
    private string attack1StateName = "Attack01";

    [SerializeField]
    private string attack2StateName = "Attack02";

    [SerializeField]
    private string breatheFireStateName = "BreatheFire";

    [SerializeField]
    private string takeOffStateName = "Idle Takeoff";

    [SerializeField]
    private string flyBreatheFireStateName = "FlyBreatheFire";

    [SerializeField]
    private string landingStateName = "Idle Landing";

    [SerializeField]
    private string gotHitStateName = "Hit01";

    [Header("Hit Reaction")]
    [SerializeField]
    private float hitReactionStepRatio = 0.05f;

    private float nextHitReactionHealth;
    private bool pendingHitReaction;

    [Header("Death")]
    [SerializeField]
    private float destroyDelay = 5f;

    [Header("Debug")]
    [SerializeField]
    private BossState currentState = BossState.Idle;

    private Coroutine currentActionRoutine;

    private bool isActionLocked;
    private bool isAirborne;
    private bool isPhaseTransitioning;

    private bool isGroundAttackTransformLocked;
    private Vector3 lockedGroundAttackPosition;
    private Quaternion lockedGroundAttackRotation;

    private readonly HashSet<LivingEntity> damagedTargetsThisAttack = new HashSet<LivingEntity>();

    protected override void OnEnable()
    {
        base.OnEnable();

        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();

        DisableExternalMovementDrivers();

        FindPlayer();

        groundY = transform.position.y;

        currentState = BossState.Idle;
        currentPhase = BossPhase.Phase1;

        hasEnteredPhase2 = false;
        pendingPhase2Transition = false;
        pendingHitReaction = false;

        isActionLocked = false;
        isAirborne = false;
        isPhaseTransitioning = false;
        isGroundAttackTransformLocked = false;

        meleeAttackChainCount = 0;
        nextAttackTime = 0f;

        nextHitReactionHealth = startingHealth * (1f - hitReactionStepRatio);

        DeactivateAllHitBoxes();

        if (anim != null)
        {
            anim.SetFloat(HashLocomotion, LocomotionIdle);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (IsDead)
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

        if (isActionLocked)
        {
            return;
        }

        switch (currentState)
        {
            case BossState.Idle:
                UpdateIdle();
                break;

            case BossState.Chase:
                UpdateChase();
                break;

            case BossState.GroundAttack:
            case BossState.TakeOff:
            case BossState.AirIdle:
            case BossState.AirBreathFire:
            case BossState.Landing:
            case BossState.HitReaction:
            case BossState.Dead:
                break;
        }
    }

    private void DisableExternalMovementDrivers()
    {
        if (disableRootMotionOnEnable && anim != null)
        {
            anim.applyRootMotion = false;
        }

        if (disableNavMeshAgentOnEnable && navMeshAgent != null)
        {
            if (navMeshAgent.enabled)
            {
                if (navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.ResetPath();
                }

                navMeshAgent.enabled = false;
            }
        }

        if (forceKinematicRigidbody && rigidBody != null)
        {
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }
    }

    private void OnAnimatorMove()
    {
        // Root Motion을 사용하지 않는다.
        // 이 보스의 이동은 MoveTowardPlayer, TakeOff, AirBreathFire, Landing에서 직접 처리한다.
    }

    private void UpdateIdle()
    {
        PlayIdle();

        float dist = GetFlatDistance(transform.position, player.position);

        if (dist <= activationRange)
        {
            currentState = BossState.Chase;
        }
    }

    private void UpdateChase()
    {
        if (!hasEnteredPhase2 && Health <= startingHealth * phase2HealthRatio)
        {
            RequestPhase2Transition();
            return;
        }

        float dist = GetFlatDistance(transform.position, player.position);

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
        DragonAttackType attackType = SelectAttack();

        switch (attackType)
        {
            case DragonAttackType.Attack1:
                meleeAttackChainCount++;

                StartAction(GroundAttackRoutine(
                    HashAttack1,
                    attack1StateName,
                    headBiteHitBox,
                    attack1Damage,
                    false,
                    attack1HitWindow
                ));
                break;

            case DragonAttackType.Attack2:
                meleeAttackChainCount++;

                StartAction(GroundAttackRoutine(
                    HashAttack2,
                    attack2StateName,
                    footStompHitBox,
                    attack2Damage,
                    false,
                    attack2HitWindow
                ));
                break;

            case DragonAttackType.Special:
                meleeAttackChainCount = 0;

                if (currentPhase == BossPhase.Phase1)
                {
                    StartAction(GroundAttackRoutine(
                        HashBreatheFire,
                        breatheFireStateName,
                        groundBreathHitBox,
                        breathDamage,
                        true,
                        groundBreathHitWindow
                    ));
                }
                else
                {
                    StartAction(AirAttackSequenceRoutine(false));
                }
                break;
        }
    }

    private DragonAttackType SelectAttack()
    {
        if (meleeAttackChainCount >= forceSpecialAfterMeleeCount)
        {
            return DragonAttackType.Special;
        }

        float dist = GetFlatDistance(transform.position, player.position);
        float distance01 = Mathf.Clamp01(dist / Mathf.Max(attackRange, 0.01f));

        float attack1Weight = Mathf.Lerp(attack1WeightNear, attack1WeightFar, distance01);
        float attack2Weight = Mathf.Lerp(attack2WeightNear, attack2WeightFar, distance01);

        float specialWeight = Mathf.Clamp(
            baseSpecialWeight + meleeAttackChainCount * specialWeightPerMeleeAttack,
            0f,
            maxSpecialWeight
        );

        float totalWeight = attack1Weight + attack2Weight + specialWeight;
        float random = Random.value * totalWeight;

        if (random < attack1Weight)
        {
            return DragonAttackType.Attack1;
        }

        random -= attack1Weight;

        if (random < attack2Weight)
        {
            return DragonAttackType.Attack2;
        }

        return DragonAttackType.Special;
    }

    private IEnumerator GroundAttackRoutine(
        int triggerHash,
        string stateName,
        HitBox hitBox,
        float damage,
        bool applyBurn,
        Vector2 hitWindow)
    {
        currentState = BossState.GroundAttack;
        isActionLocked = true;

        DeactivateAllHitBoxes();
        damagedTargetsThisAttack.Clear();

        PlayIdle();

        if (player != null)
        {
            FaceTarget(player.position, true);
        }

        lockedGroundAttackPosition = transform.position;
        lockedGroundAttackPosition.y = groundY;
        lockedGroundAttackRotation = transform.rotation;
        isGroundAttackTransformLocked = true;

        anim.SetTrigger(triggerHash);

        yield return WaitUntilCurrentStateStarts(stateName);

        bool hitBoxActive = false;

        while (TryGetCurrentStateNormalizedTime(stateName, out float normalizedTime))
        {
            ApplyGroundAttackTransformLock();

            if (!hitBoxActive && normalizedTime >= hitWindow.x)
            {
                hitBoxActive = true;
                ActivateHitBox(hitBox);
            }

            if (hitBoxActive)
            {
                ApplyDamageFromHitBox(hitBox, damage, applyBurn);
            }

            if (hitBoxActive && normalizedTime > hitWindow.y)
            {
                hitBoxActive = false;
                DeactivateHitBox(hitBox);
            }

            if (normalizedTime >= 1f)
            {
                break;
            }

            yield return null;
        }

        isGroundAttackTransformLocked = false;
        DeactivateHitBox(hitBox);

        FinishActionToChase();
    }

    private void ApplyGroundAttackTransformLock()
    {
        if (!isGroundAttackTransformLocked)
        {
            return;
        }

        transform.position = lockedGroundAttackPosition;
        transform.rotation = lockedGroundAttackRotation;
    }

    private IEnumerator AirAttackSequenceRoutine(bool isPhaseTransition)
    {
        isPhaseTransitioning = isPhaseTransition;

        yield return TakeOffRoutine();
        yield return AirIdleRoutine();
        yield return AirBreathFireRoutine();
        yield return LandingRoutine();

        isPhaseTransitioning = false;

        if (pendingHitReaction)
        {
            pendingHitReaction = false;

            currentActionRoutine = null;
            isActionLocked = false;

            StartAction(HitReactionRoutine());
            yield break;
        }

        FinishActionToChase();
    }

    private IEnumerator TakeOffRoutine()
    {
        currentState = BossState.TakeOff;
        isActionLocked = true;
        isAirborne = true;

        isGroundAttackTransformLocked = false;

        DeactivateAllHitBoxes();
        damagedTargetsThisAttack.Clear();

        PlayIdle();

        if (player != null)
        {
            FaceTarget(player.position, true);
        }

        float startY = transform.position.y;
        float targetY = groundY + airHeight;

        anim.SetTrigger(HashIdleTakeoff);

        yield return WaitUntilCurrentStateStarts(takeOffStateName);

        while (TryGetCurrentStateNormalizedTime(takeOffStateName, out float normalizedTime))
        {
            float t = Mathf.Clamp01(normalizedTime);
            float curveValue = takeOffHeightCurve.Evaluate(t);

            SetY(Mathf.Lerp(startY, targetY, curveValue));

            if (normalizedTime >= 1f)
            {
                break;
            }

            yield return null;
        }

        SetY(targetY);
        anim.SetTrigger(HashFlyGlide);
    }

    private IEnumerator AirIdleRoutine()
    {
        currentState = BossState.AirIdle;

        float timer = 0f;

        while (timer < airIdleDuration)
        {
            if (player != null)
            {
                FaceTarget(player.position);
            }

            SetY(groundY + airHeight);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AirBreathFireRoutine()
    {
        currentState = BossState.AirBreathFire;
        isActionLocked = true;

        DeactivateAllHitBoxes();
        damagedTargetsThisAttack.Clear();

        airMoveDirection = GetFlatDirection(transform.position, player.position);

        if (airMoveDirection.sqrMagnitude <= 0.001f)
        {
            airMoveDirection = transform.forward;
            airMoveDirection.y = 0f;
        }

        airMoveDirection.Normalize();

        FaceDirection(airMoveDirection, true);

        Quaternion lockedRotation = transform.rotation;

        anim.SetTrigger(HashFlyBreatheFire);

        yield return WaitUntilCurrentStateStarts(flyBreatheFireStateName);

        bool hitBoxActive = false;

        while (TryGetCurrentStateNormalizedTime(flyBreatheFireStateName, out float normalizedTime))
        {
            transform.rotation = lockedRotation;

            Vector3 nextPosition = transform.position + airMoveDirection * airMoveSpeed * Time.deltaTime;
            nextPosition.y = groundY + airHeight;
            transform.position = nextPosition;

            if (!hitBoxActive && normalizedTime >= airBreathHitWindow.x)
            {
                hitBoxActive = true;
                ActivateHitBox(airBreathHitBox);
            }

            if (hitBoxActive)
            {
                ApplyDamageFromHitBox(airBreathHitBox, airBreathDamage, true);
            }

            if (hitBoxActive && normalizedTime > airBreathHitWindow.y)
            {
                hitBoxActive = false;
                DeactivateHitBox(airBreathHitBox);
            }

            if (normalizedTime >= 1f)
            {
                break;
            }

            yield return null;
        }

        DeactivateHitBox(airBreathHitBox);
    }

    private IEnumerator LandingRoutine()
    {
        currentState = BossState.Landing;

        float startY = transform.position.y;
        float targetY = groundY;

        if (player != null)
        {
            FaceTarget(player.position, true);
        }

        anim.SetTrigger(HashIdleLand);

        yield return WaitUntilCurrentStateStarts(landingStateName);

        while (TryGetCurrentStateNormalizedTime(landingStateName, out float normalizedTime))
        {
            if (player != null)
            {
                FaceTarget(player.position);
            }

            float t = Mathf.Clamp01(normalizedTime);
            float curveValue = landingHeightCurve.Evaluate(t);

            SetY(Mathf.Lerp(startY, targetY, curveValue));

            if (normalizedTime >= 1f)
            {
                break;
            }

            yield return null;
        }

        SetY(targetY);
        isAirborne = false;
    }

    private void RequestPhase2Transition()
    {
        if (hasEnteredPhase2 || pendingPhase2Transition)
        {
            return;
        }

        pendingPhase2Transition = true;

        if (!isActionLocked)
        {
            StartPhase2TransitionNow();
        }
    }

    private void StartPhase2TransitionNow()
    {
        pendingPhase2Transition = false;

        hasEnteredPhase2 = true;
        currentPhase = BossPhase.Phase2;
        meleeAttackChainCount = 0;

        StopCurrentAction();
        StartAction(AirAttackSequenceRoutine(true));
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (IsDead)
        {
            return;
        }

        float previousHealth = Health;

        base.OnDamage(damage, hitPoint, hitNormal);

        if (IsDead)
        {
            return;
        }

        if (!hasEnteredPhase2 && Health <= startingHealth * phase2HealthRatio)
        {
            RequestPhase2Transition();
            return;
        }

        if (ShouldTriggerHitReaction(previousHealth, Health))
        {
            if (ShouldDelayHitReaction())
            {
                pendingHitReaction = true;
            }
            else
            {
                StopCurrentAction();
                StartAction(HitReactionRoutine());
            }
        }
    }

    private bool ShouldTriggerHitReaction(float previousHealth, float currentHealth)
    {
        bool shouldReact = false;
        float stepAmount = startingHealth * hitReactionStepRatio;

        while (currentHealth <= nextHitReactionHealth && nextHitReactionHealth > 0f)
        {
            shouldReact = true;
            nextHitReactionHealth -= stepAmount;
        }

        return shouldReact;
    }

    private bool ShouldDelayHitReaction()
    {
        return isAirborne ||
               isPhaseTransitioning ||
               currentState == BossState.TakeOff ||
               currentState == BossState.AirIdle ||
               currentState == BossState.AirBreathFire ||
               currentState == BossState.Landing;
    }

    private IEnumerator HitReactionRoutine()
    {
        currentState = BossState.HitReaction;
        isActionLocked = true;
        isGroundAttackTransformLocked = false;

        DeactivateAllHitBoxes();
        damagedTargetsThisAttack.Clear();

        PlayIdle();

        ResetCombatTriggers();

        anim.SetTrigger(HashGotHit1);

        yield return WaitUntilCurrentStateStarts(gotHitStateName);

        while (TryGetCurrentStateNormalizedTime(gotHitStateName, out float normalizedTime))
        {
            PlayIdle();

            if (normalizedTime >= 1f)
            {
                break;
            }

            yield return null;
        }

        FinishActionToChase();
    }

    private void ResetCombatTriggers()
    {
        if (anim == null)
        {
            return;
        }

        anim.ResetTrigger(HashAttack1);
        anim.ResetTrigger(HashAttack2);
        anim.ResetTrigger(HashBreatheFire);
        anim.ResetTrigger(HashIdleTakeoff);
        anim.ResetTrigger(HashFlyBreatheFire);
        anim.ResetTrigger(HashIdleLand);
    }

    public override void Die()
    {
        if (IsDead)
        {
            return;
        }

        base.Die();

        StopCurrentAction();
        DeactivateAllHitBoxes();

        currentState = BossState.Dead;
        isActionLocked = true;
        isGroundAttackTransformLocked = false;

        if (anim != null)
        {
            if (isAirborne)
            {
                anim.SetTrigger(HashFlyDeath);
            }
            else
            {
                anim.SetTrigger(HashDeath);
            }
        }

        Destroy(gameObject, destroyDelay);
    }

    private void MoveTowardPlayer()
    {
        isGroundAttackTransformLocked = false;

        Vector3 dir = GetFlatDirection(transform.position, player.position);

        if (dir.sqrMagnitude <= 0.001f)
        {
            PlayIdle();
            return;
        }

        FaceDirection(dir, false);

        Vector3 nextPosition = transform.position + dir.normalized * groundMoveSpeed * Time.deltaTime;
        nextPosition.y = groundY;

        transform.position = nextPosition;

        anim.SetFloat(HashLocomotion, LocomotionRun, 0.1f, Time.deltaTime);
    }

    private void PlayIdle()
    {
        if (anim == null)
        {
            return;
        }

        anim.SetFloat(HashLocomotion, LocomotionIdle, 0.1f, Time.deltaTime);
    }

    private void FaceTarget(Vector3 targetPosition, bool instant = false)
    {
        Vector3 dir = GetFlatDirection(transform.position, targetPosition);

        if (dir.sqrMagnitude <= 0.001f)
        {
            return;
        }

        FaceDirection(dir, instant);
    }

    private void FaceDirection(Vector3 direction, bool instant)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        if (instant)
        {
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }

    private void ActivateHitBox(HitBox hitBox)
    {
        if (hitBox == null)
        {
            return;
        }

        hitBox.gameObject.SetActive(true);
        hitBox.Colliders.Clear();

        Collider[] colliders = hitBox.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }

    private void DeactivateHitBox(HitBox hitBox)
    {
        if (hitBox == null)
        {
            return;
        }

        Collider[] colliders = hitBox.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        hitBox.Colliders.Clear();
    }

    private void DeactivateAllHitBoxes()
    {
        DeactivateHitBox(headBiteHitBox);
        DeactivateHitBox(footStompHitBox);
        DeactivateHitBox(groundBreathHitBox);
        DeactivateHitBox(airBreathHitBox);
    }

    private void ApplyDamageFromHitBox(HitBox hitBox, float damage, bool applyBurn)
    {
        if (hitBox == null)
        {
            return;
        }

        Collider[] targets = hitBox.Colliders.ToArray();

        foreach (Collider targetCollider in targets)
        {
            if (targetCollider == null)
            {
                continue;
            }

            LivingEntity target = targetCollider.GetComponentInParent<LivingEntity>();

            if (target == null)
            {
                continue;
            }

            if (target == this)
            {
                continue;
            }

            bool isTargetPlayer =
                target.CompareTag(playerTag) ||
                targetCollider.CompareTag(playerTag) ||
                target.transform.root.CompareTag(playerTag);

            if (!isTargetPlayer)
            {
                continue;
            }

            if (target.IsDead)
            {
                continue;
            }

            if (damagedTargetsThisAttack.Contains(target))
            {
                continue;
            }

            damagedTargetsThisAttack.Add(target);

            Vector3 hitPoint = targetCollider.ClosestPoint(transform.position);
            Vector3 hitNormal = GetFlatDirection(transform.position, target.transform.position);

            if (hitNormal.sqrMagnitude <= 0.001f)
            {
                hitNormal = transform.forward;
            }

            target.OnDamage(damage, hitPoint, hitNormal.normalized);

            if (applyBurn)
            {
                target.ApplyStatusEffect(StatusFlags.Burn, burnDuration, burnTickDamage);
            }
        }
    }

    private void StartAction(IEnumerator routine)
    {
        StopCurrentAction();
        currentActionRoutine = StartCoroutine(routine);
    }

    private void StopCurrentAction()
    {
        if (currentActionRoutine != null)
        {
            StopCoroutine(currentActionRoutine);
            currentActionRoutine = null;
        }

        isGroundAttackTransformLocked = false;

        DeactivateAllHitBoxes();
        damagedTargetsThisAttack.Clear();

        isActionLocked = false;
    }

    private void FinishActionToChase()
    {
        isGroundAttackTransformLocked = false;

        DeactivateAllHitBoxes();
        damagedTargetsThisAttack.Clear();

        currentActionRoutine = null;
        isActionLocked = false;
        nextAttackTime = Time.time + attackCooldown;

        if (pendingPhase2Transition)
        {
            StartPhase2TransitionNow();
            return;
        }

        if (pendingHitReaction && !ShouldDelayHitReaction())
        {
            pendingHitReaction = false;
            StartAction(HitReactionRoutine());
            return;
        }

        currentState = BossState.Chase;
    }

    private IEnumerator WaitUntilCurrentStateStarts(string stateName)
    {
        float timeout = 2f;

        while (timeout > 0f)
        {
            if (!anim.IsInTransition(animatorLayerIndex))
            {
                AnimatorStateInfo currentStateInfo = anim.GetCurrentAnimatorStateInfo(animatorLayerIndex);

                if (IsAnimatorStateName(currentStateInfo, stateName))
                {
                    yield break;
                }
            }
            else
            {
                AnimatorStateInfo nextStateInfo = anim.GetNextAnimatorStateInfo(animatorLayerIndex);

                if (IsAnimatorStateName(nextStateInfo, stateName))
                {
                    // 다음 상태가 목표 상태면 전환이 끝날 때까지 기다린다.
                }
            }

            timeout -= Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning($"[{gameObject.name}] Animator state '{stateName}'에 진입하지 못했습니다.");
    }

    private bool TryGetCurrentStateNormalizedTime(string stateName, out float normalizedTime)
    {
        normalizedTime = 0f;

        if (anim == null)
        {
            return false;
        }

        if (anim.IsInTransition(animatorLayerIndex))
        {
            return false;
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(animatorLayerIndex);

        if (!IsAnimatorStateName(stateInfo, stateName))
        {
            return false;
        }

        normalizedTime = stateInfo.normalizedTime;
        return true;
    }

    private bool IsAnimatorStateName(AnimatorStateInfo stateInfo, string stateName)
    {
        return stateInfo.IsName(stateName) ||
               stateInfo.shortNameHash == Animator.StringToHash(stateName);
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    private Vector3 GetFlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        dir.y = 0f;

        return dir;
    }

    private void SetY(float y)
    {
        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.cyan;
        Vector3 airPos = transform.position;
        airPos.y += airHeight;
        Gizmos.DrawLine(transform.position, airPos);
        Gizmos.DrawWireSphere(airPos, 0.5f);

        if (Application.isPlaying && player != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
#endif
}