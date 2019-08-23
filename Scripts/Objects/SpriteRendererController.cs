using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRendererController : ObjectCulling_Script
{
    public NotifyEvent_2P<SpriteRendererController, bool> OnVisibleChanged = new NotifyEvent_2P<SpriteRendererController, bool>();

    private SpriteRenderer fRenderer = null;
    private bool fVisible = false;


    public virtual SpriteRenderer Renderer
    {
        get
        {
            if (fRenderer == null)
                fRenderer = this.GetComponent<SpriteRenderer>();

            return fRenderer;
        }

        set
        {
            fRenderer = value;

            if (fRenderer != null)
                fRenderer.enabled = fVisible && !fRendererCulled;
        }
    }

    public virtual bool Visible
    {
        get
        {
            return fVisible;
        }

        set
        {
            if (fVisible == value)
                return;

            fVisible = value;

            if (Renderer != null)
                Renderer.enabled = value && !fRendererCulled;

            OnVisibleChanged.Invoke(this, value);
        }
    }

    protected override void ProcessRendererCulled(bool value)
    {
        if (Renderer != null)
            Renderer.enabled = fVisible && !value;

        base.ProcessRendererCulled(value);
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        if (Renderer != null)
            Visible = Renderer.enabled;
    }
}
