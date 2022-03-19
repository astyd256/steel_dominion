using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference dbReference;
    private int xp = 0;
    [SerializeField]
    private TMP_InputField emailField;
    [SerializeField]
    private TMP_InputField passwordField;
    [SerializeField]
    private TMP_Text logField;
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
            logField.text = $"Could not resolve all Firebase depedencies: {dependencyResult}";
        }
    }
    private void InitializeFirebase() // TODO: Really should consider moving AutoLogin couroutine from InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

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
            logField.text = $"Successfuy Autologin as {user.UserId}";
 
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
        StartCoroutine(RegisterLogic(emailField.text, passwordField.text));
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
            logField.text = output;
            yield break;
        }
        else
        {
            logField.text = $"Signed in as {user.UserId}"; //TODO: add base level
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
    public void Login()
    {
        StartCoroutine(LoginLogic(emailField.text, passwordField.text));
    } 
    private IEnumerator LoginLogic(string _email, string _password)
    {

        var loginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);

        yield return new  WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            logField.text = authError.ToString();
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
            logField.text = authError + '\n' + output;
            yield break;
        }
        else
        {
            logField.text = $"Signed in as {user.UserId}";
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
    public void LoginAnonymous()
        {
            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    logField.text = "Sign as Guest canceled.";
                    return;
                }
                if (task.IsFaulted) {
                    logField.text = "Sign as Guest encounterd an error: " + task.Exception;
                    return;
                }

                user = task.Result;
                logField.text = $"Loged as {user.UserId}"; //TODO: Clear Debug
                LoginInterfaceManager.instance.toMainMenu();
            });
        } 
    private void AuthStateChanged(object sender, System.EventArgs e)
    {
        bool signedIn = user == auth.CurrentUser && auth.CurrentUser != null;
        if (!signedIn && user != null)
        {
            //toLobby();
            logField.text = "Signed Out";
            user = null;
        }

        user = auth.CurrentUser;
        
        if (signedIn) //TODO: Add display name to users
        {
            if (user.DisplayName == null)
            logField.text = $"Signed in as {user.UserId}";
        }
    }
    public void LogOut() 
    {
        if (auth.CurrentUser != null) {
            auth.SignOut();
            xp = 0;
        }
        else logField.text = "User not logged";
        LoginInterfaceManager.instance.toLobby();
    }
    public void updateUserXp()
	{   
		StartCoroutine(retriveUserData());
	} 
	private IEnumerator retriveUserData() //TODO: Rewrite this code maybe sometime
	{
		var retriveDataTask = dbReference.Child(FirebaseManager.instance.user.UserId).GetValueAsync();

		yield return new  WaitUntil(predicate: () => retriveDataTask.IsCompleted);
		if (retriveDataTask.Exception != null)
		{
			Debug.LogWarning(message: $"Failed to register task with {retriveDataTask.Exception}");
		}
		else if (retriveDataTask.Result == null)
		{
			//No data exists yet
			Debug.LogWarning(message: $"No data found with given critteria");  
		}
		else
		{
			//Data has been retrieved
			xp = Convert.ToInt32(retriveDataTask.Result.Child("xp").Value.ToString());
        }
    }
    public void ChangeUsername (string newUsername) //TODO: Add something when change Username is failed
    {
        user = auth.CurrentUser;
        if (user != null) {
        Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile {
            DisplayName = newUsername
        };
        user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
            Debug.LogError("UpdateUserProfileAsync was canceled.");
            return;
            }
            if (task.IsFaulted) {
            Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
            return;
            }
            Debug.Log("User profile updated successfully.");
        });
        }
    }
    public int GetUserXp(){
        if (user == null) logField.text = "Error! User is null!";
        return xp;
    }
    public string GetUserName(){
        if (user == null) 
        {
            logField.text = "Error fetching name! User is null!";
            return "";    
        }
        else return user.DisplayName;
    }
}