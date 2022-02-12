using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_InventoryGameSystem : MonoBehaviour
{
    private List<S_InventorySlot> inventoryGame = new List<S_InventorySlot>();
    [SerializeField] private GameObject buttonPrefab = null;

    public void addItem()
    {
      //  GameObject newBtn = Instantiate(buttonPrefab);
    }
}
