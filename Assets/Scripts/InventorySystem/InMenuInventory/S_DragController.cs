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

    [SerializeField] private Transform unitCopyParent;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color dragColor;

    void Update()
    {
        // Drop on mouseUp or touch.end
        if (_isDragActive && (Input.GetMouseButtonUp(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)))
        {
            Drop();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            _initialPosition = new Vector2(mousePos.x, mousePos.y);
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
        else
        {
            
            RaycastHit2D hit = Physics2D.Raycast(_initialPosition, Vector2.zero);
            if (hit.collider != null && hit.collider.tag != "ValidDrop" && ((_screenPosition.x >= _initialPosition.x + 50) || 
                (_screenPosition.x <= _initialPosition.x - 50) || (_screenPosition.y >= _initialPosition.y + 50) ||
                (_screenPosition.y <= _initialPosition.y - 50)))
            {
                // Only works with collider
                // Need to create a copy of this object
                S_Draggable draggable = Instantiate(hit.transform.gameObject.GetComponent<S_Draggable>(), unitCopyParent);
                // Width and Height setting:
                RectTransform rt = draggable.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(155, 155);

                if (draggable != null)
                {
                    _currentDragged = draggable; // Copy
                    _lastDragged = hit.transform.gameObject.GetComponent<S_Draggable>(); // Copied object
                    initDrag();
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

        if (_currentDragged.GetDraggableType() == "InventoryUnitSlot")
        {
            
        }

        Destroy(_currentDragged.gameObject);

    }

}
