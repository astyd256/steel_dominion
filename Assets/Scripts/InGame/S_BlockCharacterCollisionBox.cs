using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_BlockCharacterCollisionBox : MonoBehaviour
{
    [SerializeField]
    public BoxCollider characterCollider;
    [SerializeField]
    public BoxCollider characterBlockerCollider;
    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreCollision(characterCollider, characterBlockerCollider, true);
    }
}
