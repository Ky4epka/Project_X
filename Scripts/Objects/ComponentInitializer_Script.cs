using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ComponentInitializer_Script : MonoBehaviour {

        public UnityEvent Initializers = new UnityEvent();

        private bool fInit = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (fInit)
        {
            fInit = false;
            Initializers.Invoke();
        }
	}
}
