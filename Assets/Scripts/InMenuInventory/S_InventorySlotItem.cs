[System.Serializable]
public class S_InventorySlotItem
{
    public S_InventoryItemData data;
    public int stackSize;

    public S_InventorySlotItem(S_InventoryItemData source)
    {
        data = source;
        AddToStack();
    }

    public void AddToStack()
    {
        stackSize++;
    }

    public void RemoveFromStack()
    {
        stackSize--;
    }
}
