using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectCulling_Script : MonoBehaviour
{
    public NotifyEvent_2P<ObjectCulling_Script, bool> OnCullStateChanged = new NotifyEvent_2P<ObjectCulling_Script, bool>();

    protected Transform fBody = null;
    protected bool fRendererCulled = false;
    protected Rect r;


    public Transform Body
    {
        get
        {
            if (fBody == null)
                fBody = transform;

            return fBody;
        }
    }


    public bool RendererCulled
    {
        get
        {
            return fRendererCulled;
        }
    }

    protected virtual void ProcessRendererCulled(bool value)
    {
        try
        {
            OnCullStateChanged.Invoke(this, value);
        }
        finally
        {

        }
    }

    protected virtual void DoRendererCulled(bool value)
    {
        if (fRendererCulled == value)
            return;

        fRendererCulled = value;

        ProcessRendererCulled(value);
    }

    public virtual void UpdateCulling()
    {
        DoRendererCulled(!GlobalCollector.Instance.MainCamera.WorldBounds.Contains(Body.position));
    }

    protected virtual void Awake()
    {
        ObjectCullingManager.AddObject(this);   
    }

    protected virtual void OnDestroy()
    {
        ObjectCullingManager.RemoveObject(this);
    }

    protected virtual void Start()
    {

    }
}
