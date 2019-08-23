using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectAttaching_Script_AttachStruct
{
    public Transform Body = null;
    public string key = "";
    public ObjectAttaching_Script_AttachType attach_type = 0;
    public Vector3 offset = Vector3.zero;
    public Transform PrevParent = null;
}

public enum ObjectAttaching_Script_AttachType
{
    AbsoluteVector,
    Parent
}

public class ObjectAttaching_Script : MonoBehaviour {

    public const int ATTACH_TYPE_ABSOLUTE_VECTOR = 0;
    public const int ATTACH_TYPE_RELATIVE_VECTOR = 1;

    public Transform Direct_Pivot_Body = null;

    [SerializeField]
    private List<ObjectAttaching_Script_AttachStruct> fAttaches = new List<ObjectAttaching_Script_AttachStruct>();

    public bool AddAttach(Transform body, string key, ObjectAttaching_Script_AttachType attach_type, Vector3 offset)
    {
        int index = GetAttachIndexByKey(key);

        if (index != -1)
            return false;

        ObjectAttaching_Script_AttachStruct rec = new ObjectAttaching_Script_AttachStruct();
        rec.Body = body;
        rec.key = key;
        rec.attach_type = attach_type;
        rec.offset = offset;
        fAttaches.Add(rec);

        switch (attach_type)
        {
            case ObjectAttaching_Script_AttachType.Parent:
                rec.PrevParent = body.parent;
                body.parent = Direct_Pivot_Body;
                body.localPosition = offset;
                break;
        }

        return true;
    }

    public bool DeleteAttachByIndex(int index)
    {
        if ((index < 0) || (index >= fAttaches.Count))
            return false;

        ObjectAttaching_Script_AttachStruct rec = fAttaches[index];

        switch (rec.attach_type)
        {
            case ObjectAttaching_Script_AttachType.Parent:
                rec.Body.parent = rec.PrevParent;
                break;
        }

        fAttaches.RemoveAt(index);
        return true;
    }

    public bool DeleteAttachByKey(string key)
    {
        return DeleteAttachByIndex(GetAttachIndexByKey(key));
    }

    public bool DeleteAttachByBody(Transform body)
    {
        return DeleteAttachByIndex(GetAttachIndexByBody(body));
    }

    public void ChangeAttachOffset(string key, Vector3 offset)
    {
        int index = GetAttachIndexByKey(key);
        fAttaches[index].offset = offset;
    }

    public int GetAttachIndexByKey(string key)
    {
        for (int i = 0; i < fAttaches.Count; i++)
            if (fAttaches[i].key == key)
                return i;

        return -1;
    }

    public int GetAttachIndexByBody(Transform body)
    {
        for (int i = 0; i < fAttaches.Count; i++)
            if (fAttaches[i].Body == body)
                return i;

        return -1;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < fAttaches.Count; i++)
        {
            switch (fAttaches[i].attach_type)
            {
                case ObjectAttaching_Script_AttachType.AbsoluteVector:
                    fAttaches[i].Body.position = Direct_Pivot_Body.position + fAttaches[i].offset;
                    break;
            }
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnValidate()
    {
        if (Direct_Pivot_Body == null)
            Direct_Pivot_Body = this.GetComponent<Transform>();
    }
}
