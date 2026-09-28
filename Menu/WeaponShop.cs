using UnityEngine;
using YG;

public class WeaponShop : MonoBehaviour
{
    public WeaponDatabase database;
    public PlayerWeaponManager playerWeapons; // можно не назначать — найдём на сцене

    void Awake()
    {
        if (playerWeapons == null)
            playerWeapons = FindObjectOfType<PlayerWeaponManager>();
    }

    // ---------- Проверки ----------

    public bool IsOwned(int weaponID)
    {
        int i = database.GetIndexByID(weaponID);
        //Debug.Log(i);
        Debug.Log("lenght="+YG2.saves.weaponLevels.Length);
        return i >= 0 && YG2.saves.weaponLevels[i] >= 0;
    }

    public int GetLevel(int weaponID)
    {
        int i = database.GetIndexByID(weaponID);
        if (i < 0) return -1;
        return YG2.saves.weaponLevels[i];
    }

    public bool IsEquipped(int weaponID)
    {
        return YG2.saves.equippedPrimary == weaponID
            || YG2.saves.equippedSecondary == weaponID
            || YG2.saves.equippedMelee == weaponID;
    }

    // ---------- Действия ----------

    public bool TryBuy(int weaponID)
    {
        if (IsOwned(weaponID)) return false;
        var data = database.GetByID(weaponID);
        if (data == null) return false;
        if (YG2.saves.coins < data.price) return false;

        YG2.saves.coins -= data.price;
        YG2.saves.weaponLevels[database.GetIndexByID(weaponID)] = 0;
        YG2.SaveProgress();
        return true;
    }

    public bool TryUpgrade(int weaponID)
    {
        int idx = database.GetIndexByID(weaponID);
        if (idx < 0) return false;
        int lvl = YG2.saves.weaponLevels[idx];
        if (lvl < 0) return false;

        var data = database.GetByID(weaponID);
        if (lvl >= data.MaxLevel) return false;

        int cost = (lvl < data.upgradeCosts.Length) ? data.upgradeCosts[lvl] : 0;
        if (YG2.saves.coins < cost) return false;

        YG2.saves.coins -= cost;
        YG2.saves.weaponLevels[idx] = lvl + 1;
        YG2.SaveProgress();
        return true;
    }

    public bool TryEquip(int weaponID)
    {
        if (!IsOwned(weaponID)) return false;
        var data = database.GetByID(weaponID);
        if (data == null) return false;

        switch (data.weaponType)
        {
            case WeaponType.Primary: YG2.saves.equippedPrimary = weaponID; break;
            case WeaponType.Secondary: YG2.saves.equippedSecondary = weaponID; break;
            case WeaponType.Melee: YG2.saves.equippedMelee = weaponID; break;
        }
        YG2.SaveProgress();
        return true;
    }

    public void Unequip(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Primary: YG2.saves.equippedPrimary = -1; break;
            case WeaponType.Secondary: YG2.saves.equippedSecondary = -1; break;
            case WeaponType.Melee: YG2.saves.equippedMelee = -1; break;
        }
        YG2.SaveProgress();
    }

    // Вызывать при выходе из магазина перед стартом уровня
    public void ApplyToPlayer()
    {
        if (playerWeapons != null) playerWeapons.RefreshLoadout();
    }
}