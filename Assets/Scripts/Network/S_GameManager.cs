using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_GameManager : MonoBehaviour
{
    public static S_GameManager singleton { get; private set; }

    [SerializeField] public S_ShowDialogWindow showDialogWindowPrefab;

    private enum MatchEndingState
    {
        Nothing,
        Draw,
        Win,
        Lose
    }

    private MatchEndingState _matchEndState = MatchEndingState.Nothing;

    void Awake()
    {
#if UNITY_SERVER
        Destroy(gameObject);
#endif

        DontDestroyOnLoad(gameObject);
        if (singleton == null)
        {
            singleton = this;
        }
        else if (singleton != this)
        {
            Destroy(singleton.gameObject);
            singleton = this;
        }
    }

    public void SetEndingPopup(int endID)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (endID == 0) _matchEndState = MatchEndingState.Draw;
        else if (endID == 1) _matchEndState = MatchEndingState.Lose;
        else if (endID == 2) _matchEndState = MatchEndingState.Win;
        else throw new Exception("Incorrect endID for popup choose!");
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        Debug.Log("Match ended, result is " + _matchEndState);
    }

    public void ShowDialog(string message)
    {
        S_ShowDialogWindow showDialogWindow = Instantiate(showDialogWindowPrefab); 
        showDialogWindow.ShowDialog(message);
    }
}
