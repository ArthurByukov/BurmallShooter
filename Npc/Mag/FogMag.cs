using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class FogMag : MonoBehaviour
{
    [Header("Компоненты")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;
    public HealthNPC healthNPC;
    public Transform firePoint;
    public Vector3 spawnPosition;

    [Header("Настройки атаки")]
    public GameObject projectilePrefab;
    public float sightRange = 15f;
    public float attackRange = 15f;
    public float chargeTime = 1.2f;
    public float cooldown = 2f;
    public float projectileSpeed = 15f;
    public float damage = 20f;

    [Header("Режим передвижения")]
    public MovementMode movementMode = MovementMode.Stationary;

    [Header("Настройки дистанции (только для KeepDistance)")]
    public float minDistance = 5f;
    public float maxDistance = 15f;

    public enum MovementMode
    {
        Stationary,
        KeepDistance
    }

    // Состояния
    private bool isAttacking = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private Coroutine attackCoroutine;

    private KnockbackReceiver knockback;

    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        if (healthNPC == null)
            healthNPC = GetComponent<HealthNPC>();
        knockback = GetComponent<KnockbackReceiver>();
    }

    private void OnEnable()
    {
        if (healthNPC != null)
        {
            healthNPC.OnDamageTaken.AddListener(OnDamageTakenHandler);
            healthNPC.OnDeath.AddListener(OnDeathHandler);
        }
    }

    private void OnDisable()
    {
        if (healthNPC != null)
        {
            healthNPC.OnDamageTaken.RemoveListener(OnDamageTakenHandler);
            healthNPC.OnDeath.RemoveListener(OnDeathHandler);
        }
    }

    /// <summary>
    /// Проверка: можно ли использовать NavMeshAgent прямо сейчас.
    /// </summary>
    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }

    private void Update()
    {
        if (healthNPC == null || healthNPC.IsDead) return;

        // Если врага отбросили – не выполняем ИИ, пока летит
        if (knockback != null && knockback.IsKnockback) return;

        // Если враг упал с платформы (не на NavMesh) – не выполняем ИИ
        if (knockback != null && !knockback.IsOnNavMesh) return;

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerInSight = distanceToPlayer <= sightRange;
        bool playerInAttackRange = distanceToPlayer <= attackRange;

        // --- Управление движением ---
        if (movementMode == MovementMode.KeepDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            Vector3 targetPos;

            if (distanceToPlayer < minDistance)
            {
                targetPos = player.position + dirToPlayer * minDistance;
            }
            else if (distanceToPlayer > maxDistance)
            {
                targetPos = player.position + dirToPlayer * maxDistance;
            }
            else
            {
                targetPos = transform.position;
            }

            if (!isAttacking && !isOnCooldown && CanUseAgent())
            {
                agent.SetDestination(targetPos);
            }
            else if (CanUseAgent())
            {
                agent.ResetPath();
            }
        }
        else // Stationary
        {
            if (CanUseAgent())
                agent.ResetPath();
        }

        // --- Поворот к игроку (если видим) ---
        if (playerInSight)
        {
            Vector3 lookDir = (player.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
            }
        }

        // --- Логика атаки ---
        if (playerInAttackRange && !isAttacking && !isOnCooldown)
        {
            attackCoroutine = StartCoroutine(AttackSequence());
        }
        else if (!playerInAttackRange && isAttacking)
        {
            if (attackCoroutine != null)
                StopCoroutine(attackCoroutine);
            isAttacking = false;
            if (animator != null) animator.ResetTrigger("Attack");
            if (CanUseAgent()) agent.isStopped = false;
        }

        // Обновляем кулдаун
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                isOnCooldown = false;
        }
    }

    // ---- Корутина атаки ----
    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        if (CanUseAgent()) agent.isStopped = true;
        // if (animator != null) animator.SetTrigger("Attack");

        float elapsed = 0f;
        while (elapsed < chargeTime)
        {
            elapsed += Time.deltaTime;
            if (player == null ||
                Vector3.Distance(transform.position, player.position) > attackRange)
            {
                isAttacking = false;
                // if (animator != null) animator.ResetTrigger("Attack");
                if (CanUseAgent()) agent.isStopped = false;
                yield break;
            }
            yield return null;
        }

        ShootProjectile();
        // if (animator != null) animator.SetTrigger("Shoot");

        isAttacking = false;
        if (CanUseAgent()) agent.isStopped = false;

        isOnCooldown = true;
        cooldownTimer = cooldown;
    }

    // ---- Создание снаряда ----
    private void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.speed = projectileSpeed;
            projScript.damage = damage;
            projScript.owner = gameObject;
        }
        Vector3 dir = (player.position - firePoint.position).normalized;
        proj.transform.forward = dir;
    }

    // ---- Обработчики событий здоровья ----
    private void OnDamageTakenHandler()
    {
        if (animator != null) animator.SetTrigger("Hit");
        Debug.Log($"Враг получил урон, осталось {healthNPC.CurrentHealth}");
    }

    private void OnDeathHandler()
    {
        healthNPC.DestroyEnemy();
    }
    
    

    // ---- Визуализация в редакторе ----
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        if (movementMode == MovementMode.KeepDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, minDistance);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
    }
}