using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine.Assertions;
using BehaviourInterop;

namespace ObjectGrid
{
    public class GridCell : CachedMonoBehaviour, IAssignable, IMovable2D, IMovable3D, ISizable2D, ISizable3D
    {
        public virtual void SetPosition(float x, float y)
        {
            SetPosition(x, y, 0);
        }

        public virtual void SetPosition(Vector2 position)
        {
            SetPosition(position.x, position.y, 0);
        }

        public virtual Vector2 GetPosition2D()
        {
            return GetPosition3D();
        }

        public virtual void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        public virtual void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public virtual Vector3 GetPosition3D()
        {
            return transform.position;
        }

        public virtual void SetSize(float x, float y)
        {
            SetSize(x, y, 0);
        }

        public virtual void SetSize(Vector2 size)
        {
            SetSize(size.x, size.y, 0);
        }

        public virtual Vector2 GetSize2D()
        {
            return GetSize3D();
        }

        public virtual void SetSize(float x, float y, float z)
        {
            SetSize(new Vector3(x, y, z));
        }

        public virtual void SetSize(Vector3 size)
        {
            transform.localScale = size;
        }

        public virtual Vector3 GetSize3D()
        {
            return transform.localScale;
        }

        public virtual void Assign(IAssignable source)
        {
            Assert.IsNotNull(source);
            Assert.IsFalse(source is GridCell);
            GridCell sobj = source as GridCell;
        }
    }

}