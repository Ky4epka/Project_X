using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum RotatingObject_Script_Target
{
    None,
    Point,
    Object,
    AbsoluteAngle,
    RelativeAngle,
    AbsoluteVector,
    RelativeVector,
    LocalAbsoluteVector,
    LocalRelativeVector
}

[RequireComponent(typeof(ObjectComponentsCollection_Script))]
[RequireComponent(typeof(GameObject_Script))]
public class RotatingObject_Script : HoldTargetBase_Script {


    public bool Default_LockRotate = false;
    public float Default_RotateSpeed = 1f;

    // OnRotating<sender, angle_between_body_direction_and_target>
    public NotifyEvent_2P<ObjectComponentsCollection_Script, float> OnRotating = new NotifyEvent_2P<ObjectComponentsCollection_Script, float>();
    public NotifyEvent<ObjectComponentsCollection_Script> OnTargetLineEnter = new NotifyEvent<ObjectComponentsCollection_Script>();
    public NotifyEvent<ObjectComponentsCollection_Script> OnTargetLineStay = new NotifyEvent<ObjectComponentsCollection_Script>();
    public NotifyEvent<ObjectComponentsCollection_Script> OnTargetLineLeave = new NotifyEvent<ObjectComponentsCollection_Script>();
    public NotifyEvent<ObjectComponentsCollection_Script> OnTargetEnterFieldOfView = new NotifyEvent<ObjectComponentsCollection_Script>();
    public NotifyEvent<ObjectComponentsCollection_Script> OnTargetLeaveFieldOfView = new NotifyEvent<ObjectComponentsCollection_Script>();
    public NotifyEvent<ObjectComponentsCollection_Script> OnTargetStayFieldOfView = new NotifyEvent<ObjectComponentsCollection_Script>();
    public NotifyEvent<ObjectComponentsCollection_Script> OnTargetVectorAchieved = new NotifyEvent<ObjectComponentsCollection_Script>();

    [SerializeField]
    private ObjectComponentsCollection_Script fUnit_Comp = null;
    [SerializeField]
    private bool fLockRotate = true;
    [SerializeField]
    private float fFieldOfViewAngle = 1f;
    [SerializeField]
    private RotatingObject_Script_Target fTargetType = RotatingObject_Script_Target.None;
    [SerializeField]
    private Vector3 fTargetPoint = Vector3.zero;
    [SerializeField]
    private float fRotateSpeed = 1f;
    [SerializeField]
    private float fAngleDelta = 0f;
    [SerializeField]
    private bool fOnTargetLine = false;
    [SerializeField]
    private bool fOnFieldOfView = false;
    [SerializeField]
    private bool fInit = true;
    [SerializeField]
    private Vector2 fTargetVector = Vector2.zero;


    public bool SetTargetLocalAbsoluteVector(Vector2 vector)
    {
        CancelTarget();
        fTargetType = RotatingObject_Script_Target.LocalAbsoluteVector;
        fTargetVector = vector;

        fOnTargetLine = false;
        return true;
    }

    public bool SetTargetLocalRelativeVector(Vector2 vector)
    {
        CancelTarget();
        fTargetType = RotatingObject_Script_Target.LocalRelativeVector;
        Transform parent = fUnit_Comp.GameObject_Comp.Body.parent;

        if (parent != null)
            fTargetVector = parent.InverseTransformDirection(fUnit_Comp.GameObject_Comp.Body.right) + (Vector3)vector;
        else
            fTargetVector = fUnit_Comp.GameObject_Comp.Body.InverseTransformDirection(fUnit_Comp.GameObject_Comp.Body.right) + (Vector3)vector;

        fOnTargetLine = false;
        return true;
    }

    public bool SetTargetAbsoluteVector(Vector2 vector)
    {
        CancelTarget();
        fTargetType = RotatingObject_Script_Target.AbsoluteVector;
        fTargetVector = vector;

        fOnTargetLine = false;
        return true;
    }

    public bool SetTargetRelativeVector(Vector2 vector)
    {
        CancelTarget();
        fTargetType = RotatingObject_Script_Target.RelativeVector;
        fTargetVector = (Vector2)fUnit_Comp.GameObject_Comp.Body.right + vector;

        fOnTargetLine = false;
        return true;
    }

