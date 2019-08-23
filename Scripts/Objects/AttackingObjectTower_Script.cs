using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public enum AttackingObjectTower_Script_TowerTypes
{
    None,
    Fixed,
    FreeRotate
}

public class AttackingObjectTower_Script : HoldTargetBase_Script
{
        public ObjectComponentsCollection_Script Direct_BodyObject = null;
    
        public AttackingObjectTower_Script_TowerTypes Default_TowerType = AttackingObjectTower_Script_TowerTypes.Fixed;

        public List<Transform> Tower_ProjectilePivots = new List<Transform>();

        public NotifyEvent<ObjectComponentsCollection_Script> OnTowerBodyRotate = new NotifyEvent<ObjectComponentsCollection_Script>();

    [SerializeField]
    private ObjectComponentsCollection_Script fTowerBody_Unit = null;
    [SerializeField]
    private ObjectComponentsCollection_Script fActiveBody_Comp = null;
    [SerializeField]
    private AttackingObjectTower_Script_TowerTypes fTowerType = AttackingObjectTower_Script_TowerTypes.None;
    [SerializeField]
    private bool fInit = true;

    public float TestAngle = 0;

    public void RotateToAbsoluteVector(Vector3 vector)
    {
        if (HasTarget())
            return;

        fTowerBody_Unit.RotatingObject_Comp.SetTargetAbsoluteVector(vector);
    }

    public void RotateToRelativeVector(Vector3 vector)
    {
        if (HasTarget())
            return;

        fTowerBody_Unit.RotatingObject_Comp.SetTargetRelativeVector(vector);
    }

    public void RotateToBodyDirection()
    {
        if (HasTarget())
            return;

        fTowerBody_Unit.RotatingObject_Comp.SetTargetLocalAbsoluteVector(Vector2.right);
    }

    public void RotateToAbsoluteAngle(float angle)
    {
        if (HasTarget())
            return;

        fTowerBody_Unit.RotatingObject_Comp.SetTargetAbsoluteAngle(angle);
    }

    public void RotateToRelativeAngle(float angle)
    {
        if (HasTarget())
            return;

        fTowerBody_Unit.RotatingObject_Comp.SetTargetRelativeAngle(angle);
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        if (base.SetTarget(target))
        {
            Direct_BodyObject.AttackingObject_Comp.SetTarget(target);

            switch (fTowerType)
            {
                case AttackingObjectTower_Script_TowerTypes.FreeRotate:
                    fTowerBody_Unit.RotatingObject_Comp.SetTarget(target);
                    break;
            }

            return true;
        }

        return false;
    }

    public override bool CancelTarget()
    {
        if (base.CancelTarget())
        {            
            switch (fTowerType)
            {
                case AttackingObjectTower_Script_TowerTypes.FreeRotate:
                    fTowerBody_Unit.RotatingObject_Comp.CancelTarget();
                    break;
            }

            return true;
        }

        return false;
    }

    public Vector3 GetDirection()
    {
        if (fActiveBody_Comp != null)
        {
            return fActiveBody_Comp.GameObject_Comp.Body.right;
        }

        return Vector3.zero;
    }

    public Vector3 Direction
    {
        get
        {
            return GetDirection();
        }
    }

    public void SetTowerType(AttackingObjectTower_Script_TowerTypes type)
    {
        if (fTowerType == type)
            return;

        if (fActiveBody_Comp != null)
            fActiveBody_Comp.RotatingObject_Comp.OnRotating.RemoveListener(RotatingObject_OnRotate);

        fTowerType = type;

        switch (type)
        {
            case AttackingObjectTower_Script_TowerTypes.Fixed:
                fActiveBody_Comp = Direct_BodyObject;
                break;
            case AttackingObjectTower_Script_TowerTypes.FreeRotate:
                fActiveBody_Comp = fTowerBody_Unit;
                break;
            default:
                fActiveBody_Comp = null;
                break;
        }

        if (fActiveBody_Comp != null)
        {
            fActiveBody_Comp.RotatingObject_Comp.OnRotating.AddListener(RotatingObject_OnRotate);
        }
    }

    public AttackingObjectTower_Script_TowerTypes GetTowerType()
    {
        return fTowerType;
    }

    public AttackingObjectTower_Script_TowerTypes TowerType
    {
        get
        {
            return GetTowerType();
        }

        set
        {
            SetTowerType(value);
        }
    }

    public bool GetAngleDelta(out float angle)
    {
        angle = 360;

        if (!HasTarget())
            return false;

        if (fActiveBody_Comp != null)
        {
            angle = Vector3.SignedAngle(fActiveBody_Comp.GameObject_Comp.Body.right,
                                        fTarget_Comp.GameObject_Comp.Body.position - fActiveBody_Comp.GameObject_Comp.Body.position, Vector3.forward);
            TestAngle = angle;
            return true;
        }

        return false;
    }

    public float GetAngleDelta()
    {
        float result;
        GetAngleDelta(out result);
        return result;
    }

    public void RotatingObject_OnRotate(ObjectComponentsCollection_Script sender, float angle_delta)
    {
        OnTowerBodyRotate.Invoke(sender);
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        fTowerBody_Unit = this.GetComponent<ObjectComponentsCollection_Script>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fInit)
        {
            fInit = false;
            SetTowerType(Default_TowerType);
        }
    }
}
