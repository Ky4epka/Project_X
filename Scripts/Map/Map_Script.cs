using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Map_Script_Path_Cell
{
    public int cost;
    public float dist_to_start;
    public float sum_dist;
    public int write_cnt;

    public Map_Script_Path_Cell(int ACost, float ADist_to_start, float ASum_dist, int AWrite_Cnt)
    {
        cost = ACost;
        dist_to_start = ADist_to_start;
        sum_dist = ASum_dist;
        write_cnt = AWrite_Cnt;
    }
}

public class Map_Script : MonoBehaviour {

    public int Default_MapWidth = 0;
    public int Default_MapHeight = 0;
    public float Default_WorldYCoordOffset = 0.2f;
    public MapCell_Script Default_Editor_Cell_Sample = null;
    public MapCell_Script Default_Game_Cell_Sample = null;
    public bool Default_IsEditorMode = false;
    public bool Default_UseGrid = false;

    public Transform Body = null;
    public Camera MainCamera = null;

    public NotifyEvent_3P<Map_Script, int, int> OnResize = new NotifyEvent_3P<Map_Script, int, int>();

    public delegate bool MatchCell(MapCell_Script cell, object arg);

    private bool fUseGrid = false;
    private int fWidth = 0;
    private int fHeight = 0;
    private List<List<MapCell_Script>> fCells = new List<List<MapCell_Script>>();
    private bool fIsEditorMode = false;
    private Map_Script_Path_Cell[,] fPathMap;
    private Vector2Int[] fNeigh_offsets = new Vector2Int[8];
    private List<Vector2Int> fOpen_cells = new List<Vector2Int>();

    private float fDistributingSpice = 0f;

    private const int PATH_MAP_EMPTY_CELL = -3;
    private const int PATH_MAP_START_CELL = -2;
    private const int PATH_MAP_END_CELL = -1;
    private const int PATH_MAP_BUSY_CELL = 0;

    private bool fInit = true;


    public void SetTerrainTextureSize(float width, float height)
    {
        for (int i = 0; i < fHeight; i++)
        {
            for (int j = 0; j < fWidth; j++)
            {
                fCells[i][j].SetMapCoord(j, i);
                fCells[i][j].Collider.size = new Vector2(width, height);
            }
        }
    }
    
    public void SetEditorMode(bool value)
    {
        fIsEditorMode = value;

        for (int i=0; i<fHeight; i++)
        {
            for (int j = 0; j < fWidth; j++)
            {
                MapCell_Script ncell;

                if (fIsEditorMode)
                {
                    ncell = MapCell_Script.Instantiate(Default_Editor_Cell_Sample);
                    ncell.Collider.size = new Vector2(GlobalCollector.Cell_Size, GlobalCollector.Cell_Size);
                }
                else
                    ncell = MapCell_Script.Instantiate(Default_Game_Cell_Sample);

                ncell.Unit.SetActive(true);
                ncell.Body.parent = Body;
                ncell.SetOwner(this);
                ncell.Assign(fCells[i][j]);
                MapCell_Script.Destroy(fCells[i][j]);
                fCells[i][j] = ncell;
            }
        }
    }

    public void SetUseGrid(bool value)
    {
        fUseGrid = value;

        for (int i = 0; i < fHeight; i++)
        {
            for (int j = 0; j < fWidth; j++)
            {
                fCells[i][j].SetBordered(value);
            }
        }
    }

    public MapCell_Script GetMapCell(int x, int y)
    {
        if (IsValidMapCoords(x, y))
            return fCells[y][x];
        else
        {
            return null;
        }
    }

    public MapCell_Script GetMapCell(Vector2Int pos)
    {
        return GetMapCell(pos.x, pos.y);
    }

    public MapCell_Script GetMapCell(Vector3 world_pos)
    {
        return GetMapCell(WorldToMapCoord(world_pos));
    }

    public Vector3 MapCoordToWorld(int x, int y)
    {
        Vector3 vec = Body.position + new Vector3(x * GlobalCollector.Cell_Size + GlobalCollector.Cell_Size / 2f, -y * GlobalCollector.Cell_Size - GlobalCollector.Cell_Size / 2f, 0);
        return vec;
    }

