using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SimpleHover : MonoBehaviour
{
    float oirginalY;

    public float floatStrength = 1f;
    // Start is called before the first frame update
    void Start()
    {
        this.oirginalY = this.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, oirginalY + ((float)Mathf.Sin(Time.time) * floatStrength), transform.position.z);
    }
}
