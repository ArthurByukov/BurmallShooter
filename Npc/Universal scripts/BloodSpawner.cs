//using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class BloodSpawner : MonoBehaviour
{
    [Header("Префабы")]
    [SerializeField]  private GameObject bloodSplatterPrefab;   // Particle System (брызги)
    [SerializeField] private GameObject[] bloodDecalPrefabs;   // Массив декалей (пятна на полу)

    [Header("Настройки брызг")]
    [SerializeField] private float splatterLifetime = 2f;      // Через сколько удалить объект частиц
    [SerializeField] private Vector3 splatterSpawnPosition;
    [Header("Настройки декали")]
    [SerializeField] private float decalLifetime = 15f;        // Время жизни пятна на полу
    [SerializeField] private float groundRayDistance = 10f;    // Как далеко вниз искать поверхность
    [SerializeField] private LayerMask groundLayers;           // Слои, которые считаются "полом"

    private Quaternion rot;
    /// <summary>
    /// Вызывай этот метод, когда враг получил урон.
    /// hitPoint — точка попадания (откуда летят брызги).
    /// hitNormal — нормаль поверхности в точке попадания (для ориентации брызг, если нужно).
    /// </summary>


    private void Start()
    {
        rot = Quaternion.LookRotation(splatterSpawnPosition);
    }
    public void SpawnSplatter()
    {

        if (bloodSplatterPrefab == null) return;

        // Ориентируем партикл-систему так, чтобы её ось Z смотрела от поверхности
        // (важно, если в Shape используется Cone — он будет "бить" в нужную сторону)
        Quaternion rot = Quaternion.LookRotation(splatterSpawnPosition);
        

        GameObject splatter = Instantiate(bloodSplatterPrefab, splatterSpawnPosition, rot);
        Destroy(splatter, splatterLifetime);
        Debug.Log("krovy");

    }

    

    public void SpawnDecalUnderEnemy()
    {
        if (bloodDecalPrefabs == null || bloodDecalPrefabs.Length == 0) return;

        // Пускаем луч вниз от позиции врага
        Ray ray = new Ray(transform.position, Vector3.down);

        if (!Physics.Raycast(ray, out RaycastHit hit, groundRayDistance, groundLayers))
            return; // под врагом нет поверхности — пятно не создаём

        // Выбираем случайную декаль
        GameObject decalPrefab = bloodDecalPrefabs[Random.Range(0, bloodDecalPrefabs.Length)];

        // Quad "смотрит" своей лицевой стороной вдоль -Z, поэтому разворачиваем
        // через -hit.normal, чтобы лицевая сторона была направлена ОТ пола вверх
        Quaternion rot = Quaternion.LookRotation(-hit.normal, Vector3.up);

        // Небольшой сдвиг по нормали, чтобы избежать Z-fighting с полом
        Vector3 spawnPos = hit.point + hit.normal * 0.01f;

        GameObject decal = Instantiate(decalPrefab, spawnPos, rot);

        // Случайный поворот вокруг нормали — чтобы пятна не выглядели одинаково
        decal.transform.Rotate(hit.normal, Random.Range(0f, 360f), Space.World);
        Debug.Log("decal");
        Destroy(decal, decalLifetime);
    }
}