    public Vector3 MapCoordToWorld(Vector2Int coord)
    {
        return MapCoordToWorld(coord.x, coord.y);
    }

    public Vector3 MapCoordToWorldWithoutCentroid(int x, int y)
    {
        Vector3 vec = Body.position + new Vector3(x * GlobalCollector.Cell_Size, -y * GlobalCollector.Cell_Size, 0);
        return vec;
    }

    public Vector3 MapCoordToWorldWithoutCentroid(Vector2Int coord)
    {
        return MapCoordToWorldWithoutCentroid(coord.x, coord.y);
    }

    public void WorldToMapCoord(Vector3 pos, out int x, out int y)
    {
        pos -= Body.position;
        x = (int)((pos.x ) / GlobalCollector.Cell_Size);
        y = -(int)((pos.y) / GlobalCollector.Cell_Size);
    }

    public Vector2Int WorldToMapCoord(Vector3 pos)
    {
        int x, y;
        WorldToMapCoord(pos, out x, out y);
        return new Vector2Int(x, y);
    }

    public void WorldToMapCoordWithoutCentroid(Vector3 pos, out int x, out int y)
    {
        pos -= Body.position;
        x = (int)((pos.x) / GlobalCollector.Cell_Size);
        y = -(int)((pos.y) / GlobalCollector.Cell_Size);
    }

    public Vector2Int WorldToMapCoordWithoutCentroid(Vector3 pos)
    {
        int x, y;
        WorldToMapCoordWithoutCentroid(pos, out x, out y);
        return new Vector2Int(x, y);
    }
    
    public Vector3 MapCoordToWorldWithoutCentroid_Float(float x, float y)
    {
        return new Vector3(x * GlobalCollector.Cell_Size, -y * GlobalCollector.Cell_Size);
    }

    public RectInt Bounds
    {
        get
        {
            return new RectInt(0, 0, fWidth, fHeight);
        }
    }

    public Rect WorldBounds
    {
        get
        {
            Rect rect = new Rect();
            Vector3 topleft = MapCoordToWorldWithoutCentroid(0, 0);
            Vector3 bottomright = MapCoordToWorldWithoutCentroid(fWidth + 1, fHeight + 1);
            rect.min = topleft;
            rect.max = bottomright;
            return rect;
        }
    }

    public Vector2Int ValidateMapCoords(int x, int y)
    {
        if (x < 0)
            x = 0;
        else if (x >= fWidth)
            x = fWidth - 1;

        if (y < 0)
            y = 0;
        else if (y >= fHeight)
            y = fHeight - 1;

        return new Vector2Int(x, y);
    }

    public Vector2Int ValidateMapCoords(Vector2Int coord)
    {
        return ValidateMapCoords(coord.x, coord.y);
    }

    public bool IsValidXMapCoord(int x)
    {
        return (x >= 0) && (x < fWidth);
    }

    public bool IsValidYMapCoord(int y)
    {
        return (y >= 0) && (y < fHeight);
    }

    public bool IsValidMapCoords(int x, int y)
    {
        return IsValidXMapCoord(x) && IsValidYMapCoord(y);
    }

    public bool IsValidMapCoords(Vector2Int pos)
    {
        return IsValidMapCoords(pos.x, pos.y);
    }

    public bool IsValidWorldCoords(Vector3 pos)
    {
        return IsValidMapCoords(WorldToMapCoord(pos));
    }

    public bool IsEmptyMap()
    {
        return (fHeight == 0) || (fWidth == 0);
    }

    public bool IsCellAllowedToGroundObjects(int x, int y)
    {
        if (IsValidMapCoords(x, y))
        {
            MapCell_Script cell = fCells[y][x];

            return cell.IsPassable() && !cell.HasGroundObjects();
        }

        return false;
    }

    public bool IsCellAllowedToGroundObjects(Vector2Int point)
    {
        return IsCellAllowedToGroundObjects(point.x, point.y);
    }

    public bool IsCellAllowedToGroundObjects(Vector3 world_point)
    {
        return IsCellAllowedToGroundObjects(WorldToMapCoord(world_point));
    }

