using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class S_InventorySlot : MonoBehaviour
{
   // private SO_InventoryItemData item = null;
    private int itemIndex = -1;
    private TMP_Text slotText = null;

    void Start()
    {
        slotText = this.GetComponent<TMP_Text>();
    }

    public void SetItem()//SO_InventoryItemData Item, int itemindex)
    {
        //item = Item;
        //itemIndex = itemindex;
       // slotText.text = item.displayName;
    }
}
