using UnityEngine;
using System.Collections;
using System.Collections.Generic.ValueMap;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;

namespace GridValueMap
{
    [System.Serializable]
    public abstract class GriddedValueMap<T>: ValueMapBase<T>
    {
        protected GridValueMap<T> fOwner = null;

        public GridValueMap<T> Owner
        {
            get
            {
                return fOwner;
            }

            set
            {
                if (fOwner == value)
                    return;

                fOwner = value;

                if (fOwner != null)
                    InvalidateCellPositions();
                else
                    Dispose();
            }
        }
    }

    public enum GridOrientation
    {
        LeftToRightBottomToTop = 0,
        LeftToRightTopToBottom,
        RightToLeftBottomToTop,
        RightToLeftTopToBottom
    }

    [System.Serializable]
    public class GridValueMap<T> : MonoBehaviour, IValueMap<T>
    {
        public Vector3 Initial_cellGap = Vector3.zero;
        public Vector3 Initial_cellSize = Vector3.zero;
        public Vector2Int Initial_mapSize = Vector2Int.zero;
        public GridOrientation Initial_GridOrientation = GridOrientation.LeftToRightBottomToTop;

        protected GriddedValueMap<T> fValueMap = null;
        protected Vector3 fcellGap = Vector3.zero;
        protected Vector3 fcellSize = Vector3.zero;
        protected GridOrientation fGridOrientation = GridOrientation.LeftToRightBottomToTop;
        protected Vector2Int fGridOrientationValue = new Vector2Int(1, 1);
        protected bool fLockUpdate = false;
        protected Transform fcachedTransform = null;

        protected int[,] fGridOrientations = new int[4, 2] {
                                                              { 1, 1 },
                                                              { 1, -1 },
                                                              { -1, 1 },
                                                              { -1, -1 }
                                                            };

        public Vector3 cellGap
        {
            get
            {
                return fcellGap;
            }

            set
            {
                fcellGap = value;
                InvalidateCellPositions();
            }
        }

        public Vector3 cellSize
        {
            get
            {
                return fcellSize;
            }

            set
            {
                fcellSize = value;
                InvalidateCellPositions();
            }
        }

        public GriddedValueMap<T> AttachedValueMap
        {
            get
            {
                return fValueMap;
            }

            set
            {
                if (fValueMap == value)
                    return;

                if (fValueMap != null)
                    fValueMap.Owner = null;

                fValueMap = value;

                if (fValueMap != null)
                    fValueMap.Owner = this;
            }
        }

        public GridOrientation GridOrientation
        {
            get
            {
                return fGridOrientation;
            }

            set
            {
                fGridOrientation = value;
                fGridOrientationValue.x = fGridOrientations[(int)fGridOrientation, 0];
                fGridOrientationValue.y = fGridOrientations[(int)fGridOrientation, 1];
                InvalidateCellPositions();
            }
        }

        public Vector2Int WorldToCell(float x, float y, float z)
        {
            Vector3 world = transform.position;
            world.x -= x * fGridOrientationValue.x;
            world.y -= y * fGridOrientationValue.y;
            world.x /= cellSize.x + cellGap.x;
            world.y /= cellSize.y + cellGap.y;
            Vector2Int cell = new Vector2Int((int)world.x, (int)world.y);
            return cell;
        }

        public Vector2Int WorldToCell(Vector3 world)
        {
            return WorldToCell(world.x, world.y, world.z);
        }

        public Vector3 CellToWorld(int x, int y)
        {
            Vector3 world = transform.position;
            world.x += (float)(x * fGridOrientationValue.x) * (fcellSize.x + fcellGap.x);
            world.y += (float)(y * fGridOrientationValue.y) * (fcellSize.y + fcellGap.y);
            return world;
        }
        
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return CellToWorld(cell.x, cell.y);
        }

        public Vector3 GetWorldCellCenter(int x, int y)
        {
            Vector3 world = CellToWorld(x, y);
            world.x += fcellGap.x + fcellSize.x / 2f;
            world.y += fcellGap.y + fcellSize.y / 2f;
            return world;
        }

        public Vector3 GetWorldCellCenter(Vector2Int cell)
        {
            return GetWorldCellCenter(cell.x, cell.y);
        }

        public void BeginUpdate()
        {
            fLockUpdate = true;
        }

        public void EndUpdate()
        {
            fLockUpdate = false;
            InvalidateCellPositions();
        }