    public int FindAllowedCellsToGroundObjectAroundRectEx(RectInt rect, MapCell_Script [] result, int max_radius, int start_index, bool use_radius, bool append_cells)
    {
        int count = 0;

        if (append_cells)
            count = start_index;

        Vector2Int point = Vector2Int.zero;

        if (rect.size == Vector2Int.zero)
        {
            point = rect.position;

            if (IsCellAllowedToGroundObjects(point) &&
                (!use_radius || (Vector2.Distance(rect.center, point) <= max_radius)))
            {
                result[count] = fCells[point.y][point.x];
                count++;
            }
        }

        for (int i = rect.yMin; i <= rect.yMax; i++)
        {
            point.x = rect.xMin - 1;
            point.y = i;

            if (IsCellAllowedToGroundObjects(point) &&
                (!use_radius || (Vector2.Distance(rect.center, point) <= max_radius)))
            {
                result[count] = fCells[point.y][point.x];
                count++;
            }

            point.x = rect.xMax + 1;
            point.y = i;

            if (IsCellAllowedToGroundObjects(point) &&
                (!use_radius || (Vector2.Distance(rect.center, point) <= max_radius)))
            {
                result[count] = fCells[point.y][point.x];
                count++;
            }
        }
        
        for (int j = rect.xMin - 1; j <= rect.xMax + 1; j++)
        {
            point.x = j;
            point.y = rect.yMin - 1;

            if (IsCellAllowedToGroundObjects(point) &&
                (!use_radius || (Vector2.Distance(rect.center, point) <= max_radius)))
            {
                result[count] = fCells[point.y][point.x];
                count++;
            }

            point.x = j;
            point.y = rect.yMax + 1;

            if (IsCellAllowedToGroundObjects(point) &&
                (!use_radius || (Vector2.Distance(rect.center, point) <= max_radius)))
            {
                result[count] = fCells[point.y][point.x];
                count++;
            }
        }

        return count;
    }


    public int FindAllowedCellsToGroundObjectAroundRect(RectInt rect, MapCell_Script[] result)
    {
        return FindAllowedCellsToGroundObjectAroundRectEx(rect, result, 0, 0, false, false);
    }

    public int FindAllowedCelllsToGroundObjectAroundPointInRadius(Vector2Int point, int radius, MapCell_Script [] result)
    {
        if (radius < 1)
            radius = 1;

        int count = 0;
        RectInt rect = new RectInt(point, Vector2Int.zero);

        for (int r = 0; r < radius; r++)
        {
            rect.min -= Vector2Int.one;
            rect.max += Vector2Int.one;
            count = FindAllowedCellsToGroundObjectAroundRectEx(rect, result, radius, count, true, true);
        }

        return count;
    }
    
    public int FindAllowedCelllsToGroundObjectAroundPointInRadius(Vector3 point, int radius, MapCell_Script[] result)
    {
        return FindAllowedCelllsToGroundObjectAroundPointInRadius(WorldToMapCoord(point), radius, result);
    }

