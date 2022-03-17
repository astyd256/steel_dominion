using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Username : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(LoginManager.instance.user.DisplayName);
        if (LoginManager.instance.user.DisplayName != null && LoginManager.instance.user.DisplayName != "") this.GetComponent<TMP_Text>().text = "Logged as" + LoginManager.instance.user.DisplayName; 
        else this.GetComponent<TMP_Text>().text = "No nickname";
        
    }
}
