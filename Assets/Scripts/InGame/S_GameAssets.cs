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
    [Header("Attack effects")]

    public Transform pfDamagePopup;

    public Transform pfGreasleyHitPS;

    public Transform pfRogueSmallShotMuzzlePS;

    public Transform pfRogueSmallShotHitPS;

    public Transform pfRogueBigShotMuzzlePS;

    public Transform pfRogueBigShotHitPS;

    [Header("Destroy effects")]

    public Transform pfGreasleyDestroyPS;

    public Transform pfDroneDestroyPS;

    public Transform pfRogueDestroyPS;

#if UNITY_SERVER
    public void Start()
    {
        Destroy(this.gameObject);
    }
#endif
}