    public bool FindPath(Vector2Int start, Vector2Int end, Vector2Int[] path, out int out_length)
    {
        start = ValidateMapCoords(start);
        end = ValidateMapCoords(end);
        out_length = 0;
        
        if (start == end)
            return true;

        // Zero path map
        for (int i=0; i<fHeight; i++)
        {
            for (int j = 0; j < fWidth; j++)
            {
                fPathMap[i, j].cost = PATH_MAP_EMPTY_CELL;
                fPathMap[i, j].sum_dist = -1;
                fPathMap[i, j].dist_to_start = -1;
                fPathMap[i, j].write_cnt = 0;
            }
        }

        // Wave propagation

        Vector2Int pos = start;
        Vector2Int pos2;
        Vector2Int nearest_point = start;
        float nearest_point_dist = Vector3.Distance(MapCoordToWorld(start),
                                                    MapCoordToWorld(end));
        float f_buffer = 0f;
        bool target_achieved = false;

        fOpen_cells.Clear();
        fPathMap[start.y, start.x].cost = PATH_MAP_START_CELL;
        fPathMap[end.y, end.x].cost = PATH_MAP_END_CELL;
        fOpen_cells.Add(pos);

        while (true)
        {
            int c = fOpen_cells.Count;
            int p = 0;

            // Enum open cells
            for (int i=0; i<c; i++)
            {
                pos = fOpen_cells[i];
                p++;

                // Enum neighbourhood cells
                for (int k=0; k<8; k++)
                {
                    pos2 = pos + fNeigh_offsets[k];

                    if (!IsValidMapCoords(pos2))
                        continue;

                    switch (fPathMap[pos2.y, pos2.x].cost)
                    {
                        case PATH_MAP_EMPTY_CELL:
                            MapCell_Script cell = GetMapCell(pos2);

                            if (cell.IsPassable() && 
                                !cell.HasGroundObjects())
                            {
                                int pcost = fPathMap[pos.y, pos.x].cost;

                                if (pcost == PATH_MAP_START_CELL)
                                {
                                    pcost = GetMapCell(pos).GetPathCost();
                                }
                                
                                fPathMap[pos2.y, pos2.x].cost = pcost + cell.GetPathCost();
                                fOpen_cells.Add(pos2);
                                f_buffer = Vector3.Distance(MapCoordToWorld(end), cell.WordlPosition);

                                if (f_buffer < nearest_point_dist)
                                {
                                    nearest_point_dist = f_buffer;
                                    nearest_point = pos2;
                                }
                            }
                            else
                                fPathMap[pos2.y, pos2.x].cost = PATH_MAP_BUSY_CELL;

                            break;
                        case PATH_MAP_START_CELL:
                            break;
                        case PATH_MAP_END_CELL:
                            target_achieved = true;
                            break;
                        case PATH_MAP_BUSY_CELL:
                            break;
                    }

                    if (target_achieved)
                        break;
                }

                if (target_achieved)
                    break;
            }

            fOpen_cells.RemoveRange(0, p);

            if (target_achieved)
                break;

            if (fOpen_cells.Count == 0)
            {
                break;
            }
        }

//        MapCell_Script ncell = GetMapCell(nearest_point);
        Vector2Int buffer_end = end;
        
        if ((!target_achieved))
        {
            if (start == nearest_point)
                return true;
            else
                end = nearest_point;
        }

        pos = end;
        // Path calculation
        Vector2Int best = pos;
        int best_val = System.Int32.MaxValue;
        float best_dist = System.Single.MaxValue;
        bool path_found = false;
        int it_count = 0;

        if (path != null)
        {
            path[out_length] = end;
            out_length++;
        }

        while (true)
        {
            if (it_count >= (fWidth * fHeight) * 8)
            {
                Debug.LogError("Alllowable number of iterations is exhausted!");
                return false;
            }

            for (int k=0; k<8; k++)
            {
                it_count++;
                pos2 = pos + fNeigh_offsets[k];

                if (!IsValidMapCoords(pos2))
                    continue;

                int path_val = fPathMap[pos2.y, pos2.x].cost;
                float dist = Vector2Int.Distance(start, pos2);

                switch (path_val)
                {
                    case PATH_MAP_EMPTY_CELL:
                        break;
                    case PATH_MAP_BUSY_CELL:
                        break;
                    case PATH_MAP_START_CELL:
                        path_found = true;
                        break;
                    case PATH_MAP_END_CELL:
                        break;

                    default:
                        if ((path_val < best_val) || (path_val <= best_val) && (dist < best_dist))
                        {
                            best_val = path_val;
                            best_dist = dist;
                            best = pos2;
                        }

                        break;
                }
            }

            if (path_found)
            {
                return buffer_end == end;
            }
            else
            {
                if (path != null)
                {
                    path[out_length] = best;
                    out_length++;
                }
                pos = best;
            }
        }
    }
    
    public bool TargetPointReachable(Vector2Int start, Vector2Int target)
    {
        int path_len;
        return FindPath(start, target, null, out path_len);
    }

    public bool TargetPointReachable(Vector3 start, Vector3 target)
    {
        return TargetPointReachable(WorldToMapCoord(start), WorldToMapCoord(target));
    }

