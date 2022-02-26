using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginEventHandler : MonoBehaviour
{
    public static LoginEventHandler instance {get;set;}
    void Awake()
    {
        if (instance == null) {
            instance = this;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
