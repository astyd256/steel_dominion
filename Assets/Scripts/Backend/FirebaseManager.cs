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
    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }
#endif
    // #if UNITY_SERVER   
    public async Task AddExp(string playerToken, int xp)
    {
        var task = await dbReference.Child(playerToken).Child("xp").GetValueAsync();
        //TODO: Add error message
        int curExp = Convert.ToInt32(task.Value);
        await dbReference.Child(playerToken).Child("xp").SetValueAsync(curExp + xp);
    }
    public async Task<string> GetCurInventoryServer(string playerToken)
    {
        var task = await dbReference.Child(playerToken).Child("cur_inventory").GetValueAsync();
        //TODO: Add error message
        return task.Value.ToString(); 
    }
    // #endif
    
    #if !UNITY_SERVER 
    private FirebaseAuth _auth;
    private FirebaseUser _user;
    private int _xp = 0;
    private string _inventory = "";
    private string _cur_inventory = "";
    private int _picture_id = 0;
    private bool _registration = false; // true if user is in registration state
    void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        _auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        AsyncStart();
    }
    async void AsyncStart()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();
        if (_user != null) 
        {
            await _user.ReloadAsync();
            await retriveUserData();
            if (!_registration) AutoLogin();
        }
    }
    private void AutoLogin()
    {
        if (_user != null) LoginInterfaceManager.instance.toMainMenu();
        else LoginInterfaceManager.instance.toLobby();
    }
    public async Task RegistrationFromGuest(string email, string password)
    {
        /*** HAVE TO USE VERIFY EMAIL FIRST BEFORE TRIGGERING THIS METHOD ***/
        await _user.ReloadAsync();
        if (_user != null && _user.Email == email && _user.IsEmailVerified)
        {
            await _user.UpdatePasswordAsync(password); //TODO: Add error message
            _registration = false;
            LoginInterfaceManager.instance.toMainMenu();
        }
    }   
    private async Task AddDefaultAccountIntoDatabase()
    {
        await dbReference.Child(_user.UserId).Child("xp").SetValueAsync(0);
        await dbReference.Child(_user.UserId).Child("inventory").SetValueAsync("000100000000020201010100");
        await dbReference.Child(_user.UserId).Child("cur_inventory").SetValueAsync("");
        await dbReference.Child(_user.UserId).Child("picture_id").SetValueAsync(0);
    }
    public async Task Login(string email, string password)
    {
        if (_registration)
        {
            _registration = false;
            if (_user != null)
            {
                await DeleteCurrentUserFromDB();
                await DeleteCurrentUser();
            }
        }
        await _auth.SignInWithEmailAndPasswordAsync(email, password);
        await retriveUserData(); 
        LoginInterfaceManager.instance.toMainMenu();
    }
    public async Task LoginAnonymous()
    {
        if (_registration)
        {
            _registration = false;
            if (_user != null)
            {
                await DeleteCurrentUserFromDB();
                await DeleteCurrentUser();
            }
        }
        _user = await _auth.SignInAnonymouslyAsync();
        await AddDefaultAccountIntoDatabase();
        await ChangeUsername("Guest");
    } 
    private void AuthStateChanged(object sender, System.EventArgs e)
    {
        bool signedIn = _user == _auth.CurrentUser && _auth.CurrentUser != null;
        if (!signedIn && _user != null)
        {
            LoginInterfaceManager.instance.toLobby();
            //TODO: Add error message
        }
        _user = _auth.CurrentUser;
    }
    public void LogOut() 
    {
        if (_auth.CurrentUser != null) {
            _auth.SignOut();
            _xp = 0;
            _inventory = "";
            _cur_inventory = "";
        }
    }
	private async Task retriveUserData()
	{
        if (_user != null)
        {            
            var task = await dbReference.Child(FirebaseManager.instance._user.UserId).GetValueAsync();
            if (task.ChildrenCount == 0)
            {
                Debug.LogWarning(message: $"No data was found on given _user");

            }
            else
            {
                //Data has been retrieved
                _xp = Convert.ToInt32(task.Child("xp").Value);
                _inventory = task.Child("inventory").Value.ToString();
                _cur_inventory = task.Child("cur_inventory").Value.ToString();
                _picture_id = Convert.ToInt32(task.Child("picture_id").Value);
            }
        }
        else Debug.LogWarning(message: "Can't retrieve data from Firebase. User does not exist");
    }
    public async Task ChangeUsername (string newUsername)
    {
        _user = _auth.CurrentUser;
        if (_user != null) {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile {
                DisplayName = newUsername
            };
            await _user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task => { // TODO: Add error message
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
        if (_user == null)
        {
            return 0;
        }
        return _xp;
    }
    public string GetUserName()
    {
        if (_user == null) return "";
        else return _user.DisplayName;
    }
    public async Task SaveCurInventory(string inventory)
    {
        await dbReference.Child(_user.UserId).Child("cur_inventory").SetValueAsync(inventory);
    }
#if !UNITY_SERVER
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
        //TODO: Add email verification
        _user = _auth.CurrentUser;
        if (_user != null) {
            _user.UpdateEmailAsync(newEmail).ContinueWith(task => {
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
    public async Task VerifyEmail(string email)
    {
        if (!_registration)
        {
            if (_user == null) // If user is null then user is trying to register
            {
                _user = await _auth.SignInAnonymouslyAsync();
                _registration = true;
            } 
            await _user.UpdateEmailAsync(email);
            await _user.SendEmailVerificationAsync();
            //TODO: Add info to user
        }
        else
        {
            if (!_user.IsEmailVerified || _user.Email != email) //TODO: Add timeout for sending verifying email
            {
                if (_user == null) _user = await _auth.SignInAnonymouslyAsync(); 
                await _user.UpdateEmailAsync(email);
                await _user.SendEmailVerificationAsync(); 
            }
            //TODO: Add info to user
        }             
    }    
    public string GetUserToken()
    {
        return _user.UserId;
    }
    public async Task ChangeProfilePicture(int pictureId)
    {
        //TODO: Add error messege here
        await dbReference.Child(_user.UserId).Child("picture_id").SetValueAsync(pictureId);
        _picture_id = pictureId;
    }      
    public async Task<int> GetProfilePictureId()
    {
        var task = await dbReference.Child(_user.UserId).Child("picture_id").GetValueAsync();
        //TODO: Add error message
        return Convert.ToInt32(task.Value);
           
    }
    public async Task DeleteCurrentUser()
    {
        if (_user != null) await _user.DeleteAsync(); //TODO: Add error message
    }
    public async Task DeleteCurrentUserFromDB()
    {
        await dbReference.Child(_user.UserId).RemoveValueAsync(); //TODO: Add error message
    }
    public bool IsEmailVerified()
    {
        return _user.IsEmailVerified;
    }
#endif
}