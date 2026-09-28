using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    public GameObject[] weapons; // 4 объекта оружия

    private int currentWeaponIndex = 0; // индекс активного оружия (0 = первое)

    void Start()
    {
        // Включаем только первое оружие, остальные выключаем
        //SwitchWeapon(0);
    }

    void Update()
    {
        // Проверяем нажатие цифровых клавиш 1-4
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchWeapon(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchWeapon(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            SwitchWeapon(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            SwitchWeapon(4);
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            SwitchWeapon(5);
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            SwitchWeapon(6);

        // Дополнительно: колёсико мыши для переключения (опционально)
        // float scroll = Input.GetAxis("Mouse ScrollWheel");
        // if (scroll != 0)
        // {
        //     int next = currentWeaponIndex + (scroll > 0 ? 1 : -1);
        //     if (next < 0) next = weapons.Length - 1;
        //     if (next >= weapons.Length) next = 0;
        //     SwitchWeapon(next);
        // }
    }

    // Метод переключения на оружие по индексу
    void SwitchWeapon(int index)
    {
        // Проверяем, что индекс корректен и массив не пуст
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("Нет оружия в массиве!");
            return;
        }

        if (index < 0 || index >= weapons.Length)
        {
            Debug.LogWarning("Некорректный индекс оружия: " + index);
            return;
        }

        // Если это оружие уже активно, ничего не делаем (по желанию)
        if (currentWeaponIndex == index)
            return;

        // Выключаем текущее оружие (если оно существует)
        if (weapons[currentWeaponIndex] != null)
            weapons[currentWeaponIndex].SetActive(false);

        // Включаем новое оружие
        if (weapons[index] != null)
            weapons[index].SetActive(true);

        // Запоминаем новый индекс
        currentWeaponIndex = index;
    }
}
