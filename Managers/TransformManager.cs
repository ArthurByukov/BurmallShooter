using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static BlockSpawner;

public class TransformManager : MonoBehaviour
{
    [SerializeField] float time = 2f;
    [SerializeField] private GameObject[] blocks = new GameObject [36];

    LevelManager manager;

    private int[][,] complexHeight = new int[4][,];
    private int[][,] complexTasks = new int[4][,];

    public NavMeshSurface surfaceToUpdate; // Назначьте Surface в Inspector

    // Вызовите этот метод для обновления NavMesh
    public void UpdateNavMesh()
    {
        if (surfaceToUpdate == null)
        {
            Debug.LogError("NavMeshSurface не назначен!");
            return;
        }
       
        surfaceToUpdate.RemoveData();
        surfaceToUpdate.BuildNavMesh();
      
    }

   

    private void Start()
    {
        manager = gameObject.GetComponent<LevelManager>();


        complexHeight[0] = new int[,]{
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0}
        };
        complexHeight[1] = new int [,]{
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 1, 1, 0, 0},
        { 0, 0, 1, 1, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0}
        };
        complexHeight[2] = new int[,]{
        { 3, 3, 3, 3, 3, 3},
        { 3, 0, 0, 0, 0, 3},
        { 3, 0, 1, 1, 0, 3},
        { 3, 0, 1, 1, 0, 3},
        { 3, 0, 0, 0, 0, 3},
        { 3, 3, 3, 3, 3, 3}
        };
        complexHeight[3] = new int[,]{
        { 0, 0, 0, 0, 0, 0},
        { 0, 3, 1, 1, 2, 0},
        { 0, 1, 1, 1, 1, 0},
        { 0, 1, 1, 1, 1, 0},
        { 0, 2, 1, 1, 3, 0},
        { 0, 0, 0, 0, 0, 0}
        };






        complexTasks[1] = new int[,] {
        { 1, 1, 1, 1, 1, 1},
        { 1, 0, 0, 0, 0, 1},
        { 1, 0, 2, 2, 0, 1},
        { 1, 0, 2, 2, 0, 1},
        { 1, 0, 0, 0, 0, 1},
        { 1, 1, 1, 1, 1, 1}
        };

        complexTasks[0] = new int[,]{
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 3, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0}
        };

        complexTasks[2] = new int[,]{
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 3, 0},
        { 0, 0, 1, 2, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 3, 0, 0, 3, 0},
        { 0, 0, 0, 0, 0, 0}
        };
        complexTasks[3] = new int[,]{
        { 0, 0, 0, 0, 0, 0},
        { 0, 1, 0, 0, 1, 0},
        { 0, 0, 0, 3, 0, 0},
        { 0, 0, 3, 2, 0, 0},
        { 0, 1, 0, 0, 1, 0},
        { 0, 0, 0, 0, 0, 0}
        };
    }
    
    public void GetStructure(int numOfStructure,float time) 
    {
        GameObject[,] blocks2 = new GameObject[6, 6];

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                int index = i * 6 + j;
                blocks2[i, j] = blocks[index];
            }
        }


        for (int i = 0; i < 6; i++)
        {
           for(int j = 0; j < 6; j++)
            {
                SmoothTransformChanger anim = blocks2[i,j].GetComponent<SmoothTransformChanger>();
                anim.AnimateHeight(complexHeight[numOfStructure][i,j],time);
            }

        }
    }
    public void GetTask(int numOfStructure, float time)
    {
        GameObject[,] blocks2 = new GameObject[6, 6];

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                int index = i * 6 + j;
                blocks2[i, j] = blocks[index];
            }
        }


        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                BlockSpawner blockSpawner = blocks2[i, j].GetComponent<BlockSpawner>();
                blockSpawner.SpawnMultiple(complexTasks[numOfStructure][i, j]);
            }

        }
    }

    public IEnumerator WaitAndChangeStructure(int numOfStructure, int NumOfTask)
    {
        
        manager.CanChangelvl = false;
        GetStructure(numOfStructure, time);
        yield return new WaitForSeconds(time);
        UpdateNavMesh();
        GetTask(NumOfTask, time);
        //  BlockSpawner blockSpawner = blocks[0].GetComponent<BlockSpawner>();
        //  blockSpawner.SpawnMultiple(0);
        manager.CanChangelvl = true;
    }
    
}
