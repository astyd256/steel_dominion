using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_GameAssets : MonoBehaviour
{
    private static S_GameAssets _i;

    public static S_GameAssets i
    {
        get
        {
            if (_i == null) _i = Instantiate(Resources.Load<S_GameAssets>("GameAssets"));
            return _i;
        }
    }

    public Transform pfDamagePopup;

#if UNITY_SERVER
    public void Start()
    {
        Destroy(this.gameObject);
    }
#endif
}
