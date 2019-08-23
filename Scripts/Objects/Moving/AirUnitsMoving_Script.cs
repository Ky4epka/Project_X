using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameObject))]
public class AirUnitsMoving_Script : MovingObject_Script {

    public float NormalSpeed = 1f;
    public float NormalRotateSpeed = 1f;
    public float AimingRotateSpeed = 1f;
    public int AimingDistance = 1;

    protected MapCell_Script fCurrentCell = null;

    public override bool SetTarget(Vector3 pos)
    {
        if (base.SetTarget(pos))
        {
            fUnit_Comp.RotatingObject_Comp.SetTarget(pos);
            return true;
        }

        return false;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        if (base.SetTarget(target))
        {
            fUnit_Comp.RotatingObject_Comp.SetTarget(target);
            return true;
        }

        return false;
    }

    private new bool SetTarget(ObjectComponentsCollection_Script target, int min_distance)
    {
        return false;
    }

    public override bool CancelTarget()
    {
        if (base.CancelTarget())
        {
            fUnit_Comp.RotatingObject_Comp.CancelTarget();
            return true;
        }

        return false;
    }

    public override Vector3 Position
    {
        get
        {
            return base.Position;
        }

        set
        {
            if (fCurrentCell != null)
            {
                fCurrentCell.RemoveContainingObject(fUnit);
                fCurrentCell = null;
            }

            base.Position = value;

            MapCell_Script cell = GlobalCollector.Instance.Current_Map.GetMapCell(value);

            if (cell != null)
            {
                cell.AddContainingObject(fUnit);
            }

            fCurrentCell = cell;
        }
    }

    public override bool IsMoving()
    {
        return base.IsMoving();
    }

    protected override void TryMoveBody()
    {
        Vector3 target_pos;

        if (!GetTargetWorldPoint(out target_pos))
            return;

        float koef = Vector3.Distance(Position, target_pos) / (AimingDistance * GlobalCollector.Cell_Size);

        if (koef < 0.3f)
            koef = 0.3f;
        else if (koef > 1f)
            koef = 1f;
        
        Vector3 pos = Position + fUnit_Comp.RotatingObject_Comp.Direction * NormalSpeed * Time.fixedDeltaTime * (koef);
        fUnit_Comp.RotatingObject_Comp.RotateSpeed = AimingRotateSpeed * (1f - koef) + NormalRotateSpeed;

        if (Vector3.Distance(pos, target_pos) <= NormalSpeed * Time.fixedDeltaTime * 2)
        {
            CancelTarget();
        }

        Position = pos;
    }

    private void FixedUpdate()
    {
        TryMoveBody();
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
        MoveType = MovingObject_Script_MoveType.Air;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
