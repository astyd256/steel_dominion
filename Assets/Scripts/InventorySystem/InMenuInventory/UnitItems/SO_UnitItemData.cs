using UnityEngine;

[CreateAssetMenu(menuName = "Inventory Item Data")]
public class SO_UnitItemData : ScriptableObject
{
    public int id;
    public string displayName;
    public GameObject prefab;
    public int UnitWeight = 1;
    public Sprite unitSprite;
    public int maxHealth = 1;
    public int minDamage = 1;
    public int maxDamage = 2;

    public enum UnitType
    {
        small,
        medium,
        big
    }

    public UnitType unitSize = UnitType.small;

    // INVENTORY ITEM
    public UnitType GetSizeType()
    {
        return unitSize;
    }

    public string GetName()
    {
        return displayName;
    }

    public Sprite GetSprite()
    {
        return unitSprite;
    }

    public int GetWeight()
    {
        return UnitWeight;
    }

    public int GetId()
    {
        return id;
    }

    public GameObject GetPrefab()
    {
        return prefab;
    }

    public int GetMinDamage()
    {
        return minDamage;
    }

    public int GetMaxDamage()
    {
        return maxDamage;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
    //////////////////
}
