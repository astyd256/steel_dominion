using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_Draggable : MonoBehaviour
{
    [SerializeField] private string type;
    [SerializeField] private string place;

    private Collider2D _collider;

    private Color _color;
    private Vector2 _size;

    //private float _movementTime = 15f;

    public void SetSize(int x, int y)
    {
        _size = new Vector2(x, y);
    }

    public void SetPlace(string place)
    {
       this.place = place;
    }
    public string GetPlace()
    {
        return place;
    }
    public string GetDraggableType()
    {
        return type;
    }

    private void Start()
    {
        // Initialize PLACE variable
        _color = this.GetComponent<Image>().color;
        _collider = GetComponent<Collider2D>();
        if (this.transform.parent.CompareTag("InventoryUnits"))
        {
            this.place = "InventoryUnits";
            _size = new Vector2(155, 155);
        }
        else if (this.transform.parent.CompareTag("UnitPanel"))
        {
            this.place = "UnitPanel";
            _size = this.GetComponentInParent<S_CurrentUnitsPanel>().GetSize();
        }
    }

    // IT DRAGS ONLY COPY OF AN OBJECT
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("UnitPanel") && type == "InventoryUnitSlot" && 
            this.GetComponent<S_InventoryUnitSlot>().GetBelongsToUnitsPanel() == false)
        {
            this.GetComponent<Image>().color = Color.black;

            // Code for preview of putting unit on panel:
            other.GetComponent<S_CurrentUnitsPanel>().AddingSlotPreviewStart(this.GetComponent<S_InventoryUnitSlot>());

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("UnitPanel") && type == "InventoryUnitSlot" &&
            this.GetComponent<S_InventoryUnitSlot>().GetBelongsToUnitsPanel() == false)
        {
            this.GetComponent<Image>().color = _color;

            // Preview end call:
            other.GetComponent<S_CurrentUnitsPanel>().AddingSlotPreviewEnd(this.GetComponent<S_InventoryUnitSlot>());
        }
        else if (this.GetComponent<S_InventoryUnitSlot>().GetBelongsToUnitsPanel() == true)
        {
            // Remove Unit
        }
    }

}
