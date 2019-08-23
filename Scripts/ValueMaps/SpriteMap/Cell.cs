using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;

namespace SpriteMap
{
    [System.Serializable]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Cell : ObjectGrid.GridCell
    {
        [SerializeField]
        protected List<CellState> fStates = new List<CellState>();


        public bool AddState(CellStateProps new_state_props)
        {
            GameObject new_go = new GameObject();
            CellState new_state = new_go.AddComponent<CellState>();
            new_state.ApplyProps(new_state_props);
            AddState(new_state);
            return true;
        }

        public bool AddState(CellState existing_state)
        {
            int index = StateIndex(existing_state);

            if (index != -1)
            {
                Debug.LogError("The cell state already contains in this cell.");
                return false;
            }

            fStates.Add(existing_state);
            existing_state.gameObject.SetActive(true);
            existing_state.transform.parent = transform;
            return true;
        }

        public bool RemoveState(int index, bool destroy_object)
        {
            if (!CheckStateIndex(index))
            {
                return false;
            }

            CellState state = fStates[index];
            fStates.RemoveAt(index);

            if (destroy_object)
                Destroy(state.gameObject);
            else
            {
                state.transform.parent = null;
                state.gameObject.SetActive(false);
            }

            return true;
        }

        public bool RemoveState(string state_key, bool destroy_object)
        {            
            return RemoveState(StateIndex(state_key), destroy_object);
        }

        public bool RemoveState(CellState state, bool destroy_object)
        {
            return RemoveState(StateIndex(state), destroy_object);
        }

        public void SetCellState(string state_key, bool activate)
        {

        }

        public bool GetCellState(string state_key)
        {
            return false;
        }

        public int StateIndex(string state_key)
        {
            for (int i = 0; i < fStates.Count; i++)
                if (string.Compare(fStates[i].Key, state_key) == 0)
                    return i;

            return -1;
        }

        public int StateIndex(CellState state)
        {
            for (int i = 0; i < fStates.Count; i++)
                if (fStates[i] == state)
                    return i;

            return -1;
        }

        public CellState GetState(int index)
        {
            if (CheckStateIndex(index))
                return fStates[index];
            else
                return null;
        }

        public CellState GetState(string state_key)
        {
            int index = StateIndex(state_key);

            if (index != -1)
                return GetState(index);
            else
                Debug.LogError(string.Concat("Cell state was not found with key '", state_key, "' in this cell."));

            return null;
        }

        public CellState GetState(CellState state)
        {
            int index = StateIndex(state);

            if (index != -1)
                return GetState(index);
            else
                Debug.LogError(string.Concat("Cell state was not found in this cell.'"));

            return null;
        }

        public bool IsValidStateIndex(int index)
        {
            return (index >= 0) &&
                   (index < fStates.Count);
        }

        public bool CheckStateIndex(int index)
        {
            if (!IsValidStateIndex(index))
            {
                Debug.LogError(string.Concat("Invalid state index '", index, "' (0, ", fStates.Count, ")"));
                return false;
            }

            return true;
        }

        public override void SetSize(float x, float y)
        {
            SetSize(x, y, 0);
        }

        public override void SetSize(Vector2 size)
        {
            SetSize(size.x, size.y, 0);
        }

        public override Vector2 GetSize2D()
        {
            return GetSize3D();
        }

        public override void SetSize(float x, float y, float z)
        {
            SetSize(new Vector3(x, y, z));
        }

        public override void SetSize(Vector3 size)
        {
            spriteRenderer.size = size;
        }

        public override Vector3 GetSize3D()
        {
            return transform.localScale;
        }

        public override void Assign(IAssignable source)
        {
            Assert.IsNotNull(source);
            Assert.IsFalse(source is Cell);
            Cell sobj = source as Cell;
        }

        public void ResetStates()
        {
            foreach (CellState cstate in fStates)
            {
                cstate.Active = false;
            }
        }
    }

}
