//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class S_CameraResolution : MonoBehaviour
{
    public int x = 585, y = 270;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(x, y, true);
        cam.aspect = 19.5f / 9f;
        Application.targetFrameRate = 60;
    }
}
