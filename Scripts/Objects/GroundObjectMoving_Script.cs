using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundObjectMoving_Script_Action
{
    None,
    SetTargetPoint,
    SetTargetObject,
    CancelTarget
}

[RequireComponent(typeof(GameObject_Script))]
[RequireComponent(typeof(RotatingObject_Script))]
public class GroundObjectMoving_Script : MovingObject_Script {
    
    public float Speed = 1f; // Units per second

    public NotifyEvent_2P<GroundObjectMoving_Script, Vector2Int> OnMapCellDrived = new NotifyEvent_2P<GroundObjectMoving_Script, Vector2Int>();

    [SerializeField]
    private Vector3 fAction_TargetPoint = Vector3.zero;
    [SerializeField]
    private ObjectComponentsCollection_Script fAction_TargetObject = null;
    [SerializeField]
    private GroundObjectMoving_Script_Action fAction = GroundObjectMoving_Script_Action.None;
    [SerializeField]
    private int fAction_TargetObjectMinDistance = 0;

    [SerializeField]
    private Vector2Int[] fPathToTarget = null;
    private int fPathToTargetSize = 0;
    private Vector2Int fStartPathPoint = Vector2Int.zero;
    private Vector2Int fEndPathPoint = Vector2Int.zero;
    [SerializeField]
    private bool fPathFound = false;
    [SerializeField]
    private Vector2Int fLastTargetObjPos = Vector2Int.zero;

    [SerializeField]
    private int fFixedFramesCounter = 0;
    [SerializeField]
    private bool fInit = true;
    [SerializeField]
    private MapCell_Script fCurrentCell = null;
    [SerializeField]
    private bool fNextCellIsBusy = false;
    [SerializeField]
    private bool fMoving = false;
    [SerializeField]
    private Vector3 fLastNextPoint = Vector3.zero;
    [SerializeField]
    private bool fTargetOnLineFlag = false;
    [SerializeField]
    private bool fPathUpdated = false;
        
    private const int FFC_PATH_UPDATE = 10;
    private const int MIN_TARGET_DISTANCE_BOTTOM = 1;


    public override bool SetTarget(Vector3 target)
    {
        fAction = GroundObjectMoving_Script_Action.SetTargetPoint;
        fAction_TargetPoint = target;
        fAction_TargetObjectMinDistance = 0;

        if (!IsMoving() || 
            fNextCellIsBusy)
        {
            Process_Action();
        }

        return true;
    }

    // min_dist - in map cells
    public override bool SetTarget(ObjectComponentsCollection_Script target, int min_distance)
    {
        fAction = GroundObjectMoving_Script_Action.SetTargetObject;
        fAction_TargetObject = target;
        fAction_TargetObjectMinDistance = min_distance;

        if (!IsMoving() ||
            fNextCellIsBusy)
        {
            Process_Action();
        }

        return true;
    }

    public override bool CancelTarget()
    {
        fAction = GroundObjectMoving_Script_Action.CancelTarget;

        if (!IsMoving() ||
            fNextCellIsBusy)
        {
            Process_Action();
        }

        return false;
    }

    public override bool IsMoving()
    {
        return fMoving;
    }

    public override bool HasTarget()
    {
        return fMoveTargetType != MovingObject_Script_MoveTargetType.None;
    }
    
    protected virtual bool Do_SetTarget(Vector3 target)
    {
        if (base.SetTarget(target))
        {
            fTargetObjectMinDistance = fAction_TargetObjectMinDistance;
            return true;
        }

        return false;
    }
    
    protected virtual bool Do_SetTarget(ObjectComponentsCollection_Script target, int min_distance)
    {
        if (base.SetTarget(target, min_distance))
        {
            return true;
        }

        return false;
    }

    protected virtual bool Do_CancelTarget()
    {
        if (base.CancelTarget())
        {
            fMoving = false;

            if (fPathToTarget != null)
                fPathToTargetSize = 0;

            fUnit_Comp.RotatingObject_Comp.CancelTarget();

            fPathFound = false;
            return true;
        }

        return false;
    }

    protected virtual void Process_Action()
    {
        switch (fAction)
        {
            case GroundObjectMoving_Script_Action.None:
                break;
            case GroundObjectMoving_Script_Action.SetTargetPoint:
                Do_SetTarget(fAction_TargetPoint);
                break;
            case GroundObjectMoving_Script_Action.SetTargetObject:
                Do_SetTarget(fAction_TargetObject, fAction_TargetObjectMinDistance);
                break;
            case GroundObjectMoving_Script_Action.CancelTarget:
                Do_CancelTarget();
                break;
        }

        fAction = GroundObjectMoving_Script_Action.None;
    }

