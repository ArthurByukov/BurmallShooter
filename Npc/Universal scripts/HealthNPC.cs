using UnityEngine;
using UnityEngine.Events;

public class HealthNPC : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float criticalHealthThreshold = 30f;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;   // текущее здоровье
    public UnityEvent OnDamageTaken;
    public UnityEvent OnCriticalHealth;
    public UnityEvent OnDeath;
    public GameObject DeathBody;
    public Vector3 spawnPosition;
    [SerializeField] private float currentHealth;
    public int BloodSplatterChance;
    public int BloodDecalChance;
    private bool isDead;
    LevelManager manager;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private BloodSpawner Bloodspawner;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }
    private void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("Manager");
        manager = gameManager.GetComponent<LevelManager>();
        Bloodspawner = gameObject.GetComponent<BloodSpawner>();
        manager.enemyCount++;
    }

    
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke();

        ///<summary>
        ///
        ///
        ///if(Random.value * 100 > BloodSplatterChance)
        ///{
        ///    Bloodspawner.SpawnSplatter();
        ///}
        ///
        ///if (Random.value * 100 > BloodDecalChance)
        ///{
        ///    Bloodspawner.SpawnDecalUnderEnemy();
        ///}
        ///</summary>
        Bloodspawner.SpawnSplatter();
        Bloodspawner.SpawnDecalUnderEnemy();
        if (currentHealth <= criticalHealthThreshold)
        {
            OnCriticalHealth?.Invoke();
        }

        if (currentHealth <= 0f && !isDead)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        OnHealthChanged?.Invoke(currentHealth);
    }
    public void DestroyEnemy()
    {
        Vector3 spawn = transform.position;
        spawn.y += 2f;
        manager.enemyCount--;
        Instantiate(DeathBody, spawn + spawnPosition, transform.rotation);
        Destroy(gameObject);
    }
}