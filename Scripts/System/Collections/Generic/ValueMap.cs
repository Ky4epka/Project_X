using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic.ValueMap
{
    public interface IValueMap<T> : IDisposable, IAssignable
    {
        void SetColumnCount(int col_count);
        void SetRowCount(int row_count);
        void SetSize(int col_count, int row_count);
        void InsertNewColumn(int to);
        void InsertNewRow(int to);
        void MoveColumn(int from, int to);
        void MoveRow(int from, int to);
        void AddNewColumnFirst();
        void AddNewRowFirst();
        void AddNewColumnLast();
        void AddNewRowLast();
        void DeleteColumn(int from);
        void DeleteRow(int from);
        void Clear();
        void CopyMapTo(int to_column, int to_row, IValueMap<T> from, int from_column, int from_row, int from_col_count, int from_row_count);
        void CopyColumnTo(int to_column, int to_column_start_row, IValueMap<T> from, int from_column, int from_column_start_row, int from_column_count_rows);
        void CopyRowTo(int to_row, int to_row_start_column, IValueMap<T> from, int from_row, int from_row_start_column, int from_row_count_columns);

        int GetColumnCount();
        int GetRowCount();

        bool IsValidColumn(int column);
        bool IsValidRow(int row);
        bool IsValidCoords(int column, int row);
        bool CheckColumn(string message, int column);
        bool CheckRow(string message, int row);
        
        T GetCell(int column, int row);
        T AllocCell();
        void ReleaseCell(T cell);
        void CellPositionChanged(T cell, int x, int y);
        void InvalidateCellPositions();
        void AssignCell(T source, T destination);
    }

    public abstract class ValueMapBase<T> : IValueMap<T>
    {
        public delegate void ForEachAction(T cell, int x, int y);

        protected T[,] fCells = null;
        protected Vector2Int fSize = Vector2Int.zero;

        public ValueMapBase()
        {
        }

        public ValueMapBase(ValueMapBase<T> source)
        {
            Assign(source);
        }

        public ValueMapBase(int col_count, int row_count)
        {
            SetSize(col_count, row_count);
        }

        public ValueMapBase(Vector2Int m_coord)
        {
            SetSize(m_coord.x, m_coord.y);
        }

        public void Assign(IAssignable source)
        {
            UnityEngine.Assertions.Assert.IsNotNull(source);
            UnityEngine.Assertions.Assert.IsFalse(source is ValueMapBase<T>);
            ValueMapBase<T> sobj = source as ValueMapBase<T>;
            SetSize(sobj.GetColumnCount(), sobj.GetRowCount());
            CopyMapTo(0, 0, sobj, 0, 0, sobj.GetColumnCount(), sobj.GetRowCount());
        }

        public void Dispose()
        {
            Clear();
        }


        public void SetColumnCount(int col_count)
        {
            if (col_count == GetColumnCount())
                return;

            if (col_count < 0)
            {
                Debug.LogError("Column count can not be a negative value");
                return;
            }

            int col_delta = col_count - fSize.x;
            int col_count_old = GetColumnCount();
            fSize.x = col_count;

            if (col_delta >= 0)
            {
                T[,] new_array = new T[col_count, GetRowCount()];
                Copy2DArray(col_count_old, GetRowCount(), fCells, new_array);
                fCells = new_array;
                new_array = null;

                for (int i = col_count_old; i < col_count; i++)
                {
                    AllocColumn(i);
                }
            }
            else
            {
                for (int i = col_count_old - 1; i >= col_count; i--)
                {
                    ReleaseColumn(i);
                }
                
                T[,] new_array = new T[col_count, GetRowCount()];
                Copy2DArray(col_count, GetRowCount(), fCells, new_array);
                fCells = new_array;
                new_array = null;
            }
        }

        public void SetRowCount(int row_count)
        {
            if (row_count == GetRowCount())
                return;

            if (row_count < 0)
            {
                Debug.LogError("Row count can not be a negative value");
                return;
            }

            int row_delta = row_count - GetRowCount();
            int row_count_old = GetRowCount();
            fSize.y = row_count;

            if (row_delta >= 0)
            {
                T[,] new_array = new T[GetColumnCount(), row_count];
                Copy2DArray(GetColumnCount(), row_count_old, fCells, new_array);
                fCells = new_array;
                new_array = null;

                for (int i = row_count_old; i < row_count; i++)
                {
                    AllocRow(i);
                }
            }
            else
            {
                for (int i = row_count_old - 1; i >= row_count; i--)
                {
                    ReleaseRow(i);
                }

                T[,] new_array = new T[GetColumnCount(), row_count];
                Copy2DArray(GetColumnCount(), row_count, fCells, new_array);
                fCells = new_array;
                new_array = null;
            }
        }

        public void SetSize(int col_count, int row_count)
        {
            SetColumnCount(col_count);
            SetRowCount(row_count);
        }

        public void SetSize(Vector2Int size)
        {
            SetSize(size.x, size.y);
        }

        public void InsertNewColumn(int to)
        {
            if ((to != -1) &&
                (to != GetColumnCount()) &&
                !CheckColumn("Could not insert new column. Reason: ", to))
            {
                return;
            }
            
            int col_cache = GetColumnCount();

            if (to == -1)
                to = col_cache;

            SetColumnCount(col_cache + 1);
            MoveColumn(col_cache, to);
        }

        public void InsertNewRow(int to)
        {
            if ((to != -1) &&
                (to != GetRowCount()) &&
                !CheckRow("Could not insert new row. Reason: ", to))
            {
                return;
            }

            int row_cache = GetRowCount();

            if (to == -1)
                to = row_cache;

            SetRowCount(row_cache + 1);
            MoveRow(row_cache, to);
        }
        
        public void MoveColumn(int from, int to)
        {
            if (!CheckColumn("Could not move column. Reason: 'from' ", from) ||
                (!CheckColumn("Could not move column. Reason: 'to' ", to) &&
                 (to != -1)) ||
                (from == to))
            {
                return;
            }

            if (to == -1)
                to = GetColumnCount() - 1;

            T cache;

            int step_direction = 1;

            if (to < from)
            {
                step_direction = -1;
            }

            for (int i = GetRowCount() - 1; i >= 0; i--)
            {
                cache = fCells[from, i];

                for (int j = from; j != to; j+=step_direction)
                {
                    fCells[j, i] = fCells[j + step_direction, i];
                    CellPositionChanged(fCells[j, i], j, i);
                }

                fCells[to, i] = cache;
                CellPositionChanged(fCells[to, i], to, i);
            }
        }

        public void MoveRow(int from, int to)
        {
            if (!CheckRow("Could not move row. Reason: 'from' ", from) ||
                (!CheckRow("Could not move row. Reason: 'to' ", to) &&
                 (to != -1)) ||
                (from == to))
            {
                return;
            }

            if (to == -1)
                to = GetRowCount() - 1;

            T cache;

            int step_direction = 1;

            if (to < from)
            {
                step_direction = -1;
            }

            for (int i = GetColumnCount() - 1; i >= 0; i--)
            {
                cache = fCells[i, from];

                for (int j = from; j != to; j+=step_direction)
                {
                    fCells[i, j] = fCells[i, j + step_direction];
                    CellPositionChanged(fCells[i, j], i, j);
                }

                fCells[i, to] = cache;
                CellPositionChanged(fCells[i, to], i, to);
            }
        }

        public void AddNewColumnFirst()
        {
            InsertNewColumn(0);
        }

        public void AddNewRowFirst()
        {
            InsertNewRow(0);
        }

        public void AddNewColumnLast()
        {
            InsertNewColumn(GetColumnCount());
        }

        public void AddNewRowLast()
        {
            InsertNewRow(GetRowCount());
        }

        public void DeleteColumn(int from)
        {
            if (!CheckColumn("Could not delete column. Reason: ", from))
            {
                return;
            }
            
            MoveColumn(from, GetColumnCount() - 1);
            SetColumnCount(GetColumnCount() - 1);
        }

        public void DeleteRow(int from)
        {
            if (!CheckRow("Could not delete row. Reason: ", from))
            {
                return;
            }
            
            MoveRow(from, GetRowCount() - 1);
            SetRowCount(GetRowCount() - 1);
        }

        public void Clear()
        {
            SetSize(0, 0);
        }

        public void CopyMapTo(int to_column, int to_row, IValueMap<T> from, int from_column, int from_row, int from_col_count, int from_row_count)
        {
            int int_cache = to_row + from_row_count;

            if (int_cache > GetRowCount())
            {
                from_row_count = GetRowCount() - int_cache;
            }

            int_cache = from_row + from_row_count;

            if (int_cache > from.GetRowCount())
            {
                from_row_count = from.GetRowCount() - int_cache;
            }

            for (int i = 0; i < from_row_count; i++)
            {
                CopyRowTo(i + to_row, to_column, from, i + from_row, from_column, from_col_count);
            }
        }

        public void CopyMapTo(Vector2Int to_m_coord, IValueMap<T> from, Vector2Int from_m_coord, Vector2Int from_size)
        {
            CopyMapTo(to_m_coord.x, to_m_coord.y, from, from_m_coord.x, from_m_coord.y, from_size.x, from_size.y);
        }

        public void CopyColumnTo(int to_column, int to_column_start_row, IValueMap<T> from, int from_column, int from_column_start_row, int from_column_count_rows)
        {
            if (!CheckColumn("Unable copy column. Reason: 'to_column'", to_column) ||
                !CheckRow("Unable copy column. Reason: 'to_column_start_row'", to_column_start_row) ||
                !from.CheckColumn("Unable copy column. Reason: 'from_column'", from_column) ||
                !from.CheckRow("Unable copy column. Reason: 'from_column_start_row'", from_column_start_row))
            {
                return;
            }

            int int_cache = to_column_start_row + from_column_count_rows;

            if (int_cache > GetRowCount())
            {
                from_column_count_rows = GetRowCount() - int_cache;
            }

            int_cache = from_column_start_row + from_column_count_rows;

            if (int_cache > from.GetRowCount())
            {
                from_column_count_rows = from.GetRowCount() - int_cache;
            }

            for (int i = 0; i < from_column_count_rows; i++)
            {
                AssignCell(from.GetCell(from_column, i + from_column_start_row),
                           fCells[to_column, i + to_column_start_row]);
            }
        }

        public void CopyRowTo(int to_row, int to_row_start_column, IValueMap<T> from, int from_row, int from_row_start_column, int from_row_count_columns)
        {
            if (!CheckRow("Unable copy row. Reason: 'to_row'", to_row) ||
                !CheckColumn("Unable copy row. Reason: 'to_row_start_column'", to_row_start_column) ||
                !from.CheckRow("Unable copy row. Reason: 'from_row'", from_row) ||
                !from.CheckColumn("Unable copy row. Reason: 'from_row_start_column'", from_row_start_column))
            {
                return;
            }

            int int_cache = to_row_start_column + from_row_count_columns;

            if (int_cache > GetRowCount())
            {
                from_row_count_columns = GetRowCount() - int_cache;
            }

            int_cache = from_row_start_column + from_row_count_columns;

            if (int_cache > from.GetRowCount())
            {
                from_row_count_columns = from.GetRowCount() - int_cache;
            }

            for (int i = 0; i < from_row_count_columns; i++)
            {
                AssignCell(from.GetCell(i + from_row_start_column, from_row),
                           fCells[i + to_row_start_column, to_row]);
            }
        }
               
        public int GetColumnCount()
        {
            return fSize.x;
        }

        public int GetRowCount()
        {
            return fSize.y;
        }

        public bool IsValidColumn(int column)
        {
            return (column >= 0) && (column < fSize.x);
        }

        public bool IsValidRow(int row)
        {
            return (row >= 0) && (row < fSize.y);
        }

        public bool IsValidCoords(int column, int row)
        {
            return IsValidColumn(column) && IsValidRow(row);
        }

        public bool IsValidCoords(Vector2Int m_coords)
        {
            return IsValidCoords(m_coords.x, m_coords.y);
        }

        public bool CheckColumn(string message, int column)
        {
            bool check_res = IsValidColumn(column);

            if (!check_res)
            {
                Debug.LogError(string.Concat(message, "column index (", column, ") is out of range (", 0, ", ", GetColumnCount(), ")."));
            }

            return check_res;
        }

        public bool CheckRow(string message, int row)
        {
            bool check_res = IsValidRow(row);

            if (!check_res)
            {
                Debug.LogError(string.Concat(message, "row index (", row, ") is out of range (", 0, ", ", GetRowCount(), ")."));
            }

            return check_res;
        }

        public T GetCell(int column, int row)
        {
            return fCells[column, row];
        }

        public void ForEachCell(ForEachAction action)
        {
            for (int i=0; i<GetRowCount(); i++)
            {
                for (int j=0; j<GetColumnCount(); j++)
                    action(GetCell(j, i), j, i);
            }
        }

        protected void AllocColumn(int to)
        {
            for (int i=GetRowCount()-1; i>=0; i--)
            {
                fCells[to, i] = AllocCell();
                CellPositionChanged(fCells[to, i], to, i);
            }
        }

        protected void AllocRow(int to)
        {
            for (int i=GetColumnCount()-1; i>=0; i--)
            {
                fCells[i, to] = AllocCell();
                CellPositionChanged(fCells[i, to], i, to);
            }
        }

        protected void ReleaseColumn(int from)
        {
            for (int i = GetRowCount() - 1; i >= 0; i--)
            {
                ReleaseCell(fCells[from, i]);
            }
        }

        protected void ReleaseRow(int from)
        {
            for (int i = GetColumnCount() - 1; i >= 0; i--)
            {
                ReleaseCell(fCells[i, from]);
            }
        }

        public abstract T AllocCell();
        public abstract void ReleaseCell(T cell);
        public abstract void CellPositionChanged(T cell, int x, int y);
        public abstract void AssignCell(T source, T destination);

        public void InvalidateCellPositions()
        {
            for (int i=GetRowCount()-1; i>=0; i--)
            {
                for (int k=GetColumnCount()-1; k>=0; k--)
                {
                    CellPositionChanged(GetCell(k, i), k, i);
                }
            }
        }
               
        protected static void Copy2DArray(int col_count, int row_count, T[,] source, T[,] destination)
        {
            if (source == null)
                return;

            for (int i = row_count - 1; i >= 0; i--)
            {
                for (int j = col_count - 1; j >= 0; j--)
                {
                    destination[j, i] = source[j, i];
                }
            }
        }
    }
}