    public MapCell_Script MatchCellAroundRect(RectInt rect, MatchCell match_method, object arg)
    {
        MapCell_Script result;
        Vector2Int pos = Vector2Int.zero;

        if (rect.size == Vector2Int.zero)
        {
            if (IsValidMapCoords(rect.position))
            {
                result = GetMapCell(rect.position);

                if (match_method(result, arg))
                    return result;
            }

        }

        for (int i=rect.xMin-1; i<=rect.xMax + 1; i++)
        {
            pos.x = i;
            pos.y = rect.yMin - 1;

            if (IsValidMapCoords(pos))
            {
                result = GetMapCell(pos);

                if (match_method(result, arg))
                {
                    return result;
                }
            }

            pos.x = i;
            pos.y = rect.yMax + 1;

            if (IsValidMapCoords(pos))
            {
                result = GetMapCell(pos);

                if (match_method(result, arg))
                {
                    return result;
                }
            }
        }

        for (int i = rect.yMin; i <= rect.yMax; i++)
        {
            pos.x = rect.xMin - 1;
            pos.y = i;

            if (IsValidMapCoords(pos))
            {
                result = GetMapCell(pos);

                if (match_method(result, arg))
                {
                    return result;
                }
            }

            pos.x = rect.xMax + 1;
            pos.y = i;

            if (IsValidMapCoords(pos))
            {
                result = GetMapCell(pos);

                if (match_method(result, arg))
                {
                    return result;
                }
            }
        }

        return null;
    }
    
    public MapCell_Script MatchCellAt(Vector2Int at, MatchCell match_method, object arg)
    {
        MapCell_Script result = null;
        RectInt rect = new RectInt(at, Vector2Int.zero);
        RectInt mbounds = Bounds;

        do {
            if (MathKit.RectIntContainsRect(rect, mbounds))
                return null;

            result = MatchCellAroundRect(rect, match_method, arg);
            rect.min -= Vector2Int.one;
            rect.max += Vector2Int.one;
        } while (result == null);

        return result;
    }

    public MapCell_Script MatchCellAt(Vector3 at, MatchCell match_method, object arg)
    {
        return MatchCellAt(WorldToMapCoord(at), match_method, arg);
    }

    protected bool DistributeSpace_MatchMethod(MapCell_Script cell, object arg)
    {
        fDistributingSpice -= cell.ReturnSpice(fDistributingSpice);
        return fDistributingSpice == 0;
    }

    public void DistributeSpice(Vector2Int start, float spice_value)
    {
        fDistributingSpice = spice_value;
        MatchCellAt(start, DistributeSpace_MatchMethod, null);
    }

    public void DistributeSpice(Vector3 start, float spice_value)
    {
        DistributeSpice(WorldToMapCoord(start), spice_value);
    }

    public void ClearEditorAllCellStates()
    {
        for (int i=0; i<fHeight; i++)
        {
            for (int j = 0; j < fWidth; j++)
                fCells[i][j].ClearEditorStates();
        }
    }

    public void ClearEditorCellStatesAtKey(string key)
    {
        for (int i = 0; i < fHeight; i++)
        {
            for (int j = 0; j < fWidth; j++)
                fCells[i][j].SetEditorState(key, false);
        }
    }
    
    public Vector2Int GetMapSize()
    {
        return new Vector2Int(fWidth, fHeight);
    }

    public bool IsCellPassable(int x, int y)
    {
        if (!IsValidMapCoords(x, y))
        {
            Debug.LogError("Unable get cell state 'passable'! Reason: invalid map coordinates (" + x + ", " + y + ")");
            return false;
        }

        return fCells[y][x].IsPassable();
    }

    public bool IsCellPassable(Vector2Int coords)
    {
        return IsCellPassable(coords.x, coords.y);
    }

    public bool IsCellPassable(Vector3 pos)
    {
        return IsCellPassable(WorldToMapCoord(pos));
    }
    
    public void Resize(int new_width, int new_height)
    {
        int w_delta = new_width - fWidth;
        int h_delta = new_height - fHeight;

        for (int i=Mathf.Abs(h_delta); i>=0; i--)
        {
            if (h_delta > 0)
                AddRow();
            else
                DeleteRow(fHeight);
        }

        for (int i=Mathf.Abs(w_delta); i>=0; i--)
        {
            if (w_delta > 0)
                AddColumn();
            else
                DeleteColumn(fWidth);
        }
        
        fPathMap = new Map_Script_Path_Cell[fHeight, fWidth];
        fOpen_cells.Capacity = fHeight * fWidth;
        Body.position = new Vector3(0, fHeight * GlobalCollector.Cell_Size, 0);
        OnResize.Invoke(this, fWidth, fHeight);
    }

