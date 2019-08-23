using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ObjectAliasing_Script_NotifyEvent: UnityEvent<GameObject>
{
}

[RequireComponent(typeof(ObjectAttaching_Script))]
public class ObjectAliasing_Script : MonoBehaviour {
    public Transform ObjectParent = null;
    public GameObject ObjectSample = null;
    public string AttachKey = "key";
    public ObjectAttaching_Script_AttachType AttachType = ObjectAttaching_Script_AttachType.AbsoluteVector;
    public Vector3 AttachOffset = Vector3.zero;

    public ObjectAliasing_Script_NotifyEvent ObjectRecievers = new ObjectAliasing_Script_NotifyEvent();

    private GameObject fObject = null;
    private bool fInit = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (fInit)
        {
            ObjectAttaching_Script pivot = GetComponent<ObjectAttaching_Script>();                    

            fObject = GameObject.Instantiate(ObjectSample, ObjectParent);
            fObject.SetActive(true);
            pivot.AddAttach(fObject.transform, AttachKey, AttachType, AttachOffset);

            ObjectRecievers.Invoke(fObject);

            fInit = false;
        }
	}
}
