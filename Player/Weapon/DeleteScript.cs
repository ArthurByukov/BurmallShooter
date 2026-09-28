using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DeleteScript : MonoBehaviour
{
    private Transform target; // Целевой объект, к которому должен быть повёрнут объект
    public float delay = 1;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Destroy());
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Игрок с тегом 'Player' не найден в сцене!");
        }
    }
    void Update()
    {
        // Проверяем, что target существует, чтобы избежать ошибок
        if (target != null)
        {
            transform.LookAt(target);
        }
    }
    private void LateUpdate()
    {
        Vector3 euler = transform.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        transform.eulerAngles = euler;
    }
    private IEnumerator Destroy()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }


    }
