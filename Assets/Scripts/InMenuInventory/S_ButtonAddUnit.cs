using UnityEngine;
using UnityEngine.UI;

public class S_ButtonAddUnit : MonoBehaviour
{
    public S_InventoryItemData refData;
    public Button button;
    public GameObject invobj;
    void Start()
    {
        Button btn = button.GetComponent<Button>();
        btn.onClick.AddListener(AddUnit);
    }

    void AddUnit()
    {
        S_InventorySystem inv = invobj.GetComponent<S_InventorySystem>();
        inv.Add(refData);
    }
}
