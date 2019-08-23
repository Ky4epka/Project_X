using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TypeObjectManager_Script_TypeData
{
    public Sprite TypeIcon = null;
    public float Base_Cost = 100;
    public float Base_BuildTime = 1;
    public float Base_Endurance = 100;
    public Vector2Int Base_Size = Vector2Int.one;

    public TypeObjectManager_Script_TypeData()
    {
    }

    public TypeObjectManager_Script_TypeData(Sprite type_icon, float base_cost, float base_build_time)
    {
        TypeIcon = type_icon;
        Base_Cost = base_cost;
        Base_BuildTime = base_build_time;
    }

    public void Assign(TypeObjectManager_Script_TypeData source)
    {
        TypeIcon = source.TypeIcon;
        Base_Cost = source.Base_Cost;
        Base_BuildTime = source.Base_BuildTime;
    }
}

[System.Serializable]
public class TypeObjectManager_Script_TypeInfo
{
    public string TypeName = "unknown_type";
    public GameObject Sample = null;
    public TypeObjectManager_Script_TypeData TypeData = null;

    public TypeObjectManager_Script_TypeInfo()
    {

    }

    public TypeObjectManager_Script_TypeInfo(string type_name, GameObject sample, TypeObjectManager_Script_TypeData type_data)
    {
        TypeName = type_name;
        Sample = sample;
        TypeData = type_data;
    }       

    public void Assign(TypeObjectManager_Script_TypeInfo source)
    {
        TypeName = source.TypeName;
        Sample = source.Sample;
        TypeData.Assign(source.TypeData);
    }

}

public class TypeObjectManager_Script : MonoBehaviour {
    
    protected static List<ObjectType_Script> fTypes = new List<ObjectType_Script>();


    public static ObjectTypeInfo_Script CreateObjectInstance(string type_name, Vector3 origin, Vector3 direction, bool active)
    {
        ObjectType_Script type = TypeInstance(type_name);

        if (type == null)
        {
            Debug.LogError("TypeObject_Manager_Script.CreateObjectInstance: Couldnot create object instance! Reason: object type (" + type_name + ") not found");
            return null;
        }

        return type.CreateInstance(origin, direction, active);
    }

    public static bool DestroyObjectInstance(ObjectTypeInfo_Script instance, float delay)
    {
        if (instance.ObjectType == null)
        {
            Debug.LogError("TypeObject_Manager_Script.DestroyObjectInstance: Couldnot remove object instance! Reason: object type is null");
            return false;
        }

        return instance.ObjectType.DestroyInstance(instance, delay);
    }

    public static bool DestroyObjectInstance(GameObject instance, float delay)
    {
        return DestroyObjectInstance(instance.GetComponent<ObjectTypeInfo_Script>(), delay);
    }

    public static void DestroyObjectInstance(ObjectTypeInfo_Script instance)
    {
        instance.ObjectType.DestroyInstance(instance, 0f);
    }

    public static void DestroyObjectInstance(GameObject instance)
    {
        DestroyObjectInstance(instance.GetComponent<ObjectTypeInfo_Script>(), 0f);
    }

    public static void AddType(ObjectType_Script type)
    {
        fTypes.Add(type);
    }

    public static ObjectType_Script AddType(TypeObjectManager_Script_TypeInfo type_info)
    {
        ObjectType_Script instance = new ObjectType_Script();
        instance.TypeInfo = type_info;
        instance.transform.parent = GlobalCollector.Instance.TypedObjectsParent;
        AddType(instance);
        return instance;
    }

    public static void RemoveType(ObjectType_Script type)
    {
        fTypes.Remove(type);
    }

    public static void RemoveType(string type_name)
    {
        RemoveType(TypeInstance(type_name));
    }

    public static int TypeInstanceIndex(string type_name)
    {
        for (int i=0; i<fTypes.Count; i++)
        {
            if (fTypes[i].TypeInfo.TypeName == type_name)
                return i;
        }

        return -1;
    }

    public static ObjectType_Script TypeInstance(int index)
    {
        return fTypes[index];
    }

    public static ObjectType_Script TypeInstance(string type_name)
    {
        int index = TypeInstanceIndex(type_name);

        if (index == -1)
        {
            Debug.LogError("TypeObjectManager_Script_Object_Type.TypeInstance: Invalid type name (" + type_name + ")!");
            return null;
        }

        return fTypes[index];
    }

    public static bool HasType(string type_name)
    {
        return TypeInstance(type_name) != null;
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
