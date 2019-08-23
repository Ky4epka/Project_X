using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObject_Script : MonoBehaviour {

    public Transform Body = null;
    public GameObject Unit = null;
    public BoxCollider2D Body_Collider = null;

    public static bool IsObjectHasAccess(GameObject_Script obj)
    {
        return (obj != null) && (obj.isActiveAndEnabled);
    }

    public Vector3 Position
    {
        get
        {
            return Body.position;
        }
    }

    public Vector2Int MapPosition
    {
        get
        {
            return GlobalCollector.Instance.Current_Map.WorldToMapCoord(Position);
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
        if (Body == null)
            Body = this.GetComponent<Transform>();

        if (Unit == null)
            Unit = gameObject;

        if (Body_Collider == null)
            Body_Collider = this.GetComponent<BoxCollider2D>();
    }
}
