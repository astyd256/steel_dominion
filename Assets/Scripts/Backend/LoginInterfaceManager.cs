using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginInterfaceManager : MonoBehaviour
{
    [SerializeField]
    private GameObject Lobby;
    [SerializeField]
    private GameObject LoginMenu;
    [SerializeField]
    private Button RegisterButton;
    [SerializeField]
    private Button LoginButton;
    [SerializeField]
    private GameObject Menu;
    [SerializeField]
    private TMP_InputField Email;
    [SerializeField]
    private TMP_InputField Password;
    [SerializeField]
    private TMP_Text usernamePanel;


    public static LoginInterfaceManager instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

    }
    public void toLobby()
    {
        Lobby.SetActive(true);
        LoginMenu.SetActive(false);
        Menu.SetActive(false);
    }

    public void toMainMenu()
    {
        Lobby.SetActive(false);
        LoginMenu.SetActive(false);
        Menu.SetActive(true);
        DBManager.instance.updateUserLevel();
    }
    public void toRegisterMenu()
    {
        Lobby.SetActive(false);
        LoginMenu.SetActive(true);
        LoginButton.gameObject.SetActive(false);
    }
    public void toLoginMenu()
    {
        Lobby.SetActive(false);
        LoginMenu.SetActive(true);
        RegisterButton.gameObject.SetActive(false);
    }
    public void VerifyInputs() //TODO: PROBABLY rewrite this code to add more restrictions in the password.
    {
        RegisterButton.interactable = (Email.text.Length >= 8 && Password.text.Length >= 8);
    }

    public void updateUsernamePanel()
    {
        usernamePanel.text = $"Level {DBManager.instance.level}";
    } 

}
