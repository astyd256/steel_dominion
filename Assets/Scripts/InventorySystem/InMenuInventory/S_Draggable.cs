using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class S_Draggable : MonoBehaviour
{
#if !UNITY_SERVER
    [SerializeField] private string type;
    [SerializeField] private string place;

    [SerializeField] private bool panelRemoveReady;

    private bool weightFromPanelRemoveFlag = false;
    private bool FrameCheck = false;
    private Color _color;
    [SerializeField] private Vector2 _size;

    //private float _movementTime = 15f;

    public bool GetFrameCheck()
    {
        return FrameCheck;
    }
    public Vector2 GetSize()
    {
        return _size;
    }
    public string GetDraggableType()
    {
        return type;
    }
    public void SetPlaceUnitPanel()
    {
        this.place = "UnitPanel";
    }

    public void SetPlaceInventoryUnits()
    {
        this.place = "InventoryUnits";
    }


    public string GetPlace()
    {
        return place;
    }

    public void SetSize()
    {
        /*
        if (this.transform.parent.CompareTag("UnitPanel"))
        {
            _size = this.GetComponentInParent<S_CurrentUnitsPanel>().GetComponent<GridLayoutGroup>().cellSize;
        }
        else if (this.transform.parent.CompareTag("InventoryUnits"))
        {
            _size = GameObject.Find("MainMenuManager").GetComponent<S_InventoryMenuManager>().GetSlotSize();
        }
        */
        _size = this.gameObject.GetComponent<BoxCollider2D>().size;
    }

    private void Start()
    {
        panelRemoveReady = false;
        FrameCheck = false;
        // Initialize PLACE variable
        _color = this.GetComponent<Image>().color;
        if (this.transform.parent.CompareTag("InventoryUnits"))
        {
            this.place = "InventoryUnits";
            _size = GameObject.Find("MainMenuManager").GetComponent<S_InventoryMenuManager>().GetSlotSize();
        }
        else if (this.transform.parent.CompareTag("UnitPanel"))
        {
            this.place = "UnitPanel";
            _size = this.GetComponentInParent<S_CurrentUnitsPanel>().GetComponent<GridLayoutGroup>().cellSize;
        }
    }

    // IT DRAGS ONLY COPY OF AN OBJECT
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("UnitPanel")){
            //                                    FROM INVENTORY TO PANEL  OR  FROM PANEL IF NOT FROM FIRST INSTANT ENTER TRIGGER
            if ((type == "InventoryUnitSlot" && place == "InventoryUnits") || (place == "UnitPanel" && weightFromPanelRemoveFlag == true))
            {
                GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().RosterWeight += this.GetComponent<S_InventoryUnitSlot>().GetUnitWeight();
                GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().UpdateRosterWeight();
                weightFromPanelRemoveFlag = false;
            }

            if (type == "InventoryUnitSlot" &&
                this.GetComponent<S_InventoryUnitSlot>().GetBelongsToUnitsPanel() == false && GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().GetPlacingSlotBool() == false)
            {
                // From inventory slot drag over panel
                this.GetComponent<Image>().color = Color.black;

                // Shuffle from outside of panel preview start:
                other.GetComponent<S_CurrentUnitsPanel>().AddingSlotPreviewShuffle();

                if (other.TryGetComponent<S_CurrentUnitsPanel>(out S_CurrentUnitsPanel panel))
                {
                    panel.AddingSlotPreviewStart(this.GetComponent<S_InventoryUnitSlot>());
                }

                // Placing = potential placement (preview active)
                GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().SetPlacingSlotBool(true);
            }
            else if (type == "InventoryUnitSlot" &&
                this.GetComponent<S_InventoryUnitSlot>().GetBelongsToUnitsPanel() == true && GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().GetPlacingSlotBool() == false)
            {
                // From Panel Slot drag over panel
                panelRemoveReady = false;
                this.GetComponent<Image>().color = GameObject.Find("MainMenuManager").gameObject.GetComponent<S_InventoryMenuManager>().ButtonColor;

                //Shuffle within panel:
                other.GetComponent<S_CurrentUnitsPanel>().AddingSlotPreviewShuffle();
                other.GetComponent<S_CurrentUnitsPanel>().ShuffleFromWithinPreviewStart(this.GetComponent<S_InventoryUnitSlot>());

                // Preview active == true
                GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().SetPlacingSlotBool(true);
            }

            FrameCheck = true;
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (other.CompareTag("UnitPanel") && type == "InventoryUnitSlot")
        {
            if (this.GetComponent<S_InventoryUnitSlot>().GetBelongsToUnitsPanel() == false)
            {
                // From Inventory exit panel
                GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().SetPlacingSlotBool(false);
                this.GetComponent<Image>().color = _color;

                // Shuffle outside panel preview end:
                other.GetComponent<S_CurrentUnitsPanel>().AddingSlotPreviewEnd(this.GetComponent<S_InventoryUnitSlot>());
            }

            if (this.GetComponent<S_InventoryUnitSlot>().GetBelongsToUnitsPanel() == true)
            {
                // From Panel exit panel
                weightFromPanelRemoveFlag = true;
                GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().SetPlacingSlotBool(false);

                
                // Remove Unit
                panelRemoveReady = true;
                this.GetComponent<Image>().color = Color.black;

                // Shuffle within panel preview end:
                other.GetComponent<S_CurrentUnitsPanel>().ShuffleFromWithinPreviewEnd(this.GetComponent<S_InventoryUnitSlot>());
                
            }
            // First frame doesn't run this somehow
            GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().RosterWeight -= this.GetComponent<S_InventoryUnitSlot>().GetUnitWeight();
            GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().UpdateRosterWeight();
        }
    }

    public void SetPanelRemoveReady(bool b)
    {
        panelRemoveReady = b;
    }

    public bool GetPanelRemoveReady()
    {
        return panelRemoveReady;
    }

#endif
}
