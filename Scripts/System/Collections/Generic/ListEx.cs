using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Collections.Generic
{

    public class ListEx<T> : List<T>
    {
        public delegate void ListExItemChangeNotify(ListEx<T> sender, T item, int index);
        public delegate void ListExItemRelocateNotify(ListEx<T> sender, T item, int from, int to);

        public ListExItemChangeNotify OnAddItem = null;
        public ListExItemChangeNotify OnRemoveItem = null;
        public ListExItemRelocateNotify OnRelocateItem = null;

        public Predicate<T> AddItemPredicate = null;

        protected bool fUniqueItems = false;

        public ListEx() : base()
        {
        }

        public ListEx(IEnumerable<T> collection) : base(collection)
        {
        }

        public ListEx(int capacity) : base(capacity)
        {
        }

        public bool UniqueItems
        {
            get
            {
                return fUniqueItems;
            }

            set
            {
                fUniqueItems = value;
            }
        }

        protected new void Add(T item)
        {

        }

        protected bool AddItem(T item)
        {
            if (fUniqueItems && 
                (IndexOf(item) != -1))
            {
                return false;
            }

            if ((AddItemPredicate != null) &&
                 (!AddItemPredicate(item)))
            {
                return false;
            }

            base.Add(item);

            if (OnAddItem != null)
                OnAddItem(this, item, Count - 1);

            return true;
        }

        protected new void InsertRange(int index, IEnumerable<T> collection)
        {
            base.InsertRange(index, collection);
        }

        public void InsertFirst(T item)
        {
            InsertBefore(0, item);
        }

        public void InsertLast(T item)
        {
            InsertAfter(Count, item);
        }

        public void InsertBefore(int before, T item)
        {
            if (AddItem(item))
                MoveBefore(Count - 1, before);
        }

        public void InsertBefore(T before_value, T item)
        {
            InsertBefore(IndexOf(before_value), item);
        }

        public void InsertAfter(int after, T item)
        {
            if (AddItem(item))
                MoveAfter(Count - 1, after);
        }

        public void InsertAfter(T after_value, T item)
        {
            InsertAfter(IndexOf(after_value), item);
        }

        public void InsertRangeFirst(IEnumerable<T> collection)
        {
            InsertRange(0, collection);
        }

        public void InsertRangeLast(IEnumerable<T> collection)
        {
            InsertRange(Count, collection);
        }

        public void InsertRangeBefore(int before, IEnumerable<T> collection)
        {
            InsertRange(before, collection);
        }

        public void InsertRangeBefore(T before_value, IEnumerable<T> collection)
        {
            InsertRange(IndexOf(before_value), collection);
        }

        public void InsertRangeAfter(int after, IEnumerable<T> collection)
        {
            if (after < Count)
                after++;

            InsertRange(after, collection);
        }

        public void InsertRangeAfter(T after_value, IEnumerable<T> collection)
        {
            InsertRange(IndexOf(after_value), collection);
        }

        public bool Exchange(int from, int to)
        {
            if (!IsValidIndex(from))
                throw new System.IndexOutOfRangeException();

            if (!IsValidIndex(to))
                throw new System.IndexOutOfRangeException();

            T cache = this[to];
            this[to] = this[from];
            this[from] = cache;

            if (OnRelocateItem != null)
                OnRelocateItem(this, cache, from, to);

            return true;
        }

        public bool Exchange(T from_value, T to_value)
        {
            return Exchange(IndexOf(from_value), IndexOf(to_value));
        }

        protected void MoveToEx(int from, int to, int default_to, int direct_modificator, int reverse_modificator)
        {
            if (!IsValidIndex(from))
                throw new System.IndexOutOfRangeException();

            if ((to != -1) &&
                !IsValidIndex(to))
                throw new System.IndexOutOfRangeException();

            if (to == -1)
                to = default_to;

            if (from == to)
                return;

            int step_direction = 1;

            if (from > to)
            {
                step_direction = -1;
                to += direct_modificator;
            }
            else
                to += reverse_modificator;

            T cache = this[from];

            for (int i = from; i != to; i += step_direction)
            {
                this[i] = this[i + step_direction];

                if (OnRelocateItem != null)
                    OnRelocateItem(this, this[i], i + step_direction, i);
            }

            this[to] = cache;

            if (OnRelocateItem != null)
                OnRelocateItem(this, cache, from, to);
        }

        public void MoveBefore(int from, int before)
        {
            MoveToEx(from, before, 0, -1, 1);
        }

        public void MoveBefore(T from_value, T to_value)
        {
            MoveBefore(IndexOf(from_value), IndexOf(to_value));
        }

        public void MoveAfter(int from, int after)
        {
            MoveToEx(from, after, Count - 1, 1, -1);
        }

        public void MoveAfter(T from_value, T to_value)
        {
            MoveAfter(IndexOf(from_value), IndexOf(to_value));
        }

        public void MoveToFront(int from)
        {
            MoveBefore(from, -1);
        }

        public void MoveToFront(T from_value)
        {
            MoveToFront(IndexOf(from_value));
        }

        public void MoveToBack(int from)
        {
            MoveAfter(from, -1);
        }

        public void MoveToBack(T from_value)
        {
            MoveToBack(IndexOf(from_value));
        }

        public void MoveRangeBefore(int startIndex, int count, int destinationIndex)
        {
            if (!IsValidIndex(startIndex))
                throw new System.IndexOutOfRangeException();
            
            if (!IsValidIndex(destinationIndex))
                throw new System.IndexOutOfRangeException();

            if (count + destinationIndex > Count)
                throw new System.IndexOutOfRangeException();

            if (count <= 0)
                return;

            int startIndexStep = 0;

            if (startIndex > destinationIndex)
            {
                startIndexStep = 1;
            }

            while (count > 0)
            {
                MoveBefore(startIndex, destinationIndex);
                startIndex += startIndexStep;
                destinationIndex++;
                count--;
            }
        }

        public void MoveRangeAfter(int startIndex, int count, int destinationIndex)
        {
            if (!IsValidIndex(startIndex))
                throw new System.IndexOutOfRangeException();

            if (!IsValidIndex(destinationIndex))
                throw new System.IndexOutOfRangeException();

            if (count + destinationIndex > Count)
                throw new System.IndexOutOfRangeException();

            if (count <= 0)
                return;

            int startIndexStep = 0;

            if (startIndex > destinationIndex)
            {
                startIndexStep = 1;
            }

            if (startIndex < destinationIndex)
            {
                while (count > 0)
                {
                    MoveAfter(startIndex, destinationIndex);
                    count--;
                }
            }
            else
            {
                while (count > 0)
                {
                    MoveAfter(startIndex, destinationIndex);
                    startIndex += startIndexStep;
                    destinationIndex++;
                    count--;
                }
            }

            while (count > 0)
            {
                MoveAfter(startIndex, destinationIndex);
                startIndex += startIndexStep;
                destinationIndex++;
                count--;
            }
        }

        public int FindIndex(string class_list_property_name, object match_value)
        {
            return FindIndex(class_list_property_name, match_value, 0, Count, FindIndexPredicateCap);
        }
        
        public int FindIndex(string class_list_property_name, object match_value, int startIndex)
        {
            return FindIndex(class_list_property_name, match_value, startIndex, Count, FindIndexPredicateCap);
        }

        public int FindIndex(string class_list_property_name, object match_value, int startIndex, int count)
        {
            return FindIndex(class_list_property_name, match_value, startIndex, count, FindIndexPredicateCap);
        }

        public bool FindIndexPredicateCap(T compare_value)
        {
            return true;
        }

        public int FindIndex(string class_list_property_name, object match_value, int startIndex, int count, Predicate<T> match)
        {
            PropertyInfo pinfo = typeof(T).GetType().GetProperty(class_list_property_name);

            if (pinfo == null)
            {
                throw new System.Exception(string.Concat("Unknown class property name '", class_list_property_name, "'"));
            }

            if (!IsValidIndex(startIndex))
            {
                throw new System.IndexOutOfRangeException();
            }

            if (match == null)
                match = FindIndexPredicateCap;

            int ensured_count = startIndex + count;

            if (ensured_count > Count)
                ensured_count = Count;

            for (int i=startIndex; i<ensured_count; i++)
            {
                if (pinfo.GetValue(this[i], null).Equals(match_value) && 
                    (match(this[i])))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool IsValidIndex(int index)
        {
            return (index >= 0) &&
                   (index < Count);
        }

        public new void RemoveAt(int index)
        {
            T cache = this[index];
            base.RemoveAt(index);

            if (OnRemoveItem != null)
                OnRemoveItem(this, cache, index);
        }

        public new bool Remove(T item)
        {
            int index = IndexOf(item);

            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public new int RemoveAll(Predicate<T> match)
        {
            int count_removed = 0;

            for (int i=Count-1; i>=0; i--)
            {
                if (match(this[i]))
                {
                    RemoveAt(i);
                    count_removed++;
                }
            }

            return count_removed;
        }

        public new void RemoveRange(int index, int count)
        {
            if (!IsValidIndex(index))
                throw new System.IndexOutOfRangeException();

            int fx_count = index + count;

            if (fx_count > Count)
                fx_count = Count;

            for (int i = fx_count - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
        }

        public new void Reverse(int index, int count)
        {
            base.Reverse(index, count);

            ExecAllOnRelocateItem();
        }

        public new void Reverse()
        {
            base.Reverse();

            ExecAllOnRelocateItem();
        }

        public new void Sort(Comparison<T> comparison)
        {
            base.Sort(comparison);

            ExecAllOnRelocateItem();
        }

        public new void Sort(int index, int count, IComparer<T> comparer)
        {
            base.Sort(index, count, comparer);

            ExecAllOnRelocateItem();
        }

        public new void Sort()
        {
            base.Sort();

            ExecAllOnRelocateItem();
        }

        public new void Sort(IComparer<T> comparer)
        {
            base.Sort(comparer);

            ExecAllOnRelocateItem();
        }

        public new void Clear()
        {
            ExecAllOnRemoveItem();
            base.Clear();
        }

        protected void ExecAllOnAddItem()
        {
            if (OnAddItem != null)
            {
                for (int i = 0; i < Count; i++)
                    OnAddItem(this, this[i], i);
            }
        }

        protected void ExecAllOnRemoveItem()
        {
            if (OnRemoveItem != null)
            {
                for (int i = 0; i < Count; i++)
                    OnRemoveItem(this, this[i], i);
            }
        }

        protected void ExecAllOnRelocateItem()
        {
            if (OnRelocateItem != null)
            {
                for (int i = 0; i < Count; i++)
                    OnRelocateItem(this, this[i], i, i);
            }
        }
    }
}