    protected override int GetTargetMinDistanceBottom()
    {
        return MIN_TARGET_DISTANCE_BOTTOM;
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

            if (!fDisableTracking)
            {
                MapCell_Script cell = GlobalCollector.Instance.Current_Map.GetMapCell(value);

                if (fCurrentCell != null)
                {
                    fCurrentCell.RemoveContainingObject(fUnit);
                    fCurrentCell = null;
                }

                fCurrentCell = cell;

                if (fCurrentCell != null)
                {
                    fCurrentCell.AddContainingObject(fUnit);
                }
            }
        }
    }

    public override bool DisableTracking
    {
        get
        {
            return base.DisableTracking;
        }

        set
        {
            base.DisableTracking = value;

            if (value)
            {
                if (fCurrentCell != null)
                {
                    fCurrentCell.RemoveContainingObject(fUnit);
                    fCurrentCell = null;
                }
            }
            else
            {
                ResetPosition();
            }
        }
    }

    public void Event_OnTargetLineStay(ObjectComponentsCollection_Script sender)
    {
        fTargetOnLineFlag = true;
    }

    public void Event_OnTargetLineEnter(ObjectComponentsCollection_Script sender)
    {
        fTargetOnLineFlag = true;
    }

    public void Event_OnMapResize(Map_Script sender, int new_width, int new_height)
    {
        Vector2Int[] buffer = null;

        if (fPathToTarget != null)
        {
            buffer = new Vector2Int[fPathToTarget.Length];
            fPathToTarget.CopyTo(buffer, 0);
        }

        fPathToTarget = new Vector2Int[new_width * new_height];

        if (buffer != null)
        {
            buffer.CopyTo(fPathToTarget, 0);
        }
    }

    private bool MapCellAtPosContainsNullOrSelf(Vector2Int point)
    {
        MapCell_Script cell = GlobalCollector.Instance.Current_Map.GetMapCell(point);

        return ((!cell.HasGroundObjects()) ||
                (cell.HasObject(fUnit)));
    }

    private void PathUpdated()
    {
        if ((fPathToTargetSize == 0) &&
            (fMoveTargetType == MovingObject_Script_MoveTargetType.Object))
        {
            fUnit_Comp.RotatingObject_Comp.SetTarget(fTarget_Comp);
        }

        Debug.Log("Path updated");
    }

    private void TryUpdatePath()
    {
        Vector2Int target_pos = GlobalCollector.Instance.Current_Map.WorldToMapCoord(GetTargetWorldPoint());

        if (
                // <<
                // Текущая ячейка-цель не проходима
                ((fPathToTargetSize > 1) &&
                ( !GlobalCollector.Instance.Current_Map.IsCellPassable(fPathToTarget[fPathToTargetSize - 1]) ||
                 // Или уже содержит объект
                 (!MapCellAtPosContainsNullOrSelf(fPathToTarget[fPathToTargetSize - 1])))) ||
                // Координаты цели изменились и не располагаются в той же ячейке, что раньше
                (target_pos != fEndPathPoint)
               )
        {
            fPathUpdated = true;
            fLastTargetObjPos = target_pos;

            fPathFound = GlobalCollector.Instance.Current_Map.FindPath(MapPosition, target_pos, fPathToTarget, out fPathToTargetSize);
            fStartPathPoint = MapPosition;
            fEndPathPoint = target_pos;
            int min_dist = fTargetObjectMinDistance;

            if ((fPathToTargetSize > 0) &&
                !fPathFound)
            {
                min_dist = MathKit.EnsureRange(min_dist - Mathf.RoundToInt(Vector3.Distance(GlobalCollector.Instance.Current_Map.MapCoordToWorld(fPathToTarget[0]), 
                                                                                            GlobalCollector.Instance.Current_Map.MapCoordToWorld(target_pos)) / GlobalCollector.Cell_Size), 
                                               0, 
                                               fTargetObjectMinDistance);
            }

            int rc = Mathf.Min(min_dist, fPathToTargetSize);

            if (rc > 0)
                fPathToTargetSize = MathKit.EnsureRange(fPathToTargetSize - rc, 0, fPathToTargetSize);
            

            PathUpdated();
        }
    }

    private bool HasPath()
    {
        return (fPathToTargetSize > 0);
    }

    protected override void TryMoveBody()
    {
        Vector3 next_cell_pos;
        Vector2Int m_next_cell_pos;

        if (HasPath())
        {
            m_next_cell_pos = fPathToTarget[fPathToTargetSize - 1];
            next_cell_pos = GlobalCollector.Instance.Current_Map.MapCoordToWorld(m_next_cell_pos);
        }
        else
        {
            next_cell_pos = GetTargetWorldPoint();
            m_next_cell_pos = GlobalCollector.Instance.Current_Map.WorldToMapCoord(next_cell_pos);
        }

        if (//(fPathToTarget.Count > 0) &&
            !MathKit.Vectors3DEquals(next_cell_pos, fLastNextPoint))
        {
            fLastNextPoint = next_cell_pos;
            fTargetOnLineFlag = false;
            fUnit_Comp.RotatingObject_Comp.SetTarget(next_cell_pos);
        }

        if (true)
        {
            Vector3 body_pos = Position;
            MapCell_Script cell = GlobalCollector.Instance.Current_Map.GetMapCell(body_pos);
            MapCell_Script next_cell = GlobalCollector.Instance.Current_Map.GetMapCell(next_cell_pos);
            fNextCellIsBusy = !MapCellAtPosContainsNullOrSelf(m_next_cell_pos);
            bool fTOLFOld = fTargetOnLineFlag;
            fMoving = fTargetOnLineFlag &&
                      !fNextCellIsBusy && (next_cell.IsPassable());
            fTargetOnLineFlag = false;

            if (fMoving)
            {
                float fr_speed = Speed * cell.GroundSpeedScale * Time.fixedDeltaTime;
                Vector3 self_and_cur_dir = (next_cell_pos - body_pos).normalized;
                Vector3 calc_pos = body_pos + fr_speed * self_and_cur_dir;
                float dist = Vector3.Distance(calc_pos, next_cell_pos);
                
                if (dist <= fr_speed )
                {
                    if (fPathToTargetSize > 0)
                    {
                        fPathToTargetSize--;
                    }

                    Position = next_cell_pos;
                                        
                    OnMapCellDrived.Invoke(this, GlobalCollector.Instance.Current_Map.WorldToMapCoord(next_cell_pos));

                    if (fPathToTargetSize == 0)
                    {
                        fMoving = false;

                        switch (fMoveTargetType)
                        {
                            case MovingObject_Script_MoveTargetType.Object:
                                fUnit_Comp.RotatingObject_Comp.SetTarget(fTarget_Comp);
                                break;
                            case MovingObject_Script_MoveTargetType.Point:
                                fMoveTargetType = MovingObject_Script_MoveTargetType.None;
                                Do_CancelTarget();
                                break;
                        }

                        OnTargetAchieved.Invoke(this);
                    }

                    Process_Action();
                }
                else
                {
                    Position = calc_pos;
                }
            }
            else
            {
                if (fPathUpdated)
                {
                    if (fMoveTargetType == MovingObject_Script_MoveTargetType.Object)
                    {
                        if (fTOLFOld)
                        {
                            OnTargetAchieved.Invoke(this);
                            fPathUpdated = false;
                        }
                    }
                }
            }
        }

        /*
        if (HasPath())
        {
            Vector3 body_pos = Position;
            next_cell_pos = fPathToTarget[0];
            MapCell_Script cell = GlobalCollector.Instance.Current_Map.GetMapCell(body_pos);
            fNextCellIsBusy = !MapCellAtPosContainsNullOrSelf(next_cell_pos);
            fMoving = fUnit_Comp.RotatingObject_Comp.TargetOnLine &&
                      !fNextCellIsBusy;

            if (fMoving)
            {
                float fr_speed = Speed * cell.Default_GroundSpeedScale * Time.fixedDeltaTime;
                Vector3 self_and_cur_dir = (next_cell_pos - body_pos).normalized;
                Vector3 calc_pos = body_pos + fr_speed * self_and_cur_dir;
                float dist = Vector3.Distance(calc_pos, next_cell_pos);

                if (dist <= fr_speed)
                {
                    GlobalCollector.Instance.Current_Map.GetMapCell(GlobalCollector.Instance.Current_Map.WorldToMapCoord(fPathToTarget[0])).SetEditorState("find_path_cell", false);
                    fPathToTarget.RemoveAt(0);
                    Position = next_cell_pos;

                    Vector3 cpos = Vector3.zero;

                    if (fPathToTarget.Count > 0)
                        cpos = fPathToTarget[0];

                    OnMapCellDrived.Invoke(this, GlobalCollector.Instance.Current_Map.WorldToMapCoord(next_cell_pos));

                    if (fPathToTarget.Count == 0)
                    {
                        fMoving = false;

                        switch (fMoveTargetType)
                        {
                            case MovingObject_Script_MoveTargetType.Object:
                                break;
                            case MovingObject_Script_MoveTargetType.Point:
                                fMoveTargetType = MovingObject_Script_MoveTargetType.None;
                                Do_CancelTarget();
                                break;
                        }

                        OnTargetAchieved.Invoke(this);
                    }

                    Process_Action();
                }
                else
                {
                    Position = calc_pos;
                }
            }
        }
        else
        {
            fMoving = false;
        }*/
    }

    private void FixedUpdate()
    {
        if (fMoveTargetType == MovingObject_Script_MoveTargetType.None)
            return;

        fFixedFramesCounter++;

        // Обновление пути
        if (fFixedFramesCounter % FFC_PATH_UPDATE == 0)
        {
            TryUpdatePath();
        }

        if (!DisallowTarget)
            TryMoveBody();
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        MoveType = MovingObject_Script_MoveType.Ground;
        GlobalCollector.Instance.Current_Map.OnResize.AddListener(Event_OnMapResize);
        Event_OnMapResize(GlobalCollector.Instance.Current_Map, 
                          GlobalCollector.Instance.Current_Map.GetMapSize().x, 
                          GlobalCollector.Instance.Current_Map.GetMapSize().y);
    }

    private void OnDestroy()
    {
        if (fCurrentCell != null)
        {
            fCurrentCell.RemoveContainingObject(fUnit);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fInit)
        {
            fInit = false;

            fUnit_Comp.RotatingObject_Comp.OnTargetLineStay.AddListener(Event_OnTargetLineStay);
            fUnit_Comp.RotatingObject_Comp.OnTargetLineEnter.AddListener(Event_OnTargetLineEnter);
            ResetPosition();
        }
    }
}
