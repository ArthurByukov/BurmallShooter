using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Точки спавна (дочерние объекты)")]
    [SerializeField] private Transform[] spawnPoints = new Transform[9];

    [Header("Доступные префабы")]
    public GameObject[] enemyPrefabs;
    public GameObject healthPrefab;
    public GameObject ammoPrefab;

    [System.Serializable]
    public class SpawnTask
    {
        public GameObject prefab;
        public int count;
    }

    //  Новое поле: список предопределённых задач (можно заполнить в инспекторе)
    [Header("Предопределённые задачи спавна (по индексу)")]
    [SerializeField] private SpawnTask[] spawnTaskList;

    //  Новый метод – принимает индекс и спавнит соответствующую задачу
    public void SpawnMultiple(int taskIndex)
    {
        if (spawnTaskList == null || taskIndex < 0 || taskIndex >= spawnTaskList.Length)
        {
            Debug.LogError($"Некорректный индекс задачи: {taskIndex}");
            return;
        }

        // Создаём массив из одной задачи и передаём в основной метод
        SpawnMultiple(new SpawnTask[] { spawnTaskList[taskIndex] });
    }

    // Оригинальный метод (можно использовать для передачи произвольного набора задач)
    public void SpawnMultiple(SpawnTask[] tasks)
    {
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        int pointIndex = 0;
        foreach (var task in tasks)
        {
            for (int i = 0; i < task.count; i++)
            {
                if (pointIndex >= availablePoints.Count)
                {
                    Debug.LogWarning("Недостаточно точек спавна на блоке!");
                    return;
                }
                Vector3 spawnPos = availablePoints[pointIndex].position;
                Instantiate(task.prefab, spawnPos, Quaternion.identity);
                pointIndex++;
            }
        }
    }
}