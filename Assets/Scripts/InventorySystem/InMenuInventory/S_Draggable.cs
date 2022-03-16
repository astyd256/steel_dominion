using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_Draggable : MonoBehaviour
{
    [SerializeField] private string type;

    private Collider2D _collider;

    private float _movementTime = 15f;

    public string GetDraggableType()
    {
        return type;
    }

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Enter");
        if (other.CompareTag("ValidDrop") && type == "InventoryUnitSlot")
        {
            this.GetComponent<Image>().color = Color.black;
        }
    }

}
