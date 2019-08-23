using UnityEngine;
using System.Collections;

public class ObjectTypeInfo_Script : MonoBehaviour
{
    [SerializeField]
    protected ObjectType_Script fObjectType = null;
    [SerializeField]
    protected TypeObjectManager_Script_TypeInfo fTypeInfo = null;

    public ObjectType_Script ObjectType
    {
        get
        {
            return fObjectType;
        }
        set
        {
            fObjectType = value;
            fTypeInfo = value.TypeInfo;
            name = fObjectType.TypeInfo.TypeName + fObjectType.InstanceHiId;
        }
    }

    public TypeObjectManager_Script_TypeInfo TypeInfo
    {
        get
        {
            return fTypeInfo;
        }
    }
    
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
