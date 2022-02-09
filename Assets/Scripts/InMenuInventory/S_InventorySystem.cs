using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_InventorySystem : MonoBehaviour
{
    public List<S_InventoryItemData> inventory;

    public Text nickText;

    public void SavePlayer()
    {
        List<string> itemids;
        itemids = new List<string>();

        inventory.ForEach(delegate (S_InventoryItemData item)
        {
            itemids.Add(item.id);
        });

        S_SavePlayerData.SavePlayer(nickText.text, itemids);
    }

    public void LoadPlayer()
    {
        S_SavePlayerData.LoadPlayer();
    }
    private void Awake()
    {
        inventory = new List<S_InventoryItemData>();
    }

    public void Add(S_InventoryItemData refData)
    {
        inventory.Add(refData);
    }

    public void Remove(int removeIndex)
    {
        inventory.RemoveAt(removeIndex);
    }

    public S_InventoryItemData Get(int getIndex)
    {
        if (inventory.Count >= getIndex)
        {
            return inventory[getIndex];
        }
        return null;
    }

}
