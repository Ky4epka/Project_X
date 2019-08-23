using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAutodestroyer_Script : MonoBehaviour {

    public float Lifetime = 1f;

    public bool DestroyAfterStart = true;

    private bool fInit = true;

    public void DoDestroy()
    {
        GameObject.Destroy(this.gameObject, Lifetime);
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (fInit)
        {
            fInit = false;

            if (DestroyAfterStart)
                DoDestroy();
        }
	}
}
