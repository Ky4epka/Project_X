using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameController_GameLoopState
{
    None,
    MainMenu,
    PauseMenu,
    Game,
    AfterGameLeaderboard
}

public class GameController : MonoBehaviour {

    public UI_Script UI = null;

    protected GameController_GameLoopState fGameLoopState = GameController_GameLoopState.None;

    public GameController_GameLoopState GameLoopState
    {
        get
        {
            return fGameLoopState;
        }

        set
        {
            fGameLoopState = value;
        }
    }

    public void InitAllSystems()
    {
        UI.Initialize();
    }

    IEnumerator InitAll()
    {
        yield return new WaitForSeconds(0.2f);
        InitAllSystems();
    }

	// Use this for initialization
	void Start () {
        StartCoroutine(InitAll());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
