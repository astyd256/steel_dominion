//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class S_CameraResolution : MonoBehaviour
{
    public int x = 585, y = 270;
    public Camera cam;
    // Start is called before the first frame update
    void Awake()
    {
        Screen.SetResolution(x, y, false);
        cam.aspect = 16f / 9f;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
    }
}
