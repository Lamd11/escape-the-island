using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    [Tooltip("Slightly faster than PlayerMovement.speed (default 5) so night chases are threatening.")]
    public float speed = 7.5f;
    [Header("Attack")]
    public float attackRange = 1.6f;
    [Tooltip("Damage per second target; actual damage is applied per swing.")]
    public float damagePerSecond = 10f;
    public float attackCooldown = 1.5f;
    public float knockbackForce = 6f;
    public string playerTag = "Player";
    public string attackAnimatorTrigger = "Attack";
    Animator animator;
    float cooldownRemaining = 0f;
    Rigidbody playerRb;

    [Header("Audio")]
    [Tooltip("Footstep cadence while chasing the player at night.")]
    public float enemyFootstepInterval = 0.55f;
    float enemyFootstepTimer;
    [Tooltip("3D footstep base volume (distance falloff is handled by min/max distance).")]
    [Range(0f, 1f)]
    public float enemyFootstepVolume = 0.58f;
    [Tooltip("Full volume within this radius (meters).")]
    public float enemyFootstepMinDistance = 1.5f;
    [Tooltip("Footsteps silent beyond this radius (meters).")]
    public float enemyFootstepMaxDistance = 36f;
    [Tooltip("Emit slightly toward the player so 3D panning reads as 'from that direction'.")]
    public bool offsetEmitterTowardPlayer = true;
    [Min(0f)]
    public float emitterOffsetTowardPlayerMeters = 0.35f;
    [Tooltip("Boost or reduce volume when the enemy is in front vs behind the listener (camera). Stereo pan still comes from 3D audio.")]
    public bool applyListenerDirectionToFootstepVolume = true;
    [Min(0f)]
    public float volumeMulInFrontOfListener = 1.2f;
    [Min(0f)]
    public float volumeMulBehindListener = 0.78f;
    const string FootstepChildName = "EnemyFootstep3D";
    AudioSource enemyFootstepSource;
    static Transform s_cachedListenerTransform;

    void Update()
    {
        if (GameManager.instance != null && (GameManager.instance.hasWon || GameManager.instance.hasLost))
        {
            return;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
        if (player == null) return;
        if (DayNightManager.instance == null) return;

        cooldownRemaining -= Time.deltaTime;

        if (DayNightManager.instance.isNight)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            // Chase until in attack range
            if (distance > attackRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.deltaTime
                );

                if (enemyFootstepInterval > 0f)
                {
                    enemyFootstepTimer -= Time.deltaTime;
                    if (enemyFootstepTimer <= 0f)
                    {
                        PlayEnemyFootstep3D();
                        enemyFootstepTimer = enemyFootstepInterval;
                    }
                }
            }
            else
            {
                enemyFootstepTimer = 0f;
                TryAttack();
            }
        }
    }

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        EnsureEnemyFootstepSource();
    }

    void EnsureEnemyFootstepSource()
    {
        if (enemyFootstepSource != null) return;

        Transform existing = transform.Find(FootstepChildName);
        GameObject holder = existing != null ? existing.gameObject : new GameObject(FootstepChildName);
        holder.transform.SetParent(transform);
        holder.transform.localPosition = Vector3.zero;

        enemyFootstepSource = holder.GetComponent<AudioSource>();
        if (enemyFootstepSource == null)
            enemyFootstepSource = holder.AddComponent<AudioSource>();

        enemyFootstepSource.playOnAwake = false;
        enemyFootstepSource.spatialBlend = 1f;
        enemyFootstepSource.dopplerLevel = 0f;
        ApplyFootstepRolloff();
    }

    void ApplyFootstepRolloff()
    {
        if (enemyFootstepSource == null) return;
        enemyFootstepSource.minDistance = Mathf.Max(0.01f, enemyFootstepMinDistance);
        enemyFootstepSource.maxDistance = Mathf.Max(enemyFootstepSource.minDistance + 0.01f, enemyFootstepMaxDistance);
        enemyFootstepSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    void PlayEnemyFootstep3D()
    {
        if (GameAudio.Instance == null || GameAudio.Instance.enemyFootstep == null) return;

        EnsureEnemyFootstepSource();
        ApplyFootstepRolloff();

        if (offsetEmitterTowardPlayer && player != null && emitterOffsetTowardPlayerMeters > 0f)
        {
            Vector3 flat = player.position - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.0001f)
                enemyFootstepSource.transform.position = transform.position + flat.normalized * emitterOffsetTowardPlayerMeters;
            else
                enemyFootstepSource.transform.localPosition = Vector3.zero;
        }
        else
        {
            enemyFootstepSource.transform.localPosition = Vector3.zero;
        }

        float vol = enemyFootstepVolume;
        if (GameAudio.Instance != null)
            vol *= Mathf.Clamp01(GameAudio.Instance.sfxVolume);

        if (applyListenerDirectionToFootstepVolume)
            vol *= ListenerDirectionVolumeMultiplier(enemyFootstepSource.transform.position);

        enemyFootstepSource.PlayOneShot(GameAudio.Instance.enemyFootstep, Mathf.Clamp01(vol));
    }

    static Transform GetListenerTransform()
    {
        if (s_cachedListenerTransform != null) return s_cachedListenerTransform;
        AudioListener listener = Object.FindFirstObjectByType<AudioListener>(FindObjectsInactive.Exclude);
        if (listener != null)
            s_cachedListenerTransform = listener.transform;
        return s_cachedListenerTransform;
    }

    float ListenerDirectionVolumeMultiplier(Vector3 soundWorldPosition)
    {
        Transform lt = GetListenerTransform();
        if (lt == null) return 1f;

        Vector3 toSound = soundWorldPosition - lt.position;
        toSound.y = 0f;
        if (toSound.sqrMagnitude < 0.0001f) return 1f;
        toSound.Normalize();

        Vector3 lf = lt.forward;
        lf.y = 0f;
        if (lf.sqrMagnitude < 0.0001f) return 1f;
        lf.Normalize();

        float frontness = Vector3.Dot(lf, toSound);
        return Mathf.Lerp(volumeMulBehindListener, volumeMulInFrontOfListener, (frontness + 1f) * 0.5f);
    }

    void TryAttack()
    {
        if (cooldownRemaining > 0f) return;
        if (GameManager.instance == null) return;

        cooldownRemaining = attackCooldown;

        if (animator != null && !string.IsNullOrWhiteSpace(attackAnimatorTrigger))
        {
            animator.SetTrigger(attackAnimatorTrigger);
        }

        float damage = damagePerSecond * attackCooldown;
        GameManager.instance.TakeDamage(damage, playHitSfx: true);

        if (playerRb == null)
        {
            playerRb = player.GetComponent<Rigidbody>();
        }
        if (playerRb != null && knockbackForce > 0f)
        {
            Vector3 dir = (player.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                dir.Normalize();
                playerRb.AddForce(dir * knockbackForce, ForceMode.VelocityChange);
            }
        }

        // Stop enemy when player dies
        if (GameManager.instance.hasLost)
        {
            enabled = false;
        }
    }
}