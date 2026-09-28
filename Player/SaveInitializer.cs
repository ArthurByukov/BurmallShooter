using UnityEngine;
using YG;

/// <summary>
/// Инициализирует массив weaponLevels в сохранениях YG2 под размер WeaponDatabase.
/// Вешается на любой объект в сцене, которая загружается первой (например, на _Managers).
/// </summary>
public class SaveInitializer : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Тот же WeaponDatabase, что используется во всём проекте")]
    public WeaponDatabase database;

    [Header("Options")]
    [Tooltip("Выдавать ли нож бесплатно при первом запуске (true) и экипировать в Melee-слот")]
    public bool grantStarterMelee = true;

    [Tooltip("weaponID ножа из WeaponDatabase")]
    public int starterMeleeID = 0;

    void Awake()
    {
        if (database == null || database.allWeapons == null)
        {
            Debug.LogError("[SaveInitializer] WeaponDatabase не назначен или пуст!");
            return;
        }

        int count = database.allWeapons.Length;
        if (count == 0)
        {
            Debug.LogWarning("[SaveInitializer] В WeaponDatabase нет оружия.");
            return;
        }

        // ---- 1. Приводим длину массива weaponLevels к размеру БД ----
        if (YG2.saves.weaponLevels == null || YG2.saves.weaponLevels.Length != count)
        {
            int[] newArr = new int[count];
            for (int i = 0; i < count; i++)
            {
                newArr[i] = (YG2.saves.weaponLevels != null && i < YG2.saves.weaponLevels.Length)
                    ? YG2.saves.weaponLevels[i]   // сохраняем старые данные
                    : -1;                          // новое оружие = не куплено
            }
            YG2.saves.weaponLevels = newArr;
            Debug.Log($"[SaveInitializer] weaponLevels создан/расширен до {count}.");
        }

        // ---- 2. Выдаём стартовый нож бесплатно (первый запуск) ----
        if (grantStarterMelee)
            GrantStarterMelee();

        // ---- 3. Сохраняем ----
        YG2.SaveProgress();
    }

    void GrantStarterMelee()
    {
        int idx = database.GetIndexByID(starterMeleeID);
        if (idx < 0)
        {
            Debug.LogWarning($"[SaveInitializer] Нож с weaponID={starterMeleeID} не найден в БД.");
            return;
        }

        // Открываем нож на уровне 0, если он ещё не открыт
        if (YG2.saves.weaponLevels[idx] < 0)
        {
            YG2.saves.weaponLevels[idx] = 0;
            Debug.Log("[SaveInitializer] Стартовый нож выдан (уровень 0).");
        }

        // Автоматически экипируем в Melee-слот, если слот пуст
        if (YG2.saves.equippedMelee < 0)
        {
            YG2.saves.equippedMelee = starterMeleeID;
            Debug.Log("[SaveInitializer] Стартовый нож экипирован в Melee-слот.");
        }
    }
}