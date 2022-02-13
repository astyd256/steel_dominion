using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_LoginMenu : MonoBehaviour
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
    private TMP_InputField Username;
    [SerializeField]
    private TMP_InputField Password;
    [SerializeField]
    private TMP_Text AuthUsername;


    public void OnLobbyLoginButton(){
        Lobby.SetActive(false);
        LoginMenu.SetActive(true);
        RegisterButton.gameObject.SetActive(false);
    } 

    public void OnLobbyRegisterButton(){
        Lobby.SetActive(false);
        LoginMenu.SetActive(true);
        LoginButton.gameObject.SetActive(false);
    }
    public void OnBackButton(){
        Lobby.SetActive(true);
        LoginMenu.SetActive(false);
    }

    public void VerifyInputs() //TODO: PROBABLY rewrite this code to add more restrictions in the password.
    {
        RegisterButton.interactable = (Username.text.Length >= 8 && Password.text.Length >= 8);
    } 

    public void OnLoginLoginButton(){
        StartCoroutine(Login());
    }

    public void OnLoginRegisterButton(){
        StartCoroutine(Registeration());
    }

    IEnumerator Registeration()
    {   
        WWWForm form = new WWWForm();
        form.AddField("username", Username.text);
        form.AddField("password", Password.text);
        WWW www = new WWW("http://localhost/sqlconnect/register.php", form); // TODO: Obsolete. Rewrite.
        yield return www;
        
        if (www.text == "0") 
        {
            Debug.Log("User created successfully."); // TODO: Rewrite something here probably, change interface in main menu when you log in. Clear debug!
            LoginMenu.SetActive(false);
            Menu.SetActive(true);
            AuthUsername.text = Username.text;
        }
        else
        {
            Debug.Log("Can't register error number #" + www.text); //TODO: Clear debug.
        }
    }

    IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", Username.text);
        form.AddField("password", Password.text);
        WWW www = new WWW("http://localhost/sqlconnect/login.php", form); // TODO: Obsolete. Rewrite.
        yield return www;

        if (www.text[0] == '0')
        {
            Debug.Log(www.text); //TODO: Clear debug
            DBManager.username = Username.text;
            DBManager.level = int.Parse(www.text.Split('\t')[1]);
            DBManager.exp = int.Parse(www.text.Split('\t')[2]);
            LoginMenu.SetActive(false);
            Menu.SetActive(true);
            AuthUsername.text = Username.text;
        }
        else
        {
            Debug.Log("User login failed. Error #" + www.text); //TODO: Clear debug
        }
    }

}