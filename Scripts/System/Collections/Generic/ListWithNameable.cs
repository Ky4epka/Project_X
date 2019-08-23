using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    public class ListWithNameable<T>: ListEx<T> where T: System.Runtime.InteropServices.INameable
    {
        public ListWithNameable() : base()
        {
        }

        public ListWithNameable(IEnumerable<T> collection) : base(collection)
        {
        }

        public ListWithNameable(int capacity) : base(capacity)
        {
        }

        public bool Contains(string itemName)
        {
            return IndexOf(itemName) != -1;
        }
               
        protected int IndexOfByNameEx(string itemName, int index, int count, bool fromEnd)
        {
            int dir_step = 1;
            int end = index + count;

            if (fromEnd)
            {
                end = index - count;

                if (end < 0)
                    end = 0;

                dir_step = -1;
            }
            else if (end > Count)
                end = Count;

            for (int i = index; i != end; i += dir_step)
            {
                if (string.Equals((this[i] as System.Runtime.InteropServices.INameable).GetName(), itemName))
                {
                    return i;
                }
            }

            return -1;
        }

        public int IndexOf(string itemName, int index, int count)
        {
            return IndexOfByNameEx(itemName, index, count, false);
        }

        public int IndexOf(string itemName, int index)
        {
            return IndexOfByNameEx(itemName, index, Count, false);
        }

        public int IndexOf(string itemName)
        {
            return IndexOfByNameEx(itemName, 0, Count, false);
        }
        
        public int LastIndexOf(string itemName, int index, int count)
        {
            return IndexOfByNameEx(itemName, index, count, true);
        }

        public int LastIndexOf(string itemName, int index)
        {
            return IndexOfByNameEx(itemName, index, Count, true);
        }

        public int LastIndexOf(string itemName)
        {
            return IndexOfByNameEx(itemName, 0, Count, true);
        }

        public bool Remove(string itemName)
        {
            int index = IndexOf(itemName);

            if (index == -1)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

    }
}