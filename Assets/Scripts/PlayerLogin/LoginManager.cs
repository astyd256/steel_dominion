using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;

public class LoginManager : MonoBehaviour
{
    public static LoginManager instance;

  [Header("LoginEventHandler")]
    public FirebaseAuth auth;
    public FirebaseUser user;
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

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null) 
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(checkDependencyTask => {
            var dependencyStatus = checkDependencyTask.Result;
            if (checkDependencyTask.Result == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies {dependencyStatus}");
            }
        });
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

        if (succeeded) toMainMenu(); //TODO: Add some interface for user to know that the action failed
    
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

        if (succeeded) toMainMenu(); //TODO: Add some interface for user to know that the action failed
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

        if (succeeded) toMainMenu(); //TODO: Add some interface for user to know that the action failed
            

    }
    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }
    private void AuthStateChanged(object sender, System.EventArgs e)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed Out");
            }
            if (signedIn)
            {
                Debug.Log($"Signed In: {user.DisplayName}");
            }
        }
    }
    private IEnumerator RegisterLogic(string _username, string _email, string _password)
    {
        if(_username == "" ) // TODO: Add more advanced checks for username or/and password
        {

        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if(registerTask.Exception != null)
            {
                user.DeleteAsync();
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string output = "Unknown error, please try again.";
                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "Email already in use";
                        break;
                    case AuthError.WeakPassword:
                        output = "Weak Password";
                        break;
                    case AuthError.MissingEmail:
                        output = "Please enter your Email";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please enter your password";
                        break;
                    case AuthError.SessionExpired:
                        output = "Session Expired";
                        break;
                }
                Debug.LogError(output); // TODO: Add error output to user.
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = _username,

                    //TODO: Add default photo to profile
                };

                var updateUserTask = user.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => updateUserTask.IsCompleted);
                //TODO: Send verification Email
            }
        }

    }
    private IEnumerator LoginLogic(string _email, string _password)
    {
        Credential credential = EmailAuthProvider.GetCredential(_email, _password);

        var loginTask = auth.SignInWithCredentialAsync(credential);

        yield return new  WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string output = "Unknown error, please try again.";
            switch (authError)
            {
                case AuthError.MissingEmail:
                    output = "Please enter your email";
                    break;
                case AuthError.MissingPassword:
                    output = "Please enter your password";
                    break;
                case AuthError.InvalidEmail:
                    output = "Please enter a valid email";
                    break;
                case AuthError.WrongPassword:
                    output = "Please enter your password";
                    break;
                case AuthError.UserNotFound:
                    output = "User not found";
                    break;
            }
            Debug.LogError(output); // TODO: Add error output to user.
        }
        else
        {
            if (user.IsEmailVerified) // TODO: Add email verification
            {
                LoginManager.instance.toMainMenu();
            }
            else
            {
                LoginManager.instance.toMainMenu();
            }
        }   
    }
    public void LogOut() 
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null) {
            FirebaseAuth.DefaultInstance.SignOut();
            toLobbyMenu();
        }
    }
    public void toLobbyMenu()
    {
        Lobby.SetActive(true);
        LoginMenu.SetActive(false);
    }
    public void toLoginMenu()
    {
        Lobby.SetActive(false);
        LoginMenu.SetActive(true);
        RegisterButton.gameObject.SetActive(false);
    }
    public void toRegisterMenu()
    {
        Lobby.SetActive(false);
        LoginMenu.SetActive(true);
        LoginButton.gameObject.SetActive(false);
    }
    public void toMainMenu()
    {
        Lobby.SetActive(true);
        LoginMenu.SetActive(false);
    }
}