using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Параметры снаряда")]
    public float speed = 15f;
    public float damage = 20f;
    public GameObject owner;           // кто выпустил снаряд (враг или игрок)

    [Header("Вращение (опционально)")]
    public bool enableRotation = true;
    public Vector3 rotationAxis = Vector3.forward;
    public float rotateSpeed = 720f;

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        if (enableRotation)
        {
            transform.Rotate(rotationAxis, rotateSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Попадание в игрока
        if (other.CompareTag("Player"))
        {
            // Не наносим урон, если владелец – сам игрок (отражённый снаряд)
            if (owner != null && owner == other.gameObject)
                return;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Попадание во врага (через компонент HealthNPC)
        HealthNPC enemyHealth = other.GetComponent<HealthNPC>();
        if (enemyHealth != null)
        {
            // Не наносим урон, если владелец – этот же враг
            if (owner != null && owner == other.gameObject)
                return;

            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Попадание в окружение
        if (other.CompareTag("Environment") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Отражает снаряд в новом направлении с новой скоростью и (опционально) новым владельцем.
    /// </summary>
    public void Reflect(Vector3 newDirection, float newSpeed, GameObject newOwner = null)
    {
        transform.forward = newDirection.normalized;
        speed = newSpeed;
        if (newOwner != null)
            owner = newOwner;
    }
}