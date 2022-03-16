using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_DragController : MonoBehaviour
{

    private bool _isDragActive = false;

    private Vector2 _screenPosition;

    //private Vector3 _worldPosition;

    private S_Draggable _lastDragged;

    private S_Draggable _currentDragged;

    [SerializeField] private Transform mainMenu;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material dragMaterial;

    void Update()
    {
        // Drop on mouseUp or touch.end
        if (_isDragActive && (Input.GetMouseButtonDown(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)))
        {
            Drop();
            return;
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

        //_worldPosition = Camera.main.ScreenToWorldPoint(_screenPosition);

        if (_isDragActive)
        {
            Drag(); 
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(_screenPosition, Vector2.zero);
            if (hit.collider != null)
            {
                // Only works with collider
                // Need to create a copy of this object
                //S_Draggable draggable = hit.transform.gameObject.GetComponent<S_Draggable>();
                S_Draggable draggable = Instantiate(hit.transform.gameObject.GetComponent<S_Draggable>(), mainMenu);
                if (draggable != null)
                {
                    _currentDragged = draggable;
                    _lastDragged = hit.transform.gameObject.GetComponent<S_Draggable>();
                    initDrag();
                }
            }
        }
    }

    void initDrag()
    {
        _isDragActive = true;
        _lastDragged.GetComponent<Image>().material = dragMaterial;
    }

    void Drag()
    {
        // Position change
        _currentDragged.transform.position = new Vector2(_screenPosition.x, _screenPosition.y);
    }
    void Drop()
    {
        _isDragActive = false;
        _lastDragged.GetComponent<Image>().material = normalMaterial;
    }

}
