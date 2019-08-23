using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI : MonoBehaviour {

    public CommonStats CommonStats = null;
    public GUI_SelectedUnit SelectedUnit = null;
    public GUI_Radar Radar = null;

    protected Player_Script fBindedPlayer = null;


    public Player_Script BindedPlayer
    {
        get
        {
            return fBindedPlayer;
        }

        set
        {
            fBindedPlayer = value;
            CommonStats.BindedPlayer = value;
            SelectedUnit.BindedPlayer = value;
        }
    }

    public void Initialize()
    {
        Radar.Initialize();
        BindedPlayer = GlobalCollector.Instance.LocalPlayer;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
