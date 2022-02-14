using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_InventoryMenuManager : MonoBehaviour
{
    [SerializeField]
    public List<SO_UnitItemData> Units = new List<SO_UnitItemData>();

    public void SavePlayer()
    {
        List<int> unitsIds = new List<int>();

        foreach(var unit in Units)
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
