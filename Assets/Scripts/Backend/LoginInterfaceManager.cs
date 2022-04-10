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

    public string getUserName()
    {
        return userNameTMP.text;
    }
    public void setPlayerProfileValues()
    {
        userNameTMP.text = $"{FirebaseManager.instance.GetUserName()}";
        userLevelTMP.text = Mathf.Floor(Mathf.Sqrt(FirebaseManager.instance.GetUserXp() / 20) + 1).ToString();
        userXPBar.value = (Mathf.Sqrt(FirebaseManager.instance.GetUserXp() / 20) + 1 - (Mathf.Floor(Mathf.Sqrt(FirebaseManager.instance.GetUserXp() / 20)) + 1));
    }
    public void toLobby()
    {
        Lobby.SetActive(true);
        LoginMenu.SetActive(false);
        Menu.SetActive(false);
    }

    public void toMainMenu()
    {
        setPlayerProfileValues();
        Lobby.SetActive(false);
        LoginMenu.SetActive(false);
        Menu.SetActive(true);
        
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
        RegisterButton.interactable = (emailField.text.Length >= 8 && passwordField.text.Length >= 8);
    }
    public void Register()
    {
        FirebaseManager.instance.Registeration(emailField.text, passwordField.text);
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
