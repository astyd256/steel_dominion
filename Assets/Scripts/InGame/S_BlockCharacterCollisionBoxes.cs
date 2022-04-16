using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_BlockCharacterCollisionBoxes : MonoBehaviour
{
    [SerializeField]
    public List<BoxCollider> characterColliders;
    [SerializeField]
    public List<BoxCollider> characterBlockerColliders;
    // Start is called before the first frame update
    void Start()
    {
        foreach (BoxCollider collider in characterColliders)
        {
            foreach(BoxCollider blockcollider in characterBlockerColliders)
            {
                Physics.IgnoreCollision(collider, blockcollider, true);
            }
        }
    }
}
