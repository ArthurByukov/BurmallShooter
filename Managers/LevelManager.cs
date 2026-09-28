using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelManager;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [Tooltip("Индекс ландшафта из complexHeight в TransformManager")]
        public int structureIndex;

        [Tooltip("Индекс задания/спавна из complexTasks в TransformManager")]
        public int taskIndex;

        [Tooltip("Необязательная задержка перед началом этой волны")]
        public float delayBeforeWave = 0f;
    }

    [Header("Волны")]
    [SerializeField] private Wave[] waves = new Wave[0];

    [Header("Состояние")]
    public int enemyCount;
    public bool CanChangelvl = true;

    [Header("Ссылки")]
    [SerializeField] private TransformManager transformManager;

    [Header("Старт")]
    [SerializeField] private bool startFirstWaveAutomatically = true;

    private int currentWaveIndex = 0;

    private void Awake()
    {
        if (transformManager == null)
            transformManager = GetComponent<TransformManager>();
    }

    private void Start()
    {
        if (startFirstWaveAutomatically)
            CanChangelvl = true;
    }

    private void Update()
    {
        if (!CanChangelvl) return;
        if (enemyCount > 0) return;
        if (waves == null || currentWaveIndex >= waves.Length) return;

     
        StartCoroutine(StartWave(currentWaveIndex));
    }

    private IEnumerator StartWave(int index)
    {

        Wave wave = waves[index];
        currentWaveIndex++;

        if (wave.delayBeforeWave > 0f)
            yield return new WaitForSeconds(wave.delayBeforeWave);

        if (transformManager == null)
        {
            Debug.LogError("LevelManager: TransformManager не назначен!");
            yield break;
        }

        StartCoroutine(
            transformManager.WaitAndChangeStructure(wave.structureIndex, wave.taskIndex)
        );

        // Даём кадр, чтобы заспавненные враги успели увеличить enemyCount
        yield return null;

        
    }
}