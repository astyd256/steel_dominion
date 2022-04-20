using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;
    private DatabaseReference dbReference;
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
    
    #if UNITY_SERVER
    
    void AddExp(string winnerPlayerToken, string loserPlayerToken)
    {
        var winnerExp = await dbReference.Child(winnerPlayerToken).Child("_xp").GetValueAsync();
        await dbReference.Child(winnerPlayerToken).Child("_xp").SetValueAsync(winnerExp + 100);
        var loserExp = await dbReference.Child(loserPlayerToken).Child("_xp").GetValueAsync();
        await dbReference.Child(loserPlayerToken).Child("_xp").SetValueAsync(loserExp + 50);
    }

    string GetCurInventory(string PlayerToken)
    {
        return await dbReference.Child(PlayerToken).Child("_cur_inventory
        ").GetValueAsync();
    }
    
    #endif
    
    #if !UNITY_SERVER 
    private FirebaseAuth auth;
    private FirebaseUser user;
    private int _xp = 0;
    private string _inventory = "";
    private string _cur_inventory= "";
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
            await user.ReloadAsync();
            await retriveUserData();
            AutoLogin();
        }
    }
    private void AutoLogin()
    {
        if(user != null)
        {
            LoginInterfaceManager.instance.toMainMenu();
        }
        else
        {
            LoginInterfaceManager.instance.toLobby();
        }
    }
    public async void Registration(string email, string password)
    {
        await RegisterUser(email, password);
    }   
    private async Task RegisterUser(string _email, string _password)
    {
        var registerTask =  auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
        await registerTask;

        if(registerTask.Exception != null)
        {
            await user.DeleteAsync();
            //TODO: Add error message
        }
        else
        {
            AddDefaultAccountIntoDatabase();
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
    private async void AddDefaultAccountIntoDatabase()
    {
        await dbReference.Child(user.UserId).Child("xp").SetValueAsync(0);
        await dbReference.Child(user.UserId).Child("inventory").SetValueAsync("000100000000020201010100");
        await dbReference.Child(user.UserId).Child("cur_inventory").SetValueAsync("");
    }
    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => 
        {
            if (task.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)task.Exception.GetBaseException();
                //TODO: Add error message
            }
            else LoginInterfaceManager.instance.toMainMenu();   
        });
    }
    public void LoginAnonymous()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(async task => {
            if (task.IsCanceled) {
                //TODO: Add error message here
                return;
            }
            if (task.IsFaulted) {
                //TODO: Add error message here
                return;
            }
            user = task.Result;
            AddDefaultAccountIntoDatabase();
            await ChangeUsername("Guest");
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
            user = null;
        }
        user = auth.CurrentUser;
    }
    public void LogOut() 
    {
        if (auth.CurrentUser != null) {
            auth.SignOut();
            _xp = 0;
            _inventory = "";
            _cur_inventory
             = "";
        }
    }
	private async Task retriveUserData()
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
                _xp = Convert.ToInt32(task.Child("_xp").Value);
                _inventory = task.Child("_inventory").ToString();
                _cur_inventory = task.Child("cur_inventory").ToString();
            }

        }
        else Debug.LogWarning(message: "Can't retrieve data from Firebase. User does not exist");
    }
    public async Task ChangeUsername (string newUsername)
    {
        user = auth.CurrentUser;
        if (user != null) {
        Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile {
            DisplayName = newUsername
        };
        await user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task => {
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
    public int GetUserXp()
    {
        if (user == null)
        {
            return 0;
        }
        return _xp;
    }
    public string GetUserName()
    {
        if (user == null) return "";
        else return user.DisplayName;
    }
    public async void SaveCurInventory(string inventory)
    {
        await dbReference.Child(user.UserId).Child("cur_inventory").SetValueAsync(inventory);
    }
    public string GetCurInventory()
    {
        return _cur_inventory;
    }
    public string GetInventory()
    {
        return _inventory;
    }
    public void ChangeEmail(string newEmail)
    {
        user = auth.CurrentUser;
        if (user != null) {
            user.UpdateEmailAsync(newEmail).ContinueWith(task => {
                if (task.IsCanceled) {
                Debug.LogError("UpdateEmailAsync was canceled.");
                return;
                }
                if (task.IsFaulted) {
                Debug.LogError("UpdateEmailAsync encountered an error: " + task.Exception);
                return;
                }

                Debug.Log("User email updated successfully.");
            });
        }

    }    
    #endif
}