    public bool UseFogOfWar
    {
        get
        {
            return true;
        }
    }

    public bool UseUnexploredArea
    {
        get
        {
            return true;
        }
    }



    void AddRow()
    {
        List<MapCell_Script> row = new List<MapCell_Script>();

        for (int i=0; i<fWidth; i++)
        {
            MapCell_Script cell;

            if (fIsEditorMode)
                cell = MapCell_Script.Instantiate(Default_Editor_Cell_Sample);
            else
                cell = MapCell_Script.Instantiate(Default_Game_Cell_Sample);

            cell.Unit.SetActive(true);
            cell.Body.parent = Body;
            cell.SetOwner(this);
            cell.SetMapCoord(i, fHeight);
            cell.SetBordered(fUseGrid);
            cell.Collider.size = new Vector2(GlobalCollector.Cell_Size, GlobalCollector.Cell_Size);
            row.Add(cell);
        }

        fCells.Add(row);
        fHeight++;
    }

    void DeleteRow(int from)
    {
        if (IsEmptyMap())
        {
            Debug.LogError("Cannot remove row ("+from+")! Reason: Map is empty.");
            return;
        }

        if (from < 0)
            from = 0;

        if (from >= fHeight)
            from = fHeight - 1;
        
        for (int i=fWidth-1; i>=0 ; i--)
        {
            MapCell_Script.Destroy(fCells[from][i]);
        }

        fCells.RemoveAt(from);
        fHeight--;
    }

    void AddColumn()
    {
        for (int i=0; i<fHeight; i++)
        {
            MapCell_Script cell;

            if (fIsEditorMode)
                cell = MapCell_Script.Instantiate(Default_Editor_Cell_Sample);
            else
                cell = MapCell_Script.Instantiate(Default_Game_Cell_Sample);

            cell.Unit.SetActive(true);
            cell.Body.parent = Body;
            cell.SetOwner(this);
            cell.SetMapCoord(fWidth, i);
            cell.SetBordered(fUseGrid);
            cell.Collider.size = new Vector2(GlobalCollector.Cell_Size, GlobalCollector.Cell_Size);
            fCells[i].Add(cell);
        }

        fWidth++;
    }

    void DeleteColumn(int from)
    {
        if (IsEmptyMap())
        {
            Debug.LogError("Cannot remove column ("+from+")! Reason: map is empty");
            return;
        }

        if (from < 0)
            from = 0;

        if (from >= fWidth)
            from = fWidth - 1;

        for (int i=fHeight-1; i>=0; i--)
        {
            MapCell_Script.Destroy(fCells[i][from]);
            fCells[i].RemoveAt(from);
        }

        fWidth--;
    }

    void Clear()
    {
        for (int i=fHeight-1; i>=0; i--)
        {
            DeleteRow(i);
        }

        fHeight = 0;
        fWidth = 0;
    }

	// Use this for initialization
	void Start () {
        fNeigh_offsets[0] = new Vector2Int(-1, 1);
        fNeigh_offsets[1] = new Vector2Int(0, 1);
        fNeigh_offsets[2] = new Vector2Int(1, 1);
        fNeigh_offsets[3] = new Vector2Int(1, 0);
        fNeigh_offsets[4] = new Vector2Int(1, -1);
        fNeigh_offsets[5] = new Vector2Int(0, -1);
        fNeigh_offsets[6] = new Vector2Int(-1, -1);
        fNeigh_offsets[7] = new Vector2Int(-1, 0);
	}
	
	// Update is called once per frame
	void Update () {	       
        if (fInit)
        {
            fInit = false;
            SetTerrainTextureSize(GlobalCollector.Cell_Size, GlobalCollector.Cell_Size);
            SetEditorMode(Default_IsEditorMode);
            SetUseGrid(Default_UseGrid);
            Resize(Default_MapWidth, Default_MapHeight);
        }

        for (int i=0; i<fHeight; i++)
        {
            for (int j = 0; j < fWidth; j++)
                fCells[i][j].Cell_Update();
        }
	}
}
