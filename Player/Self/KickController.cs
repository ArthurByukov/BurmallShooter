using UnityEngine;

public class KickController : MonoBehaviour
{
    [Header("Параметры пинка")]
    [SerializeField]
    private float kickRange = 3f;               // дальность действия
    [SerializeField]
    private float kickRadius = 1.5f;            // радиус сферы проверки
    [SerializeField]
    private float kickForce = 20f;              // сила отбрасывания врагов
    [SerializeField] 
    private float reflectSpeedMultiplier = 1.5f;// множитель скорости отражённого снаряда
    [SerializeField] 
    private float maxAngle = 45f;               // максимальный угол между взглядом и целью (в градусах)
    [SerializeField]
    private LayerMask kickableLayers;           // маска слоёв, которые можно пнуть

    private Transform playerCam;
    [SerializeField]private Animator animator;
    private void Start()
    {
     //   animator = GetComponent<Animator>();
        playerCam = Camera.main?.transform;
        if (playerCam == null)
            Debug.LogError("KickController: не найдена основная камера (Camera.main)!");
    }

    private void Update()
    {
        if (MenuManager.GameplayBlocked) return;
        // Нажмите F для удара ногой
        if (Input.GetKeyDown(KeyCode.F))
        {
            PerformKick();
        }
    }

    private void PerformKick()
    {
        if (playerCam == null) return;
        animator.SetTrigger("Hit");
        Vector3 origin = transform.position;
        Vector3 direction = playerCam.forward;

        // Находим все коллайдеры в сфере перед игроком
        Vector3 sphereCenter = origin + direction * (kickRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(sphereCenter, kickRadius, kickableLayers);

        foreach (Collider hit in hits)
        {
            // Проверяем, смотрит ли игрок на объект
            Vector3 toObject = (hit.transform.position - origin).normalized;
            float angle = Vector3.Angle(direction, toObject);
            if (angle > maxAngle) continue;

            // --- Обработка снаряда ---
            Projectile proj = hit.GetComponent<Projectile>();
            if (proj != null)
            {
                // Отражаем в направлении взгляда игрока, увеличиваем скорость
                proj.Reflect(direction, proj.speed * reflectSpeedMultiplier, gameObject);
                continue;
            }

            // --- Обработка врага (через компонент KnockbackReceiver) ---
            KnockbackReceiver knockback = hit.GetComponent<KnockbackReceiver>();
            if (knockback != null)
            {
                knockback.ApplyKnockback(direction, kickForce);
                continue;
            }

            // При желании можно добавить обработку других объектов (ящики, физические тела)
        }
    }

    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        if (playerCam == null) return;
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        Vector3 direction = playerCam.forward;
        Vector3 sphereCenter = origin + direction * (kickRange * 0.5f);
        Gizmos.DrawWireSphere(sphereCenter, kickRadius);
        Gizmos.DrawLine(origin, origin + direction * kickRange);
    }
}