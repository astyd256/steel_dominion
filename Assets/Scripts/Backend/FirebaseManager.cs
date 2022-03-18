using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference DBReference;
    //[Header("LoginEventHandler")]
    [SerializeField]
    private TMP_InputField Email;
    [SerializeField]
    private TMP_InputField Password;
    [SerializeField]
    private TMP_Text Log;
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
        StartCoroutine(CheckAndFixDependencies());
    }

    private IEnumerator CheckAndFixDependencies()
    {
        var checkAndFixDependenciesTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(predicate: () => checkAndFixDependenciesTask.IsCompleted);

        var dependencyResult = checkAndFixDependenciesTask.Result;

        if(dependencyResult == DependencyStatus.Available)
        {
            InitializeFirebase();
        }
        else
        {
            Log.text = $"Could not resolve all Firebase depedencies: {dependencyResult}";
        }
    }
    private void InitializeFirebase() // TODO: Really should consider moving AutoLogin couroutine from InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        DBReference = FirebaseDatabase.DefaultInstance.RootReference;

        StartCoroutine(CheckAutoLogin());

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }
    private IEnumerator CheckAutoLogin()
    {
        yield return new WaitForEndOfFrame();
        if(user != null)
        {
            var reloadUserTask = user.ReloadAsync();

            yield return new WaitUntil(predicate: () => reloadUserTask.IsCompleted);

            AutoLogin();
        }
        else
        {
            //TODO: Email Verification
            LoginInterfaceManager.instance.toLobby();
        }

    }
    private void AutoLogin()
    {
        if(user != null)
        {
            Log.text = $"Successfuy Autologin as {user.UserId}";
            //TODO: Email Verification
            LoginInterfaceManager.instance.toMainMenu();
        }
        else
        {
            LoginInterfaceManager.instance.toLobby();
        }
    }
    public void Registeration()
    {   
        StartCoroutine(RegisterLogic(Email.text, Password.text));
    }
    public void LoginAnonymous() //TODO: Rewrite code
    {
        //succeeded = true;
        auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled) {
                //succeeded = false;
                Log.text = "SignInAnonymously canceled.";
                return;
            }
            if (task.IsFaulted) {
                //succeeded = false;
                Log.text = "SignInAnonymously encounterd an error: " + task.Exception;
                return;
            }

            user = task.Result;
            LoginInterfaceManager.instance.toMainMenu();
            return;        
        });

        //if (succeeded) LoginInterfaceManager.instance.toMainMenu(); //TODO: Add some interface for user to know that the action failed
    }    
    public void Login()
    {
        StartCoroutine(LoginLogic(Email.text, Password.text));
    } 
    private void AuthStateChanged(object sender, System.EventArgs e)
    {
        bool signedIn = user == auth.CurrentUser && auth.CurrentUser != null;
        if (!signedIn && user != null)
        {
            //toLobby();
            Log.text = "Signed Out";
            user = null;
        }

        user = auth.CurrentUser;
        
        if (signedIn) //TODO: Add display name to users
        {
            if (user.DisplayName == null)
            Log.text = $"Signed in as {user.UserId}";
        }
    }
    private IEnumerator RegisterLogic(string _email, string _password)
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
            Log.text = output;
            yield break;
        }
        else
        {
            Log.text = $"Signed in as {user.UserId}";
            if (user.IsEmailVerified) // TODO: Add email verification
            {
                LoginInterfaceManager.instance.toMainMenu();
            }
            else
            {
                LoginInterfaceManager.instance.toMainMenu();
            } 
            //TODO: Send verification Email
        }

    }
    private IEnumerator LoginLogic(string _email, string _password)
    {

        var loginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);

        yield return new  WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            Log.text = authError.ToString();
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
                case AuthError.Failure:
                    output = "No internet connection";
                    break;
            }
            Log.text = authError + '\n' + output;
            yield break;
        }
        else
        {
            Log.text = $"Signed in as {user.UserId}";
            if (user.IsEmailVerified) // TODO: Add email verification
            {
                LoginInterfaceManager.instance.toMainMenu();
            }
            else
            {
                LoginInterfaceManager.instance.toMainMenu();
            }
        }   
    }
    public void LogOut() 
    {
        if (auth.CurrentUser != null) {
            auth.SignOut();
        }
        else Log.text = "User not logged";
        LoginInterfaceManager.instance.toLobby();
    }
}