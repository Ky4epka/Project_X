using UnityEngine;

namespace BehaviourInterop
{
    public interface IMovable2D
    {
        void SetPosition(float x, float y);
        void SetPosition(Vector2 position);
        Vector2 GetPosition2D();
    }

    public interface IMovable2DInt
    {
        void SetPosition(int x, int y);
        void SetPosition(Vector2Int position);
        Vector2Int GetPosition2DInt();
    }

    public interface IMovable3D
    {
        void SetPosition(float x, float y, float z);
        void SetPosition(Vector3 position);
        Vector3 GetPosition3D();
    }

    public interface IMovable3DInt
    {
        void SetPosition(int x, int y, int z);
        void SetPosition(Vector3Int position);
        Vector3Int GetPosition3DInt();
    }

}