using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_UnitButton : MonoBehaviour
{
    public event Action<int> ClientUnitClicked;

    public int id = 0;
    //Recolor button on toggle and save button id to gameplayer
    public void UseUnit()
    {
        ClientUnitClicked?.Invoke(id);
        Destroy(gameObject);
    }
}
