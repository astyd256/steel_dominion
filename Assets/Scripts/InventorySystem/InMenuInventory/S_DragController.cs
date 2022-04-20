using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_DragController : MonoBehaviour
{

    private bool _isDragActive = false;

    private Vector2 _screenPosition;

    private Vector2 _initialPosition;

    private S_Draggable _lastDragged;

    private S_Draggable _currentDragged;

    private RaycastHit2D hit;

    [SerializeField] private S_CurrentUnitsPanel currentUnitsPanel = null;
    [SerializeField] private Transform unitCopyParent;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color dragColor;
    [SerializeField] private Color holdColor;
    [SerializeField] public Color addedColor;
    [SerializeField] private Vector2 draggableColliderSize;
    [SerializeField] private S_MainMenuManager mainMenuManager;

    private void Start()
    {
    }

    void Update()
    {
        if (mainMenuManager.getInventoryActive())
        {
            // Drop on mouseUp or touch.end
            if (_isDragActive && (Input.GetMouseButtonUp(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)))
            {
                Drop();
                return;
            }

            if (Input.GetMouseButtonDown(0)) // Initial position take
            {
                Vector3 mousePos = Input.mousePosition;
                _initialPosition = new Vector2(mousePos.x, mousePos.y);
                int layermask = 1 << 5;
                hit = Physics2D.Raycast(_initialPosition, Vector2.zero, 1000f, layermask); // taken
                if (hit.collider != null && hit.collider.tag == "UnitSlot") // If valid object
                {
                    _lastDragged = hit.transform.gameObject.GetComponent<S_Draggable>(); // Copied object
                    _lastDragged.GetComponent<Image>().color = holdColor;
                }
            }
            if (Input.GetMouseButtonUp(0)) // Clickhold highlight end
            {
                if (hit.collider != null && hit.collider.tag == "UnitSlot") // If valid object
                {
                    if (_lastDragged.GetComponent<S_InventoryUnitSlot>().GetCanDrag() == true)
                    {
                        _lastDragged.GetComponent<Image>().color = normalColor;
                    }
                    else if (_lastDragged.GetComponent<S_InventoryUnitSlot>().GetCanDrag() == false)
                    {
                        _lastDragged.GetComponent<Image>().color = addedColor;
                    }
                }
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos = Input.mousePosition;
                _screenPosition = new Vector2(mousePos.x, mousePos.y);
            }
            else if (Input.touchCount > 0)
            {
                _screenPosition = Input.GetTouch(0).position;
            }
            else
            {
                return;
            }

            if (_isDragActive)
            {
                Drag();
            }
            else // Try drag start
            {
                // Here was raycast before
                if (hit.collider != null && hit.collider.tag == "UnitSlot" && ((_screenPosition.x >= _initialPosition.x + 30) ||
                    (_screenPosition.x <= _initialPosition.x - 30) || (_screenPosition.y >= _initialPosition.y + 30) ||
                    (_screenPosition.y <= _initialPosition.y - 30)))
                {
                    if (hit.transform.gameObject.GetComponent<S_InventoryUnitSlot>().GetCanDrag() == true)
                    {

                        // Only works with collider
                        // NEED TO SET APPROPRIATE SIZE AND ADD ANIMATIONS IN FUTURE:::

                        if (hit.transform.gameObject.GetComponent<S_Draggable>().GetDraggableType() == "InventoryUnitSlot")
                        {   // FROM INVENTORY
                            if (hit.transform.gameObject.GetComponent<S_Draggable>().GetPlace() == "InventoryUnits")
                            {
                                // Need to create a copy of this object
                                S_Draggable draggable = Instantiate(hit.transform.gameObject.GetComponent<S_Draggable>(), unitCopyParent);
                                // Width and Height setting:
                                RectTransform rt = draggable.GetComponent<RectTransform>();
                                // Rigid body for interaction
                                draggable.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                                // Draggable size
                                rt.sizeDelta = hit.transform.gameObject.GetComponent<S_Draggable>().GetSize();
                                // Collider size
                                draggable.GetComponent<BoxCollider2D>().size = draggableColliderSize;

                                if (draggable != null)
                                {
                                    _currentDragged = draggable; // Copy
                                    _currentDragged.GetComponent<Image>().color = normalColor;
                                    initDrag();
                                }

                            } // FROM PANEL
                            if (hit.transform.gameObject.GetComponent<S_Draggable>().GetPlace() == "UnitPanel")
                            {
                                //Collider size
                                hit.transform.gameObject.GetComponent<BoxCollider2D>().size = draggableColliderSize;
                                hit.transform.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                                hit.transform.gameObject.GetComponent<S_Draggable>().transform.SetParent(GameObject.Find("DragController").transform);
                                hit.transform.gameObject.GetComponent<Image>().color = normalColor;
                                //Size of remaining elements change
                                if (currentUnitsPanel.GetSlotsCount() > 0)
                                {
                                    if (currentUnitsPanel.GetSlotsCount() - 1 > 0)
                                    {
                                        currentUnitsPanel.SetLayoutGroupSize(currentUnitsPanel.panelWidth / (currentUnitsPanel.GetSlotsCount() - 1), currentUnitsPanel.panelHeight);
                                    }
                                    currentUnitsPanel.RosterWeight = (currentUnitsPanel.RosterWeight - hit.transform.gameObject.GetComponent<S_InventoryUnitSlot>().GetUnitWeight());

                                }

                                _currentDragged = hit.transform.gameObject.GetComponent<S_Draggable>();
                                initDrag();
                            }
                        }
                    }
                }
            }
        }
    }

    void initDrag()
    {
        _isDragActive = true;
        _lastDragged.GetComponent<Image>().color = dragColor;
    }

    void Drag()
    {
        // Position change
        _currentDragged.transform.position = new Vector2(_screenPosition.x, _screenPosition.y);
    }
    void Drop()
    {
        _isDragActive = false;
        _lastDragged.GetComponent<Image>().color = normalColor;

        if (_currentDragged.GetDraggableType() == "InventoryUnitSlot" && _currentDragged.GetPlace() == "InventoryUnits"
            && currentUnitsPanel.previewActive == true)
        {
            if (currentUnitsPanel.OverWeight == false) // If not overweight
            {
                // Took from Inventory, drop on Panel
                // ADD ORIGINAL
                //currentUnitsPanel.AddingSlotPreviewEnd(this.GetComponent<S_InventoryUnitSlot>());
                currentUnitsPanel.AddUnitSLot(_lastDragged.GetComponent<S_InventoryUnitSlot>());
                _lastDragged.GetComponent<Image>().color = addedColor;

                _currentDragged.SetPlaceUnitPanel();
                // Roster weight control (Because drop triggers OnTriggerExit, we need to add the value)
                currentUnitsPanel.RosterWeight += _currentDragged.GetComponent<S_InventoryUnitSlot>().GetUnitWeight();
                currentUnitsPanel.UpdateRosterWeight();

                // SAVE BUTTON ACTIVE:
                GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().SaveInventoryButton.SetActive(true);
            }

        }
        else if (_currentDragged.GetPlace() == "UnitPanel" && _currentDragged.GetComponent<S_Draggable>().GetPanelRemoveReady())
        {
            // Took from Panel, drop off
            // If from unitpanel and left boundaries (collision box) then remove
            _currentDragged.SetPlaceInventoryUnits();
            currentUnitsPanel.RemoveUnitFromPanel(_lastDragged.GetComponent<S_InventoryUnitSlot>());
            // Roster weight change
            currentUnitsPanel.ChangePlace(_lastDragged.GetComponent<S_InventoryUnitSlot>());
            // RosterWeightChange at Draggable Trigger2D

            // SAVE BUTTON ACTIVE:
            GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().SaveInventoryButton.SetActive(true);
        }
        else if (_currentDragged.GetPlace() == "UnitPanel" && _currentDragged.GetComponent<S_Draggable>().GetPanelRemoveReady() == false)
        {
            // Took from panel, drop on panel
            Destroy(_currentDragged.GetComponent<Rigidbody2D>()); // MUSTHAVE
            _currentDragged.GetComponent<S_Draggable>().transform.SetParent(currentUnitsPanel.transform);
            GameObject.Find("CurrentUnitsParent").GetComponent<S_CurrentUnitsPanel>().SetPlacingSlotBool(false);
            // Size adjust on panel
            if (currentUnitsPanel.GetSlotsCount() > 0)
            {
                currentUnitsPanel.SetLayoutGroupSize(currentUnitsPanel.panelWidth / currentUnitsPanel.GetSlotsCount(), currentUnitsPanel.panelHeight);
            }
            hit.transform.gameObject.GetComponent<BoxCollider2D>().size = currentUnitsPanel.GetComponent<GridLayoutGroup>().cellSize;
        }


        if ((_currentDragged.GetPlace() == "UnitPanel" && _currentDragged.GetComponent<S_Draggable>().GetPanelRemoveReady())
            || (_currentDragged.GetDraggableType() == "InventoryUnitSlot" && _currentDragged.GetPlace() == "InventoryUnits"))
        {
            // Took from Panel, drop off
            // Took from Inventory, drop on
            Destroy(_currentDragged.gameObject);
        }

    }

}
