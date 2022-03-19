using System;
using System.Collections;
using System.Threading.Tasks;
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
    private FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference dbReference;
    #if !UNITY_SERVER 
    private int xp = 0;
    [Header("Temporary log")]
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
    }
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        AsyncStart();
    }
    async void AsyncStart()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();


        if (user != null) 
        {
            logField.text = $"first await";
            await user.ReloadAsync();
            logField.text = $"second await";
            await retriveUserData();
            logField.text = $"Autologin";
            AutoLogin();
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
    public void Registeration(string email, string password)
    {   
        StartCoroutine(RegisterLogic(email, password));
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
    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => 
        {
            if (task.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)task.Exception.GetBaseException();
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

            }
            else LoginInterfaceManager.instance.toMainMenu();   
        });
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
                Debug.Log(user);
                LoginInterfaceManager.instance.toMainMenu();
            });
        } 
    private void AuthStateChanged(object sender, System.EventArgs e)
    {
        bool signedIn = user == auth.CurrentUser && auth.CurrentUser != null;
        if (!signedIn && user != null)
        {
            LoginInterfaceManager.instance.toLobby();
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
    }
	private async Task<int> retriveUserData()
	{
        if (user != null)
        {            
            var task = await dbReference.Child(FirebaseManager.instance.user.UserId).GetValueAsync();
            if (task.ChildrenCount == 0)
            {
                Debug.LogWarning(message: $"No data was found on given user");

            }
            else
            {
                //Data has been retrieved
                xp = Convert.ToInt32(task.Child("xp").Value.ToString());
            }

        }
        else Debug.LogWarning(message: "Can't retrieve data from Firebase. User does not exist");
		return 0;
    }
    public void ChangeUsername (string newUsername)
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
    #endif
}