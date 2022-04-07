using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class S_PlayerData
{
    public string playername;
    public List<int> unitData;

    public S_PlayerData(string name, List<int> unitdata)
    {
        playername = name; 
        unitData = unitdata;
    }
}
