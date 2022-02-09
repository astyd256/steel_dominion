[System.Serializable]
public class S_InventorySlotItem
{
    public S_InventoryItemData data;

    public S_InventorySlotItem(S_InventoryItemData source)
    {
        data = source;
    }
}
