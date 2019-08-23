using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectCullingManager : MonoBehaviour
{
    public int FrameSkipValue = 3;

    protected static LinkedList<ObjectCulling_Script> fSceneRenderers = new LinkedList<ObjectCulling_Script>();


    public static void AddObject(ObjectCulling_Script renderer)
    {
        fSceneRenderers.AddLast(renderer);
    }

    public static void RemoveObject(ObjectCulling_Script renderer)
    {
        fSceneRenderers.Remove(renderer);
    }
    
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % FrameSkipValue == 0)
        {
            LinkedListNode<ObjectCulling_Script> node = fSceneRenderers.First;

            while (node != null)
            {
                node.Value.UpdateCulling();
                node = node.Next;
            }
        }
    }
}