        public void Assign(IAssignable source)
        {
            Assert.IsNotNull(source);
            Assert.IsFalse(source is GridValueMap<T>);
            GridValueMap<T> sobj = source as GridValueMap<T>;

            BeginUpdate();
                AttachedValueMap = sobj.AttachedValueMap;
                cellGap = sobj.cellGap;
                cellSize = sobj.cellSize;
                GridOrientation = sobj.GridOrientation;
            EndUpdate();
        }

        public void Dispose()
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.Dispose();
        }

        public void SetColumnCount(int col_count)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.SetColumnCount(col_count);
        }

        public void SetRowCount(int row_count)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.SetRowCount(row_count);
        }

        public void SetSize(int col_count, int row_count)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.SetSize(col_count, row_count);
        }

        public void InsertNewColumn(int to)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.InsertNewColumn(to);
        }

        public void InsertNewRow(int to)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.InsertNewRow(to);
        }

        public void MoveColumn(int from, int to)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.MoveColumn(from, to);
        }

        public void MoveRow(int from, int to)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.MoveRow(from, to);
        }

        public void AddNewColumnFirst()
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.AddNewColumnFirst();
        }

        public void AddNewRowFirst()
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.AddNewRowFirst();
        }

        public void AddNewColumnLast()
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.AddNewColumnLast();
        }

        public void AddNewRowLast()
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.AddNewRowLast();
        }

        public void DeleteColumn(int from)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.DeleteColumn(from);
        }

        public void DeleteRow(int from)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.DeleteRow(from);
        }

        public void Clear()
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.Clear();
        }

        public void CopyMapTo(int to_column, int to_row, IValueMap<T> from, int from_column, int from_row, int from_col_count, int from_row_count)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.CopyMapTo(to_column, to_row, from, from_column, from_row, from_col_count, from_row_count);
        }

        public void CopyColumnTo(int to_column, int to_column_start_row, IValueMap<T> from, int from_column, int from_column_start_row, int from_column_count_rows)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.CopyColumnTo(to_column, to_column_start_row, from, from_column, from_column_start_row, from_column_count_rows);
        }

        public void CopyRowTo(int to_row, int to_row_start_column, IValueMap<T> from, int from_row, int from_row_start_column, int from_row_count_columns)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.CopyRowTo(to_row, to_row_start_column, from, from_row, from_row_start_column, from_row_count_columns);
        }

        public int GetColumnCount()
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.GetColumnCount();
        }

        public int GetRowCount()
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.GetRowCount();
        }

        public bool IsValidColumn(int column)
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.IsValidColumn(column);
        }

        public bool IsValidRow(int row)
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.IsValidRow(row);
        }

        public bool IsValidCoords(int column, int row)
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.IsValidCoords(column, row);
        }

        public bool IsValidCoords(Vector2Int m_coords)
        {
            return IsValidCoords(m_coords.x, m_coords.y);
        }

        public bool CheckColumn(string message, int column)
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.CheckColumn(message, column);
        }

        public bool CheckRow(string message, int row)
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.CheckRow(message, row);
        }


        public T GetCell(int column, int row)
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.GetCell(column, row);
        }

        public T GetCell(Vector2Int m_coord)
        {
            return GetCell(m_coord.x, m_coord.y);
        }

        public T AllocCell()
        {
            Assert.IsNotNull(fValueMap);
            return fValueMap.AllocCell();
        }

        public void ReleaseCell(T cell)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.ReleaseCell(cell);
        }

        public void CellPositionChanged(T cell, int x, int y)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.CellPositionChanged(cell, x, y);
        }

        public void AssignCell(T source, T destination)
        {
            Assert.IsNotNull(fValueMap);
            fValueMap.AssignCell(source, destination);
        }

        public void InvalidateCellPositions()
        {
            if (fLockUpdate)
                return;

            Assert.IsNotNull(fValueMap);
            fValueMap.InvalidateCellPositions();
        }
        
        public new Transform transform
        {
            get
            {
                if (fcachedTransform == null)
                    fcachedTransform = base.transform;

                return fcachedTransform;
            }
        }

        protected virtual void Start()
        {
            cellGap = Initial_cellGap;
            cellSize = Initial_cellSize;
            GridOrientation = Initial_GridOrientation;
            SetSize(Initial_mapSize.x, Initial_mapSize.y);
        }
    }
}