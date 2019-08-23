using UnityEngine;

namespace BehaviourInterop
{
    public interface ISizable2D
    {
        void SetSize(float x, float y);
        void SetSize(Vector2 position);
        Vector2 GetSize2D();
    }

    public interface ISizable2DInt
    {
        void SetSize(int x, int y);
        void SetSize(Vector2Int position);
        Vector2Int GetSize2DInt();
    }

    public interface ISizable3D
    {
        void SetSize(float x, float y, float z);
        void SetSize(Vector3 position);
        Vector3 GetSize3D();
    }

    public interface ISizable3DInt
    {
        void SetPosition(int x, int y, int z);
        void SetPosition(Vector3Int position);
        Vector3Int GetSize3DInt();
    }

}