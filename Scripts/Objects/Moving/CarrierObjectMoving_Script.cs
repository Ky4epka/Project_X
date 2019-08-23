using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CarrierObjectMoving_Script_FlyStage
{
    None,
    Fly,
    Grounding
}

public enum CarrierObjectMoving_Script_MoveTargetType
{
    None,
    Point,
    Object,
    LoadingPlatform
}

[RequireComponent(typeof(GameObject))]
public class CarrierObjectMoving_Script : MovingObject_Script
{
        public NotifyEvent_2P<CarrierObjectMoving_Script, Vector3> OnTargetPointAchieved = new NotifyEvent_2P<CarrierObjectMoving_Script, Vector3>();
        public NotifyEvent_2P<CarrierObjectMoving_Script, ObjectComponentsCollection_Script> OnTargetObjectAchieved = new NotifyEvent_2P<CarrierObjectMoving_Script, ObjectComponentsCollection_Script>();
        public NotifyEvent_2P<CarrierObjectMoving_Script, CarrierLoadingPlatform_Script> OnTargetPlatformAchieved = new NotifyEvent_2P<CarrierObjectMoving_Script, CarrierLoadingPlatform_Script>();

        public float NormalSpeed = 1f;
        public float NormalRotateSpeed = 1f;
        public float AimingRotateSpeed = 1f;
        public int AimingDistance = 1;

    [SerializeField]
        protected CarrierObjectMoving_Script_FlyStage fStage = CarrierObjectMoving_Script_FlyStage.None;
    [SerializeField]
    protected CarrierObjectMoving_Script_MoveTargetType fCarrierTargetType = CarrierObjectMoving_Script_MoveTargetType.None;
    [SerializeField]
    protected CarrierLoadingPlatform_Script fTargetPlatform = null;
    [SerializeField]
    protected bool fInLineFlag = false;
    [SerializeField]
    protected bool fTargetVectorAchieved = false;
        protected bool fInit = true;
    [SerializeField]
        protected object fData = null;


    public override bool SetTarget(Vector3 pos)
    {
        if (base.SetTarget(pos))
        {
            fUnit_Comp.RotatingObject_Comp.SetTarget(pos);
            Stage = CarrierObjectMoving_Script_FlyStage.Fly;
            fCarrierTargetType = CarrierObjectMoving_Script_MoveTargetType.Point;
            return true;
        }

        return false;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        if (base.SetTarget(target))
        {
            fUnit_Comp.RotatingObject_Comp.SetTarget(target);
            Stage = CarrierObjectMoving_Script_FlyStage.Fly;
            fCarrierTargetType = CarrierObjectMoving_Script_MoveTargetType.Object;
            return true;
        }

        return false;
    }

    public virtual bool SetTarget(CarrierLoadingPlatform_Script target)
    {
        if (fDisallowTarget)
            return false;

        fTargetPlatform = target;
        fTargetPlatform.OnDestroy.AddListener(Event_OnPlatformDestroyed);
        fCarrierTargetType = CarrierObjectMoving_Script_MoveTargetType.LoadingPlatform;
        Stage = CarrierObjectMoving_Script_FlyStage.Fly;
        fUnit_Comp.RotatingObject_Comp.SetTarget(target.Position);
        return true;
    }

    private new bool SetTarget(ObjectComponentsCollection_Script target, int min_distance)
    {
        return false;
    }

    public override bool CancelTarget()
    {
        if (fCarrierTargetType != CarrierObjectMoving_Script_MoveTargetType.None)
        {
            fUnit_Comp.RotatingObject_Comp.CancelTarget();
            fMoveTargetType = MovingObject_Script_MoveTargetType.None;
            Stage = CarrierObjectMoving_Script_FlyStage.None;
            fCarrierTargetType = CarrierObjectMoving_Script_MoveTargetType.None;

            if (fTargetPlatform != null)
            {
                fTargetPlatform.OnDestroy.RemoveListener(Event_OnPlatformDestroyed);
                fTargetPlatform = null;
            }

            OnTargetCancel.Invoke(this);
            return true;
        }

        return false;
    }

    public object Data
    {
        get
        {
            return fData;
        }

        set
        {
            fData = value;
        }
    }

    public override Vector3 Position
    {
        get
        {
            return base.Position;
        }

        set
        {
            base.Position = value;
        }
    }

    public override bool IsMoving()
    {
        return base.IsMoving();
    }

    public void Event_OnTargetLineEnter(ObjectComponentsCollection_Script sender)
    {
        fInLineFlag = true;
    }

