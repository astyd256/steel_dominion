using UnityEngine;

[CreateAssetMenu(menuName = "Inventory Item Data")]
public class S_InventoryItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public GameObject prefab;
    public int UnitWeight = 1;
}
