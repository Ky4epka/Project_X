using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject_Script : MonoBehaviour {

    public Player_Script Owner = null;

    protected Transform fBody = null;
    protected GameObject fUnit = null;
    protected ObjectType_Script fParentType = null;

    IEnumerator DoPostInit()
    {
        yield return null;

        Transform parent = fBody.parent;

        if (parent == null)
        {
            Debug.LogError("Could'not create test object (reason: Has no parent transform)");
        }
        else
        {
            fParentType = parent.GetComponent<ObjectType_Script>();

            if (fParentType == null)
            {
                Debug.LogError("Could'not create test object for  (reason: Has no parent object type)");
            }
            else
            {
                ObjectTypeInfo_Script ot = fParentType.CreateInstance(fBody.position, fBody.right, true);

                if (ot != null)
                {
                    UnitObject_Script unit = ot.GetComponent<UnitObject_Script>();

                    if (unit != null)
                    {
                        yield return null;
                        yield return null;
                        unit.Owner = Owner;
                    }
                }

                GameObject.Destroy(fUnit, 1);
            }
        }
    }

	// Use this for initialization
	void Start () {
        fBody = transform;
        fUnit = gameObject;
        StartCoroutine(DoPostInit());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
