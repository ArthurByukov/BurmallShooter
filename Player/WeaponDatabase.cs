using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Weapons/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    public WeaponData[] allWeapons;

    public WeaponData GetByID(int id)
    {
        if (allWeapons == null) return null;
        foreach (var w in allWeapons)
            if (w != null && w.weaponID == id) return w;
        return null;
    }

    public int GetIndexByID(int id)
    {
        for (int i = 0; i < allWeapons.Length; i++)
        {
            if (allWeapons[i] != null && allWeapons[i].weaponID == id)
            {
               // Debug.Log("id=" + i);
                return i;
                
            }
        }
        return -1;
    }
}