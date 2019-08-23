using UnityEngine;
using System.Collections;

public class BuildingMovingObject_Script : MovingObject_Script
{

    [SerializeField]
    protected Vector2Int fMapPosition = Vector2Int.zero;

    public override bool SetTarget(Vector3 pos)
    {
        // Buildings no follows to target
        return false;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        // Buildings no follows to target
        return false;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target, int min_distance)
    {
        // Buildings no follows to target
        return false;
    }

    public override bool CancelTarget()
    {
        return false;
    }

    private new Vector3 Position
    // Hides world position changing
    {
        get
        {
            return Vector3.zero;
        }

        set
        {
        }
    }

    public override Vector2Int MapPosition
    {
        get
        {
            return fMapPosition;
        }

        set
        {
            Vector2Int size = fUnit_Comp.BuildingObject_Comp.Size;
            MapCell_Script cell;
            Vector2Int pos;

            for (int i = 0; i < size.y; i++)
            {
                for (int j = 0; j < size.x; j++)
                {
                    pos = fMapPosition + new Vector2Int(j, i);

                    if (GlobalCollector.Instance.Current_Map.IsValidMapCoords(pos))
                    {
                        cell = GlobalCollector.Instance.Current_Map.GetMapCell(pos);
                        cell.RemoveContainingObject(fUnit);
                    }
                }
            }

            fMapPosition = value;
            fUnit_Comp.GameObject_Comp.Body.position = GlobalCollector.Instance.Current_Map.MapCoordToWorldWithoutCentroid(value) + 
                                                       GlobalCollector.Instance.Current_Map.MapCoordToWorldWithoutCentroid_Float(fUnit_Comp.BuildingObject_Comp.Size.x / 2f,
                                                                                                                                 fUnit_Comp.BuildingObject_Comp.Size.y / 2f);

            for (int i = 0; i < size.y; i++)
            {
                for (int j = 0; j < size.x; j++)
                {
                    pos = fMapPosition + new Vector2Int(j, i);

                    if (GlobalCollector.Instance.Current_Map.IsValidMapCoords(pos))
                    {
                        cell = GlobalCollector.Instance.Current_Map.GetMapCell(pos);
                        cell.AddContainingObject(fUnit);
                    }
                }
            }
        }
    }

    private new void ResetPosition()
    {

    }

    public override void ResetMapPosition()
    {
        MapPosition = GlobalCollector.Instance.Current_Map.WorldToMapCoord(fUnit_Comp.GameObject_Comp.Body.position - (Vector3)(fUnit_Comp.BuildingObject_Comp.WorldSize / 2));
    }

    protected override void TryMoveBody()
    {
        // Buildings no moving properties
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        MoveType = MovingObject_Script_MoveType.Ground;
        ResetPosition();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
