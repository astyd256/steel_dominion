using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class S_SavePlayerData
{
    public static void SavePlayer (string name, List<string> items)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = "C:/Saves/MyDataSave.boo";
        FileStream stream = new FileStream(path, FileMode.Create);

        S_PlayerData pdata = new S_PlayerData(name, items);

        formatter.Serialize(stream, pdata);
        stream.Close();
    }

    public static S_PlayerData LoadPlayer()
    {
        string path = "C:/Saves/MyDataSave.boo";
        
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            S_PlayerData pdata = formatter.Deserialize(stream) as S_PlayerData;
            stream.Close();

            Debug.Log(pdata.playername);

            pdata.itemids.ForEach(delegate (string it)
            {
                Debug.Log(it);
            });

            return pdata;
        }
        else
        {
            Debug.LogError("Save file not found");
            return null;
        }
    }
}
