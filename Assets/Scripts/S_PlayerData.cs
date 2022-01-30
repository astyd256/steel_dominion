using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class S_PlayerData
{
    public string playername;
    public List<string> itemids;

    public S_PlayerData(string name, List<string> items)
    {
        playername = name;
        itemids = items;
    }
}
