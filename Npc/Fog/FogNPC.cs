using UnityEngine;
using UnityEngine.AI;

public class FogNPC : MonoBehaviour
{
    [Header("Настройки компонентов")]
    public NavMeshAgent agent;
    private Transform player;
    public Animator animator;
    public LayerMask whatIsGround, whatIsPlayer;
    public GameObject explosiveObject;
    // Ссылка на компонент здоровья
    public HealthNPC healthNPC;
    public Vector3 spawnPosition;

    [Header("Настройки патрулирования")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Random;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] coverPoints;
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;


    //States
    public float sightRange, attackRange;
    public bool  Attacking, playerInAttackRange;

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


    private void Update()
    {
        // Проверка видимости
        Attacking = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
                
        if (!Attacking && !playerInAttackRange) Patroling();
        if (Attacking && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && Attacking) AttackPlayer();
        
        
    }

    // ----- Обработчики событий здоровья -----
    private void OnDamageTakenHandler()
    {
        animator.SetTrigger("Hit");
        Debug.Log($"NPC получил урон, остаток здоровья: {healthNPC.CurrentHealth}");
    }


    private void OnDeathHandler()
    {
        healthNPC.DestroyEnemy();
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
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
            Instantiate(explosiveObject, transform.position + spawnPosition, transform.rotation);
                playerHealth.TakeDamage(50f);
                healthNPC.DestroyEnemy();
            }
      
    }
    

    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
