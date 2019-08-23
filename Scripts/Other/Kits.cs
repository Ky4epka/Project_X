using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Kits : MonoBehaviour
{
    protected static Vector3 fBufferVector3 = Vector3.zero;

    protected static int SortVector3OnNearestPoint_Comparer(Vector3 p1, Vector3 p2)
    {
        float d1 = Vector3.Distance(p1, fBufferVector3);
        float d2 = Vector3.Distance(p2, fBufferVector3);

        if (d1 > d2)
            return 1;
        else if (d1 < d2)
            return -1;

        return 0;
    }

    public static void SortVector3OnNearestPoint(List<Vector3> list, Vector3 point)
    {
        fBufferVector3 = point;
        list.Sort(SortVector3OnNearestPoint_Comparer);
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
