using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class KnockbackReceiver : MonoBehaviour
{
    [Header("Настройки отбрасывания")]
    public float knockbackDuration = 0.5f;   // длительность воздействия
    public float destroyBelowY = -20f;       // высота, ниже которой враг уничтожается

    private NavMeshAgent agent;
    private Rigidbody rb;
    private bool isKnockback = false;
    private HealthNPC health;

    // Публичные свойства для внешних скриптов (например, FogMag)
    public bool IsKnockback => isKnockback;
    public bool IsOnNavMesh { get; private set; } = true;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        health = GetComponent<HealthNPC>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        // Если враг мёртв – не обрабатываем падение
        if (health != null && health.IsDead) return;

        // Если враг упал ниже порога – уничтожаем
        if (transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Применяет импульс силы для отбрасывания.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (isKnockback || rb == null) return;

        // Отключаем NavMeshAgent на время отбрасывания
        if (agent != null && agent.enabled)
            agent.enabled = false;

        isKnockback = true;
        rb.isKinematic = false;
        rb.AddForce(direction * force, ForceMode.Impulse);

        StartCoroutine(RecoverFromKnockback());
    }

    private IEnumerator RecoverFromKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);

        // Гасим скорость, чтобы враг не улетал бесконечно
        if (rb != null)
            rb.velocity = Vector3.zero;

        if (agent != null)
        {
            // Проверяем, стоит ли враг на NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas))
            {
                // Враг на NavMesh – возвращаем управление агенту
                rb.isKinematic = true;
                agent.enabled = true;
                // Принудительно ставим агента точно на NavMesh, чтобы SetDestination не ругался
                agent.Warp(hit.position);
                IsOnNavMesh = true;
            }
            else
            {
                // Враг упал с платформы – оставляем физику, агента НЕ включаем
                rb.isKinematic = false;
                agent.enabled = false;
                IsOnNavMesh = false;
            }
        }

        isKnockback = false;
    }
}