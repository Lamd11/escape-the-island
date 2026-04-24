using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
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
            }
            else
            {
                TryAttack();
            }
        }
    }

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
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
        GameManager.instance.TakeDamage(damage);

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