using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_InventorySystem : MonoBehaviour
{
    private Dictionary<S_InventoryItemData, S_InventorySlotItem> itemDictionary;
   
    public List<S_InventorySlotItem> inventory;
    private void Awake()
    {
        inventory = new List<S_InventorySlotItem>();
        itemDictionary = new Dictionary<S_InventoryItemData, S_InventorySlotItem>();
    }

    public void Add(S_InventoryItemData refData)
    {
        if (itemDictionary.TryGetValue(refData, out S_InventorySlotItem value))
        {
            value.AddToStack();
        }
        else
        {
            S_InventorySlotItem newItem = new S_InventorySlotItem(refData);
            inventory.Add(newItem);
            itemDictionary.Add(refData, newItem);
        }
    }

    public void Remove(S_InventoryItemData refData)
    {
        if (itemDictionary.TryGetValue(refData, out S_InventorySlotItem value))
        {
            value.RemoveFromStack();

            if (value.stackSize == 0)
            {
                inventory.Remove(value);
                itemDictionary.Remove(refData);
            }
        }
    }

    public S_InventorySlotItem Get(S_InventoryItemData refData)
    {
        if (itemDictionary.TryGetValue(refData, out S_InventorySlotItem value))
        {
            return value;
        }
        return null;
    }

}