    public bool SetTargetAbsoluteAngle(float angle)
    { 
        return SetTargetAbsoluteVector(new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)));
    }

    public bool SetTargetRelativeAngle(float angle)
    {
        angle += fUnit_Comp.GameObject_Comp.Body.localEulerAngles.z;
        return SetTargetRelativeVector(new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)));
    }

    public bool SetTarget(Vector3 target)
    {
        CancelTarget();
        fTargetPoint = target;
        fTargetType = RotatingObject_Script_Target.Point;
        fOnTargetLine = false;
        return true;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        if (base.SetTarget(target))
        {
            fTargetType = RotatingObject_Script_Target.Object;
            fOnTargetLine = false;
            return true;
        }

        return false;
    }

    public override bool CancelTarget()
    {
        if (base.CancelTarget())
        {
            fTargetType = RotatingObject_Script_Target.None;
            fOnTargetLine = false;
            return true;
        }

        return false;
    }

    public bool LockRotate
    {
        get
        {
            return fLockRotate;
        }

        set
        {
            fLockRotate = value;
        }
    }

    public float RotateSpeed
    {
        get
        {
            return fRotateSpeed;
        }

        set
        {
            fRotateSpeed = value;
        }
    }

    public bool TargetOnLine
    {
        get
        {
            return fOnTargetLine;
        }
    }

    public Vector3 GetTargetPoint()
    {
        switch (fTargetType)
        {
            case RotatingObject_Script_Target.None:
                break;
            case RotatingObject_Script_Target.Point:
                return fTargetPoint;
            case RotatingObject_Script_Target.Object:
                return Target.GameObject_Comp.Body.position;
        }

        return Vector3.zero;
    }

    public override bool HasTarget()
    {
        return ((fTargetType == RotatingObject_Script_Target.Object) && (Target != null)) ||
               (fTargetType != RotatingObject_Script_Target.None);
    }

    public float GetAngleDelta(out bool has)
    {
        has = HasTarget();

        if (has)
        {
            return fAngleDelta;
        }

        return 360f;
    }

    public float GetAngleDelta()
    {
        bool has;
        return GetAngleDelta(out has);
    }

    public Vector3 Direction
    {
        get
        {
            return fUnit_Comp.GameObject_Comp.Body.right;
        }

        set
        {
            CancelTarget();
            fUnit_Comp.GameObject_Comp.Body.right = value;
        }
    }
    
    private void FixedUpdate()
    {
        if ((fLockRotate) || (fTargetType == RotatingObject_Script_Target.None))
            return;

        Vector3 body_pos = fUnit_Comp.GameObject_Comp.Body.position;
        Vector3 body_dir = fUnit_Comp.GameObject_Comp.Body.right;
        Vector3 target_pos = GetTargetPoint();
        Vector3 delta_norm = Vector3.zero;
        Transform parent; 
        
        switch (fTargetType)
        {
            case RotatingObject_Script_Target.AbsoluteVector:
                delta_norm = fTargetVector;
                break;
            case RotatingObject_Script_Target.RelativeVector:
                delta_norm = fTargetVector;
                break;
            case RotatingObject_Script_Target.Object:
                delta_norm = (target_pos - body_pos).normalized;
                break;
            case RotatingObject_Script_Target.Point:
                delta_norm = (target_pos - body_pos).normalized;
                break;
            case RotatingObject_Script_Target.LocalAbsoluteVector:
                parent = fUnit_Comp.GameObject_Comp.Body.parent;

                if (parent != null)
                    delta_norm = parent.TransformDirection(fTargetVector);
                else
                    delta_norm = fUnit_Comp.GameObject_Comp.Body.TransformDirection(fTargetVector);

                break;
            case RotatingObject_Script_Target.LocalRelativeVector:
                parent = fUnit_Comp.GameObject_Comp.Body.parent;

                if (parent != null)
                    delta_norm = parent.TransformDirection(fTargetVector);
                else
                    delta_norm = fUnit_Comp.GameObject_Comp.Body.TransformDirection(fTargetVector);

                break;
        }

        if (MathKit.Vectors3DEquals(delta_norm, Vector3.zero))
            delta_norm = body_dir;

        float angle = Vector3.SignedAngle(body_dir, delta_norm, Vector3.forward);
        float angle_sign = Mathf.Sign(angle);

        float speed_fixed = fRotateSpeed * Time.fixedDeltaTime;
        Quaternion q = fUnit_Comp.GameObject_Comp.Body.rotation;

        if (Mathf.Abs(angle) <= speed_fixed * 2f)
        {
            q.eulerAngles = new Vector3(0, 0, Mathf.Atan2(delta_norm.y, delta_norm.x) * Mathf.Rad2Deg);

            if ((fTargetType == RotatingObject_Script_Target.AbsoluteVector) ||
                (fTargetType == RotatingObject_Script_Target.RelativeVector) ||
                (fTargetType == RotatingObject_Script_Target.LocalAbsoluteVector) ||
                (fTargetType == RotatingObject_Script_Target.LocalRelativeVector))
            {
                CancelTarget();

                OnTargetVectorAchieved.Invoke(fUnit_Comp);
            }
            else
            {
                if (!fOnTargetLine)
                    OnTargetLineEnter.Invoke(fUnit_Comp);

                OnTargetLineStay.Invoke(fUnit_Comp);
                fOnTargetLine = true;
            }
        }
        else
        {
            if (fOnTargetLine)
                OnTargetLineLeave.Invoke(fUnit_Comp);

            fOnTargetLine = false;
            q.eulerAngles += new Vector3(0, 0, speed_fixed * angle_sign);
        }

        if (Mathf.Abs(angle) <= fFieldOfViewAngle)
        {
            if (!fOnFieldOfView)
                OnTargetEnterFieldOfView.Invoke(fUnit_Comp);

            OnTargetStayFieldOfView.Invoke(fUnit_Comp);
            fOnFieldOfView = true;
        }
        else
        {
            if (fOnFieldOfView)
                OnTargetLeaveFieldOfView.Invoke(fUnit_Comp);

            fOnFieldOfView = false;
        }

        fUnit_Comp.GameObject_Comp.Body.rotation = q;
        fAngleDelta = angle;
        OnRotating.Invoke(fUnit_Comp, angle);
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
        fUnit_Comp = this.GetComponent<ObjectComponentsCollection_Script>();
	}

    // Update is called once per frame
    void Update () {
        if (fInit)
        {
            fInit = false;
            LockRotate = Default_LockRotate;
            RotateSpeed = Default_RotateSpeed;
        }
	}
}
