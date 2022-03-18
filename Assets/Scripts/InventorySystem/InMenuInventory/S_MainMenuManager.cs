using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_MainMenuManager : MonoBehaviour
{
    [SerializeField] private S_InventoryMenuManager inventoryMenuManager = null;
    [SerializeField] private S_CurrentUnitsPanel currentUnitsPanel = null;
    [SerializeField] public Color ActiveButtonColor;
    [SerializeField] public Color ButtonColor;

    public void SavePlayer()
    {
        List<int> unitsIds = new List<int>();

        foreach(Transform unit in currentUnitsPanel.transform)
        {
            int id = unit.gameObject.GetComponent<S_InventoryUnitSlot>().GetUnitData().GetId();
            unitsIds.Add(id);

        }

       // foreach (var unit in inventoryMenuManager.GetUnits())
       // {
       //     unitsIds.Add(unit.id);
       // }

        S_SavePlayerData.SavePlayer("Default", unitsIds);
    }

    public void LoadPlayer()
    {
        // S_SavePlayerData.LoadPlayer();
    }
}
