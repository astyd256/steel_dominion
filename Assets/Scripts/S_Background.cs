using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class S_Background : MonoBehaviour
{

    [SerializeField] public List<Sprite> backgrounds;



    void Start()
    {
        
    }
    public void SetInvisible()
    {
        this.gameObject.SetActive(false);
    }

    public void SetVisible()
    {
        this.gameObject.SetActive(true);
    }

    public void SetImage(int index)
    {
        if (index >= 0 && index < backgrounds.Count)
        this.gameObject.GetComponent<SpriteRenderer>().sprite = backgrounds[index];
    }

}
