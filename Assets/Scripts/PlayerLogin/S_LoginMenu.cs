using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;

public class S_LoginMenu : MonoBehaviour
{
    private bool succeeded;
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
        RegisterButton.interactable = (Email.text.Length >= 8 && Password.text.Length >= 8);
    } 


    public void Registeration()
    {   
        succeeded = true;
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(Email.text, Password.text).ContinueWith(task => {
            if (task.IsCanceled) {
                succeeded = false;
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                succeeded = false;
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });

        if (succeeded){
            LoginMenu.SetActive(false);
            Menu.SetActive(true);
        }
    
    }

    public void LoginAnonymous() 
    {
        succeeded = true;
        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled) {
                succeeded = false;
                Debug.LogError("SignInAnonymously was canceled.");
                return;
            }
            if (task.IsFaulted) {
                succeeded = false;
                Debug.LogError("SignInAnonymously encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            return;        
        });

        if (succeeded){
            LoginMenu.SetActive(false);
            Menu.SetActive(true);
        }
    }    
    public void Login()
    {
        succeeded = true;
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(Email.text, Password.text).ContinueWith(task => {
            if (task.IsCanceled) {
                succeeded = false;
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                succeeded = false;
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            return;
        });

        if (succeeded){
            LoginMenu.SetActive(false);
            Menu.SetActive(true);
        }
            

    }

    public void LogOut() 
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null) {
            FirebaseAuth.DefaultInstance.SignOut();
            Lobby.SetActive(true);
            Menu.SetActive(false);
        }
    }

}