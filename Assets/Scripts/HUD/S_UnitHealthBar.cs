using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_UnitHealthBar : MonoBehaviour
{
#if !UNITY_SERVER
    Camera playerCamera;
#endif
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_SERVER
            this.enabled = false;
#endif

#if !UNITY_SERVER
        playerCamera = Camera.main;
#endif
    }
#if !UNITY_SERVER
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position+playerCamera.transform.rotation * Vector3.back, playerCamera.transform.rotation*Vector3.down);
    }
#endif
}
