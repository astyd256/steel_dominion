using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_MainMenuManager : MonoBehaviour
{
    [SerializeField] private S_InventoryMenuManager inventoryMenuManager = null;

    public void SavePlayer()
    {
        List<int> unitsIds = new List<int>();

        foreach (var unit in inventoryMenuManager.GetUnits())
        {
            unitsIds.Add(unit.id);
        }

        S_SavePlayerData.SavePlayer("Default", unitsIds);
    }

    public void LoadPlayer()
    {
        // S_SavePlayerData.LoadPlayer();
    }
}
