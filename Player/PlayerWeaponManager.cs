using UnityEngine;
using YG;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Refs")]
    public WeaponDatabase database;

    [Tooltip("Все оружия на игроке (включая выключенные). Можно оставить пустым — найдём автоматически.")]
    public Weapon[] allWeapons;

    [Header("Runtime")]
    private Weapon[] slotWeapon = new Weapon[3];   // [0]=primary, [1]=secondary, [2]=melee
    private int currentSlot = 0;

    public int CurrentSlot => currentSlot;
    public Weapon CurrentWeapon => slotWeapon[currentSlot];

    void Awake()
    {
        if (allWeapons == null || allWeapons.Length == 0)
            allWeapons = GetComponentsInChildren<Weapon>(true);

        // На старте выключаем всё
        foreach (var w in allWeapons)
            if (w != null) w.gameObject.SetActive(false);
    }

    void Start()
    {
        BuildSlotsFromSaves();
        SelectSlot(FirstNonEmptySlot());
    }

    void Update()
    {
        if (MenuManager.GameplayBlocked) return;   
        // Во время перезарядки смена оружия запрещена (и клавишами, и колесом)
        if (CurrentWeapon != null && CurrentWeapon.IsReloading)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.01f) CycleSlot(1);
        else if (scroll < -0.01f) CycleSlot(-1);
    }

    // ---- Слоты ----

    void BuildSlotsFromSaves()
    {
        slotWeapon[0] = FindWeaponByID(YG2.saves.equippedPrimary);
        slotWeapon[1] = FindWeaponByID(YG2.saves.equippedSecondary);
        slotWeapon[2] = FindWeaponByID(YG2.saves.equippedMelee);

        // применяем уровень к каждому
        ApplySavedLevel(slotWeapon[0], YG2.saves.equippedPrimary);
        ApplySavedLevel(slotWeapon[1], YG2.saves.equippedSecondary);
        ApplySavedLevel(slotWeapon[2], YG2.saves.equippedMelee);
    }

    Weapon FindWeaponByID(int id)
    {
        if (id < 0) return null;
        foreach (var w in allWeapons)
            if (w != null && w.weaponData != null && w.weaponData.weaponID == id)
                return w;
        return null;
    }

    void ApplySavedLevel(Weapon w, int id)
    {
        if (w == null || id < 0) return;
        int dbIndex = database.GetIndexByID(id);
        if (dbIndex < 0) return;
        int level = YG2.saves.weaponLevels[dbIndex];
        if (level < 0) level = 0;
        w.SetLevel(level);
    }

    public void SelectSlot(int slot)
    {
        if (slot < 0 || slot > 2) return;
        if (slotWeapon[slot] == null) return;   // пустой слот — ничего не делаем

        // Нельзя переключаться во время перезарядки
        if (slotWeapon[currentSlot] != null && slotWeapon[currentSlot].IsReloading)
            return;

        if (slot == currentSlot) return;        // уже в руках — не дёргаем анимацию

        // выключаем текущее
        if (slotWeapon[currentSlot] != null)
            slotWeapon[currentSlot].gameObject.SetActive(false);

        currentSlot = slot;
        slotWeapon[currentSlot].gameObject.SetActive(true);
    }

    void CycleSlot(int dir)
    {
        for (int i = 1; i <= 3; i++)
        {
            int next = (currentSlot + dir * i + 3) % 3;
            if (slotWeapon[next] != null)
            {
                SelectSlot(next);
                return;
            }
        }
    }

    int FirstNonEmptySlot()
    {
        for (int i = 0; i < 3; i++) if (slotWeapon[i] != null) return i;
        return 0;
    }

    // Вызывать после изменения экипировки в магазине
    public void RefreshLoadout()
    {
        int prev = currentSlot;
        if (slotWeapon[prev] != null) slotWeapon[prev].gameObject.SetActive(false);
        BuildSlotsFromSaves();
        SelectSlot(FirstNonEmptySlot());
    }
}