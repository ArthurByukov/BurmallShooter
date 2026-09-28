using UnityEngine;

public enum WeaponType { Primary, Secondary, Melee }

[System.Serializable]
public class WeaponLevelData
{
    public float damage;
    public float range;
    public int magazineSize;
    public int maxAmmo;
    public float reloadTime;
    public float attackCooldown;
}

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public int weaponID;                 // уникальный ID в БД (для сохранений)
    public string weaponName;
    public WeaponType weaponType;        // какой слот занимает
    public Sprite icon;

    [Header("Shop")]
    public int price = 100;              // цена покупки (ур. 0)
    public int[] upgradeCosts = new int[3]; // стоимость перехода 0→1, 1→2, 2→3

    [Header("Recoil")]
    public float recoilUp = 2f;
    public float recoilSide = 1f;
    public float recoilReturnTime = 0.5f;

    [Header("Aim")]
    public bool canAim = false;
    public float aimFOV = 40f;
    public float aimTransitionTime = 0.3f;

    [Header("Levels")]
    public WeaponLevelData[] levels;     // 4 элемента: 0..3
    public int MaxLevel => levels.Length - 1;
}