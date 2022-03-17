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

    private string dragSource;

    [SerializeField] private S_CurrentUnitsPanel currentUnitsPanel = null;
    [SerializeField] private Transform unitCopyParent;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color dragColor;
    [SerializeField] private Color holdColor;
    [SerializeField] private Color addedColor;

    void Update()
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
            hit = Physics2D.Raycast(_initialPosition, Vector2.zero); // taken
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
            if (hit.collider != null && hit.collider.tag == "UnitSlot" && ((_screenPosition.x >= _initialPosition.x + 50) || 
                (_screenPosition.x <= _initialPosition.x - 50) || (_screenPosition.y >= _initialPosition.y + 50) ||
                (_screenPosition.y <= _initialPosition.y - 50)))
            {
                if (hit.transform.gameObject.GetComponent<S_InventoryUnitSlot>().GetCanDrag() == true)
                {

                    // Only works with collider
                    // Need to create a copy of this object
                    S_Draggable draggable = Instantiate(hit.transform.gameObject.GetComponent<S_Draggable>(), unitCopyParent);
                    // Width and Height setting:

                    // NEED TO SET APPROPRIATE SIZE AND ADD ANIMATIONS IN FUTURE:::

                    RectTransform rt = draggable.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(155, 155);

                    ///////////////////////////////////////////////////////////////

                    // Need to add rigid body for interaction:
                    draggable.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

                    if (draggable != null)
                    {
                        _currentDragged = draggable; // Copy
                        _currentDragged.GetComponent<Image>().color = normalColor;
                        initDrag();
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
            )
        {
            // if && currentUnitsPanel.previewActive == true
            // Code for adding unit to panel
            // ADD ORIGINAL
            currentUnitsPanel.AddUnitSLot(_lastDragged.GetComponent<S_InventoryUnitSlot>()); 
            _lastDragged.GetComponent<Image>().color = addedColor;

        }

        Destroy(_currentDragged.gameObject);

    }

}