    public void Event_OnTargetVectorAchieved(ObjectComponentsCollection_Script sender)
    {
        fTargetVectorAchieved = true;
    }

    public void Event_OnPlatformDestroyed(CarrierLoadingPlatform_Script sender)
    {
        CancelTarget();
    }

    protected virtual CarrierObjectMoving_Script_FlyStage Stage
    {
        get
        {
            return fStage;
        }

        set
        {
            fStage = value;
        }
    }

    protected override bool GetTargetWorldPoint(out Vector3 point)
    {
        point = Vector3.zero;

        switch (fCarrierTargetType)
        {
            case CarrierObjectMoving_Script_MoveTargetType.Object:
                if (fTarget_Comp != null)
                {
                    point = fTarget_Comp.GameObject_Comp.Body.position;
                    return true;
                }

                break;
            case CarrierObjectMoving_Script_MoveTargetType.Point:
                point = fTargetPoint;
                return true;
            case CarrierObjectMoving_Script_MoveTargetType.LoadingPlatform:
                if (fTargetPlatform != null)
                {
                    point = fTargetPlatform.Position;
                    return true;
                }

                break;
        }

        return false;
    }

    protected override void TryMoveBody()
    {
        Vector3 target_pos;

        if (!GetTargetWorldPoint(out target_pos))
            return;

        switch (fStage)
        {
            case CarrierObjectMoving_Script_FlyStage.None:
                break;
            case CarrierObjectMoving_Script_FlyStage.Grounding:
                if (fTargetVectorAchieved)
                {
                    fInLineFlag = false;
                    OnTargetAchieved.Invoke(this);
                    Vector3 cache_pt = fTargetPoint;
                    ObjectComponentsCollection_Script cache_obj = fTarget_Comp;
                    CarrierLoadingPlatform_Script cache_platform = fTargetPlatform;
                    CarrierObjectMoving_Script_MoveTargetType cache_tt = fCarrierTargetType;


                    switch (cache_tt)
                    {
                        case CarrierObjectMoving_Script_MoveTargetType.Point:
                            OnTargetPointAchieved.Invoke(this, cache_pt);
                            break;
                        case CarrierObjectMoving_Script_MoveTargetType.Object:
                            OnTargetObjectAchieved.Invoke(this, cache_obj);
                            break;
                        case CarrierObjectMoving_Script_MoveTargetType.LoadingPlatform:
                            OnTargetPlatformAchieved.Invoke(this, cache_platform);
                            break;
                    }

                    CancelTarget();
                }

                break;
            case CarrierObjectMoving_Script_FlyStage.Fly:
                float koef = Vector3.Distance(Position, target_pos) / (AimingDistance * GlobalCollector.Cell_Size);

                if (koef < 0.3f)
                    koef = 0.3f;
                else if (koef > 1f)
                    koef = 1f;

                Vector3 pos = Position + fUnit_Comp.RotatingObject_Comp.Direction * NormalSpeed * Time.fixedDeltaTime * (koef);
                fUnit_Comp.RotatingObject_Comp.RotateSpeed = AimingRotateSpeed * (1f - koef) + NormalRotateSpeed;

                if (Vector3.Distance(pos, target_pos) <= NormalSpeed * Time.fixedDeltaTime * 1.2)
                {
                    pos = target_pos;
                    Stage = CarrierObjectMoving_Script_FlyStage.Grounding;
                    Vector3 vector;

                    switch (fCarrierTargetType)
                    {
                        case CarrierObjectMoving_Script_MoveTargetType.Point:
                            vector = Vector3.up;
                            break;
                        case CarrierObjectMoving_Script_MoveTargetType.Object:
                            vector = fTarget_Comp.GameObject_Comp.Body.right;
                            break;
                        case CarrierObjectMoving_Script_MoveTargetType.LoadingPlatform:
                            vector = fTargetPlatform.Direction;
                            break;
                        default:
                            vector = Vector3.up;
                            break;
                    }

                    fInLineFlag = false;
                    fTargetVectorAchieved = false;
                    fUnit_Comp.RotatingObject_Comp.SetTargetAbsoluteVector(vector);
                }

                Position = pos;
                break;
        }
    }

    private void FixedUpdate()
    {
        TryMoveBody();
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        MoveType = MovingObject_Script_MoveType.Air;
    }

    // Update is called once per frame
    void Update()
    {
        if (fInit)
        {
            fUnit_Comp.RotatingObject_Comp.OnTargetLineEnter.AddListener(Event_OnTargetLineEnter);
            fUnit_Comp.RotatingObject_Comp.OnTargetVectorAchieved.AddListener(Event_OnTargetVectorAchieved);
        }
    }
}
