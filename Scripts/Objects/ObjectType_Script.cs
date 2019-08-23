using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectType_Script : MonoBehaviour
{
    public TypeObjectManager_Script_TypeInfo Default_TypeInfo = new TypeObjectManager_Script_TypeInfo();

    public NotifyEvent_2P<ObjectType_Script, ObjectTypeInfo_Script> OnCreateInstance = new NotifyEvent_2P<ObjectType_Script, ObjectTypeInfo_Script>();
    public NotifyEvent_2P<ObjectType_Script, ObjectTypeInfo_Script> OnDestroyInstance = new NotifyEvent_2P<ObjectType_Script, ObjectTypeInfo_Script>();

    [SerializeField]
    private TypeObjectManager_Script_TypeInfo fTypeInfo = new TypeObjectManager_Script_TypeInfo();
    [SerializeField]
    private List<ObjectTypeInfo_Script> fInstances = new List<ObjectTypeInfo_Script>();
    [SerializeField]
    private int fInstanceHiId = 0;
    [SerializeField]
    private Transform fBody = null;


    public TypeObjectManager_Script_TypeInfo TypeInfo
    {
        get
        {
            return fTypeInfo;
        }

        set
        {
            fTypeInfo = value;
            name = fTypeInfo.TypeName;
        }
    }

    public ObjectTypeInfo_Script CreateInstance()
    {
        return CreateInstance(Vector3.zero, Vector3.right, false);
    }

    public ObjectTypeInfo_Script CreateInstance(Vector3 origin, bool active)
    {
        return CreateInstance(origin, Vector3.right, active);
    }

    public ObjectTypeInfo_Script CreateInstance(Vector3 origin, Vector3 direction, bool active)
    {
        Quaternion q = new Quaternion();
        q.eulerAngles = new Vector3(0, 0, Vector3.Angle(direction, Vector3.zero));
        GameObject _object = GameObject.Instantiate(fTypeInfo.Sample, origin, q, fBody);
        ObjectTypeInfo_Script instance = _object.GetComponent<ObjectTypeInfo_Script>();

        if (instance == null)
            instance = _object.AddComponent<ObjectTypeInfo_Script>();

        _object.SetActive(active);

        AddInstance(instance);
        return instance;
    }

    public void AddInstance(ObjectTypeInfo_Script instance)
    {
        if (!fInstances.Contains(instance))
        {
            fInstances.Add(instance);
            fInstanceHiId++;
            instance.ObjectType = this;
            OnCreateInstance.Invoke(this, instance);
        }
    }

    public bool DestroyInstance(ObjectTypeInfo_Script instance)
    {
        DestroyInstance(instance, Time.deltaTime);
        return fInstances.Remove(instance);
    }

    public bool DestroyInstance(ObjectTypeInfo_Script instance, float delay)
    {
        OnDestroyInstance.Invoke(this, instance);
        Destroy(instance.gameObject, delay);
        return fInstances.Remove(instance);
    }
    
    public void Clear()
    {
        while (fInstances.Count > 0)
            DestroyInstance(fInstances[0]);
    }

    public int InstanceHiId
    {
        get
        {
            return fInstanceHiId;
        }
    }


    public ObjectTypeInfo_Script InstanceAt(int index)
    {
        return fInstances[index];
    }

    public int InstanceCount
    {
        get
        {
            return fInstances.Count;
        }
    }


    private void Awake()
    {
        fBody = transform;
        TypeInfo = Default_TypeInfo;
        TypeObjectManager_Script.AddType(this);
    }

}
