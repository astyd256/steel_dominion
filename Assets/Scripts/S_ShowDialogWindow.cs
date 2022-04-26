using UnityEngine;
using TMPro;

public class S_ShowDialogWindow : MonoBehaviour
{
    [SerializeField] public string message;
    [SerializeField] public TextMeshProUGUI messageTMP;
    public void ShowDialog(string msg)
    {
        message = msg;
        messageTMP.text = msg;
        this.gameObject.SetActive(true);
    }

    public void CloseDialog()
    {
        this .gameObject.SetActive(false);
    }
}
