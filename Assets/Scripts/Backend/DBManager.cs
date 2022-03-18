using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using static Firebase.Extensions.TaskExtension;

public class DBManager: MonoBehaviour
{
	public string level = "0";
	public static DBManager instance;
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
	public void updateUserLevel()
	{   
		StartCoroutine(retriveUserData("level"));
	} 
	private IEnumerator retriveUserData(string _criteria) //TODO: Rewrite this code maybe sometime
	{
		var retriveDataTask = FirebaseManager.instance.DBReference.Child(FirebaseManager.instance.user.UserId).GetValueAsync();

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
			level = retriveDataTask.Result.Child(_criteria).Value.ToString();
			LoginInterfaceManager.instance.updateUsernamePanel();
        }
    }   
    
}
