using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowGrenade : MonoBehaviour
{
    [Header("Throw Settings")]
    public GameObject grenadePrefab;    // Префаб гранаты
    public float throwForce = 15f;      // Сила броска
    public Transform throwPoint;        // Точка, из которой вылетает граната (обычно перед камерой)

    [Header("Key Settings")]
    public KeyCode throwKey = KeyCode.G; // Клавиша для броска

    void Update()
    {
        // Если нажата клавиша G
        if (Input.GetKeyDown(throwKey))
        {
            ThrowwGrenade();
        }
    }

    void ThrowwGrenade()
    {
        // 1. Создаем гранату в точке броска
        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);

        // 2. Получаем компонент Rigidbody у созданной гранаты
        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // 3. Придаем гранате скорость в направлении "вперед" от точки броска[reference:8][reference:9]
            rb.velocity = throwPoint.forward * throwForce;
        }
    }
}
