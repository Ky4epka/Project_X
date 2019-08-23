using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Script : MonoBehaviour {

    public NotifyEvent<UI_Script> OnInitialize = new NotifyEvent<UI_Script>();

    public GUI GUI = null;

    protected bool fInitialized = false;

    public bool Initialized
    {
        get
        {
            return fInitialized;
        }
    }

    public void Initialize()
    {
        if (fInitialized)
            return;

        GUI.Initialize();
        fInitialized = true;
        OnInitialize.Invoke(this);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
