using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitScript : MonoBehaviour
{
    private Transform target; // Целевой объект, к которому должен быть повёрнут объект
    [SerializeField]
    private int increaseAmount = 50;
    [SerializeField]
    private bool IsBig = true;
    [SerializeField]
    [Header("0 аптечка, 1 броня, 2 патроны")]
    private int WhatShouldAdd = 0;
    [Header("Pickup Sound")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float pickupVolume = 1f;


    public Transform player;
   
    void Start()
    {
       


        // Находим игрока по тегу "Player" и присваиваем его трансформ в target
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
    private void OnTriggerEnter(Collider other)
    {
        // Работаем только если в триггер вошёл именно игрок
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        if (WhatShouldAdd == 0)
        {
            playerHealth.Heal(increaseAmount, IsBig);
        }
        else if (WhatShouldAdd == 1)
        {
            playerHealth.AddArmor(increaseAmount, IsBig);
        }
        else if (WhatShouldAdd == 2)
        {
            GiveAmmoToAllWeapons(other.transform);
        }
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        Destroy(gameObject);
    }

    // Добавляем патроны каждому оружию-ребёнку игрока (включая выключенные)
    private void GiveAmmoToAllWeapons(Transform playerTransform)
    {
        // true = включая неактивные объекты (оружие в инвентаре)
        Weapon[] weapons = playerTransform.GetComponentsInChildren<Weapon>(true);

        if (weapons.Length == 0)
        {
            Debug.LogWarning("У игрока не найдено ни одного Weapon в детях!");
            return;
        }

        foreach (Weapon w in weapons)
        {
            if (w == null || w.weaponData == null) continue;
            // Каждому оружию — патронов ровно на один магазин
            w.GetAmmo(w.MagazineSize);
        }
    }
}
