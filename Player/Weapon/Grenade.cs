using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private Transform target; // Целевой объект (игрок) – используется для поворота гранаты

    [Header("Параметры взрыва")]
    public float damage = 50f;
    public float delay = 3f;             // Задержка до взрыва
    public float radius = 10f;           // Радиус поражения
    public float force = 700f;           // Физическая сила для обычных объектов (Rigidbody)
    public float knockbackForce = 800f;  // Сила отбрасывания для врагов (через KnockbackReceiver)

    public GameObject explosionEffect;   // Эффект взрыва

    private void Start()
    {
        Invoke(nameof(Explode), delay);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
        else
            Debug.LogWarning("Игрок с тегом 'Player' не найден!");
    }

    void Update()
    {
        if (target != null)
            transform.LookAt(target);
    }

    private void LateUpdate()
    {
        Vector3 euler = transform.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        transform.eulerAngles = euler;
    }

    private void Explode()
    {
        // Эффект взрыва
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position + Vector3.up * 3, Quaternion.identity);

        // Находим все объекты в радиусе
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in colliders)
        {
            // --- 1. Обработка врагов (через HealthNPC и KnockbackReceiver) ---
            HealthNPC enemy = hit.GetComponent<HealthNPC>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);   // наносим урон

                // Отбрасывание врага (если есть компонент KnockbackReceiver)
                KnockbackReceiver knockback = hit.GetComponent<KnockbackReceiver>();
                if (knockback != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    knockback.ApplyKnockback(direction, knockbackForce);
                }
                continue; // пропускаем дальнейшую обработку для этого объекта (чтобы не применять AddExplosionForce)
            }

            // --- 2. Обработка обычных физических объектов (ящики, бочки и т.п.) ---
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Применяем взрывную силу (не для врагов, т.к. они уже обработаны)
                rb.AddExplosionForce(force, transform.position, radius);
            }
        }

        // Уничтожаем гранату
        Destroy(gameObject);
    }

    // Опционально: визуализация радиуса в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}