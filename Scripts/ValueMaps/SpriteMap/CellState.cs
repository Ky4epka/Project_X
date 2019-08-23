using UnityEngine;
using System.Collections;

namespace SpriteMap
{
    [System.Serializable]
    public struct CellStateProps
    {
        public string Key;
        public bool Active;
        public Sprite Sprite;

        public CellStateProps(bool use_default)
        {
            Key = "";
            Active = false;
            Sprite = null;

            if (use_default)
            {
                Default();
            }
        }

        public void Default()
        {
            Key = "default_key";
            Active = false;
            Sprite = null;
        }
    }

    [RequireComponent(typeof(SpriteRenderer))]
    [System.Serializable]
    public class CellState : CachedMonoBehaviour
    {
        [Tooltip("Use this to set initial props before 'Start' function of this script.")]
        public CellStateProps Initial_Props = new CellStateProps(true);

        [SerializeField]
        protected CellStateProps fProps = new CellStateProps();

        public void ApplyProps(CellStateProps props)
        {
            Key = props.Key;
            Active = props.Active;
            Sprite = props.Sprite;
        }
        
        public CellStateProps CollectProps()
        {
            return fProps;
        }

        public string Key
        {
            get
            {
                return fProps.Key;
            }

            set
            {
                fProps.Key = value;
            }
        }
        
        public bool Active
        {
            get
            {
                return fProps.Active;
            }

            set
            {
                fProps.Active = value;
                spriteRenderer.enabled = value;
            }
        }

        public Sprite Sprite
        {
            get
            {
                return fProps.Sprite;
            }

            set
            {
                fProps.Sprite = value;
                spriteRenderer.sprite = value;
            }
        }

        void Start()
        {
            if (transform.parent != null)
            {
                Cell parent_cell = transform.parent.GetComponent<Cell>();

                if (parent_cell != null)
                {
                    parent_cell.AddState(this);
                }
            }

            ApplyProps(Initial_Props);
        }

        private void OnDestroy()
        {
            if (transform.parent != null)
            {
                Cell parent_cell = transform.parent.GetComponent<Cell>();

                if (parent_cell != null)
                {
                    parent_cell.RemoveState(this, true);
                }
            }
        }
    }
}