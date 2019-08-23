using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MovingObject_Script_MoveTargetType
{
    None,
    Point,
    Object
}

public enum MovingObject_Script_MoveType
{
    None,
    Ground,
    Air,
    Projectile
}

public class MovingObject_Script : HoldTargetBase_Script {
    
        public NotifyEvent<MovingObject_Script> OnOutOfMapBounds = new NotifyEvent<MovingObject_Script>();
        public NotifyEvent_3P<MovingObject_Script, Collider2D[], int> OnObjectCollission = new NotifyEvent_3P<MovingObject_Script, Collider2D[], int>();
        public NotifyEvent<MovingObject_Script> OnTargetAchieved = new NotifyEvent<MovingObject_Script>();
        public NotifyEvent<MovingObject_Script> OnOutOfPath = new NotifyEvent<MovingObject_Script>();
        

    [SerializeField]
        protected ObjectComponentsCollection_Script fUnit_Comp = null;
    [SerializeField]
    protected MovingObject_Script_MoveTargetType fMoveTargetType = MovingObject_Script_MoveTargetType.None;
    [SerializeField]
    protected Vector3 fTargetPoint = Vector3.zero;
    [SerializeField]
    protected int fTargetObjectMinDistance = 0;
    [SerializeField]
    protected float fMaxPathValue = 0f;
    [SerializeField]
    protected float fPathValue = 0f;
    [SerializeField]
    protected bool fDisableTracking = false;
    [SerializeField]
    protected MovingObject_Script_MoveType fMoveType = MovingObject_Script_MoveType.None;



    public virtual bool SetTarget(Vector3 pos)
    {
        if (DisallowTarget)
            return false;

        base.CancelTarget();
        fTargetPoint = pos;
        fMoveTargetType = MovingObject_Script_MoveTargetType.Point;
        fTargetObjectMinDistance = 0;
        fPathValue = 0f;
        return true;
    }

    // min_dist - in map cells
    public virtual bool SetTarget(ObjectComponentsCollection_Script target, int min_distance)
    {
        if (base.SetTarget(target))
        {
            fPathValue = 0f;
            fMoveTargetType = MovingObject_Script_MoveTargetType.Object;

            if (min_distance <= 0)
                min_distance = GetTargetMinDistanceBottom();

            fTargetObjectMinDistance = min_distance;
            return true;
        }

        return false;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        return SetTarget(target, GetTargetMinDistanceBottom());
    }

    public override bool CancelTarget()
    {
        if (base.CancelTarget())
        {
            fMoveTargetType = MovingObject_Script_MoveTargetType.None;
            return true;
        }

        return false;
    }

    public virtual Vector3 Position
    {
        get
        {
            return fUnit_Comp.GameObject_Comp.Body.position;
        }

        set
        {
            if (DisallowTarget)
                return;

            fUnit_Comp.GameObject_Comp.Body.position = value;
        }
    }

    public virtual Vector2Int MapPosition
    {
        get
        {
            return GlobalCollector.Instance.Current_Map.WorldToMapCoord(fUnit_Comp.GameObject_Comp.Body.position);
        }

        set
        {
            if (DisallowTarget)
                return;

            Position = GlobalCollector.Instance.Current_Map.MapCoordToWorld(value);
        }
    }

    public virtual void ResetPosition()
    {
        Position = Position;
    }

    public virtual void ResetMapPosition()
    {

    }

    public virtual float MaxPathValue
    {
        get
        {
            return fMaxPathValue;
        }

        set
        {
            fMaxPathValue = value;
        }
    }

    public virtual bool DisableTracking
    {
        get
        {
            return fDisableTracking;
        }

        set
        {
            fDisableTracking = value;
        }
    }

    public virtual bool IsMoving()
    {
        return false;
    }

    public virtual MovingObject_Script_MoveType MoveType
    {
        get
        {
            return fMoveType;
        }

        set
        {
            fMoveType = value;
        }
    }

    protected virtual int GetTargetMinDistanceBottom()
    {
        return 0;
    }
    
    protected virtual bool GetTargetWorldPoint(out Vector3 result)
    {
        result = Vector3.zero;

        switch (fMoveTargetType)
        {
            case MovingObject_Script_MoveTargetType.Object:
                if (Target != null)
                {
                    result = Target.GameObject_Comp.Body.position;
                    return true;
                }

                break;
            case MovingObject_Script_MoveTargetType.Point:
                result = fTargetPoint;
                return true;
        }

        return false;
    }

    protected virtual Vector3 GetTargetWorldPoint()
    {
        Vector3 result;
        GetTargetWorldPoint(out result);
        return result;
    }

    protected virtual void TryMoveBody()
    {

    }

    protected void Invoke_OnOutOfMapBounds()
    {
        OnOutOfMapBounds.Invoke(this);
    }

    protected void Invoke_OnOutOfPath()
    {
        OnOutOfPath.Invoke(this);
    }

    protected void Invoke_OnTargetAchieved()
    {
        OnTargetAchieved.Invoke(this);
    }

    protected void Invoke_OnObjectCollission(Collider2D[] col_list, int count)
    {
        OnObjectCollission.Invoke(this, col_list, count);
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
        fUnit_Comp = this.GetComponent<ObjectComponentsCollection_Script>();
	}


    // Update is called once per frame
    void Update () {
	}
}
