using UnityEngine;
using System.Collections;

public class MathKit : MonoBehaviour
{
    public static bool NormalizeRange(ref int min, ref int max)
    {
        if (min > max)
        {
            int temp = min;
            min = max;
            max = temp;
            return false;
        }

        return true;
    }

    public static int EnsureRange(int val, int min, int max)
    {
        NormalizeRange(ref min, ref max);

        if (val < min)
            val = min;
        else if (val > max)
            val = max;

        return val;
    }

    public static Vector2 EnsureVectorRectRange(Vector2 val, Rect r)
    {
        val.x = EnsureRange(val.x, r.xMin, r.xMax);
        val.y = EnsureRange(val.y, r.yMin, r.yMax);
        return val;
    }

    public static bool NormalizeRange(ref float min, ref float max)
    {
        if (min > max)
        {
            float temp = min;
            min = max;
            max = temp;
            return false;
        }

        return true;
    }

    public static float EnsureRange(float val, float min, float max)
    {
        NormalizeRange(ref min, ref max);

        if (val < min)
            val = min;
        else if (val > max)
            val = max;

        return val;
    }

    public static bool RectIntContainsRect(RectInt who, RectInt check)
    {
        return (check.xMin >= who.xMin) &&
               (check.yMin >= who.yMin) &&
               (check.xMax <= who.xMax) &&
               (check.yMax <= who.yMax);
    }

    public static bool NumbersEquals(float n1, float n2, float eps)
    {
        float n = n1 - n2;

        if (n < 0)
            n = -n;

        return n <= eps;
    }

    public static bool NumbersEquals(float n1, float n2)
    {
        return NumbersEquals(n1, n2, float.Epsilon);
    }

    public static bool NumbersEquals(double n1, double n2, double eps)
    {
        double n = n1 - n2;

        if (n < 0)
            n = -n;

        return n <= eps;
    }
    
    public static bool NumbersEquals(double n1, double n2)
    {
        return NumbersEquals(n1, n2, double.Epsilon);
    }

    public static bool Vectors2DEquals(Vector2 vec1, Vector2 vec2)
    {
        return NumbersEquals(vec1.x, vec2.x, Vector2.kEpsilon) && NumbersEquals(vec1.y, vec2.y, Vector2.kEpsilon);            
    }

    public static bool Vectors3DEquals(Vector3 vec1, Vector3 vec2)
    {
        return NumbersEquals(vec1.x, vec2.x, Vector3.kEpsilon) && 
               NumbersEquals(vec1.y, vec2.y, Vector3.kEpsilon) && 
               NumbersEquals(vec1.z, vec2.z, Vector3.kEpsilon);
    }


    public static Vector2 RotateVector2D(Vector2 vec, float angle)
    {
        angle = Mathf.Deg2Rad * angle;
        return new Vector2(vec.x * Mathf.Cos(angle) - vec.y * Mathf.Sin(angle), 
                           vec.x * Mathf.Sin(angle) + vec.y * Mathf.Cos(angle));
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
