using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if !UNITY_SERVER 
public class LoginInterfaceManager : MonoBehaviour
{
    [SerializeField] private GameObject Lobby;
    [SerializeField] private GameObject LoginMenu;
    [SerializeField] private Button RegisterButton;
    [SerializeField] private Button LoginButton;
    [SerializeField] private GameObject Menu;
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_Text userLevelTMP;
    [SerializeField] private TMP_Text userNameTMP;
    [SerializeField] private Slider userXPBar;
    [SerializeField] private GameObject MainMenuManager;


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
    public void setPlayerProfileValues()
    {
        userNameTMP.text = $"{FirebaseManager.instance.GetUserName()}";
        userLevelTMP.text = Mathf.Floor(Mathf.Sqrt(FirebaseManager.instance.GetUserXp() / 20) + 1).ToString();
        userXPBar.value = (Mathf.Sqrt(FirebaseManager.instance.GetUserXp() / 20) + 1 - (Mathf.Floor(Mathf.Sqrt(FirebaseManager.instance.GetUserXp() / 20)) + 1));

        MainMenuManager.GetComponent<S_ProfileSettingsManager>().profileNameSet();
    }
    public void toLobby()
    {
        Lobby.SetActive(true);
        LoginMenu.SetActive(false);
        Menu.SetActive(false);
        MainMenuManager.SetActive(false);
    }

    public void toMainMenu()
    {
        //Set name and XP to profile in menu and settings
        setPlayerProfileValues();

        MainMenuManager.SetActive(true);

        Lobby.SetActive(false);
        LoginMenu.SetActive(false);
        Menu.SetActive(true);
        
    }
    public void toRegisterMenu()
    {
        Lobby.SetActive(false);
        LoginMenu.SetActive(true);
        LoginButton.gameObject.SetActive(false);
        RegisterButton.gameObject.SetActive(true);
    }
    public void toLoginMenu()
    {
        Lobby.SetActive(false);
        LoginMenu.SetActive(true);
        LoginButton.gameObject.SetActive(true);
        RegisterButton.gameObject.SetActive(false);
    }
    public void VerifyInputs() //TODO: PROBABLY rewrite this code to add more restrictions in the password.
    {
        RegisterButton.interactable = (emailField.text.Length >= 8 && passwordField.text.Length >= 8);
    }
    public void Register()
    {
        FirebaseManager.instance.Registration(emailField.text, passwordField.text);
    }
    public void Login()
    {
        FirebaseManager.instance.Login(emailField.text, passwordField.text);
    }
    public void LoginAnonymous()
    {
        FirebaseManager.instance.LoginAnonymous();
    }
}
#endif
