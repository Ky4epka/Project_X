using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearMovingObject_Script : MovingObject_Script {

        public Collider2D Collider = null;
        public float Speed = 1f;

        protected Collider2D[] fOverlaps = new Collider2D[MAX_OVERLAP_COUNT];

        protected const int MAX_OVERLAP_COUNT = 10;


    public override bool SetTarget(Vector3 pos)
    {
        Debug.Log("SetTarget_UnitComp: " + fUnit_Comp);
        Debug.Log("SetTarget_UnitComp_GameObject: " + fUnit_Comp.GameObject_Comp);
        Debug.Log("SetTarget_UnitComp_GameObject_Body: " + fUnit_Comp.GameObject_Comp.Body);
        pos = fUnit_Comp.GameObject_Comp.Body.position + fUnit_Comp.GameObject_Comp.Body.right *
              Vector3.Distance(fUnit_Comp.GameObject_Comp.Body.position, pos);
        return base.SetTarget(pos);
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target, int min_distance)
    {
        return SetTarget(target.GameObject_Comp.Body.position);
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        return SetTarget(target.GameObject_Comp.Body.position);
    }

    private void FixedUpdate()
    {
        Vector3 target_pos;

        if (!GetTargetWorldPoint(out target_pos))
            return;

        float fixed_speed = Speed * Time.fixedDeltaTime;
        Vector3 cur_pos = fUnit_Comp.GameObject_Comp.Body.position;
        Vector3 next_pos = cur_pos + fUnit_Comp.GameObject_Comp.Body.right * fixed_speed;
        fPathValue += fixed_speed;

        ContactFilter2D col_filter = new ContactFilter2D();
        col_filter.NoFilter();
        int col_count = Collider.OverlapCollider(col_filter, fOverlaps);

        if (col_count > 0)
        {
            Invoke_OnObjectCollission(fOverlaps, col_count);
        }

        if (!GlobalCollector.Instance.Current_Map.IsValidWorldCoords(next_pos))
        {
            Invoke_OnOutOfMapBounds();
            return;
        }
        
        if ((!MathKit.NumbersEquals(fMaxPathValue, 0, Vector3.kEpsilon)) && (fPathValue / GlobalCollector.Cell_Size >= fMaxPathValue))
        {
            Invoke_OnOutOfPath();
            return;
        }

        if (Vector3.Distance(target_pos, next_pos) <= fixed_speed * 2f)
        {
            Invoke_OnTargetAchieved();
            return;
        }

        fUnit_Comp.GameObject_Comp.Body.position = next_pos;
    }

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
        MoveType = MovingObject_Script_MoveType.None;
	}

    protected void OnValidate()
    {
        Start();
        fUnit_Comp.Initialize();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
