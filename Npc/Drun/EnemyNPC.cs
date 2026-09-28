using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyNPC : MonoBehaviour
{
    [Header("Настройки компонентов")]
    public NavMeshAgent agent;
    private Transform player;
    public Animator animator;
    public LayerMask whatIsGround, whatIsPlayer;

    // Ссылка на компонент здоровья
    public HealthNPC healthNPC;

    public GameObject DeathBody;

    [Header("Настройки патрулирования")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Random;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] coverPoints;
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float fearRunSpeed, sightRange, attackRange, AllertTime, attackCooldown;
    public bool wantAttack, alerted, Attacking, playerInAttackRange, isInFear;
    private bool isAlerting = false;
    private float attackTimer = 0f;

    private enum PatrolMode
    {
        Random,
        Waypoints
    }

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (healthNPC == null)
            healthNPC = GetComponent<HealthNPC>();
    }

    private void OnEnable()
    {
        if (healthNPC != null)
        {
            healthNPC.OnDamageTaken.AddListener(OnDamageTakenHandler);
            healthNPC.OnCriticalHealth.AddListener(OnCriticalHealthHandler);
            healthNPC.OnDeath.AddListener(OnDeathHandler);
        }
    }

    private void OnDisable()
    {
        if (healthNPC != null)
        {
            healthNPC.OnDamageTaken.RemoveListener(OnDamageTakenHandler);
            healthNPC.OnCriticalHealth.RemoveListener(OnCriticalHealthHandler);
            healthNPC.OnDeath.RemoveListener(OnDeathHandler);
        }
    }

    private void Update()
    {
        // Проверка видимости
        Attacking = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!Attacking) wantAttack = false;

        // Режим страха
        if (isInFear)
        {
            animator.SetBool("InFear", true);
            // Движение к укрытию уже запущено в корутине
        }
        else
        {
            animator.SetBool("InFear", false);

            if (Attacking && !wantAttack && !isAlerting)
            {
                StartCoroutine(Alert());
            }
            else
            {
                if (!Attacking && !playerInAttackRange) Patroling();
                if (Attacking && !playerInAttackRange && wantAttack) ChasePlayer();
                if (playerInAttackRange && Attacking && wantAttack) AttackPlayer();
            }
        }
    }

    // ----- Обработчики событий здоровья -----
    private void OnDamageTakenHandler()
    {
        animator.SetTrigger("Hit");
        Debug.Log($"NPC получил урон, остаток здоровья: {healthNPC.CurrentHealth}");
    }

    private void OnCriticalHealthHandler()
    {
        if (!isInFear)
        {
            isInFear = true;
            StartCoroutine(FleeToCover());
        }
    }

    private void OnDeathHandler()
    {
        StartCoroutine(DestroyEnemy());
    }

    
    // ----- Остальные методы (без изменений) -----
    private IEnumerator Alert()
    {
        isAlerting = true;
        agent.isStopped = true;
        animator.SetTrigger("Alert");
        yield return new WaitForSeconds(AllertTime);
        wantAttack = true;
        agent.isStopped = false;
        isAlerting = false;
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet) agent.SetDestination(walkPoint);
        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        if (!alreadyAttacked)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(20f);
                animator.SetTrigger("Attack");
                alreadyAttacked = true;
                attackTimer = attackCooldown;
            }
        }
        else
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f) alreadyAttacked = false;
        }
    }

    private IEnumerator FleeToCover()
    {
        Transform bestCover = GetNearestCover();
        if (bestCover != null)
        {
            agent.speed = fearRunSpeed;
            agent.SetDestination(bestCover.position);
            animator.SetTrigger("Fear");

            while (Vector3.Distance(transform.position, bestCover.position) > 1f)
                yield return null;

            agent.isStopped = true;
            animator.SetBool("IsWalking", false);
            yield return new WaitForSeconds(5f);

            agent.speed = 3.5f;
            agent.isStopped = false;
            isInFear = false;
        }
        else
        {
            isInFear = false;
        }
    }

    private Transform GetNearestCover()
    {
        Transform nearest = null;
        float minDistance = 50f;
        foreach (Transform cover in coverPoints)
        {
            float dist = Vector3.Distance(transform.position, cover.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = cover;
            }
        }
        return nearest;
    }

    private IEnumerator DestroyEnemy()
    {
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(0.3f);

        healthNPC.DestroyEnemy();
       // Vector3 spawn = transform.position;
       // spawn.y += 2f;
       // Instantiate(DeathBody, spawn, transform.rotation);
       // Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}