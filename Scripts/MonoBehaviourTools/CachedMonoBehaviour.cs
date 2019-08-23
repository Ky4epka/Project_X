using UnityEngine;
using System.Collections;

public class CachedMonoBehaviour : MonoBehaviour
{
    private Transform fcachedTransform = null;
    private GameObject fcachedGameObject = null;
    private SpriteRenderer fcachedSpriteRenderer = null;

    public new Transform transform
    {
        get
        {
            if (fcachedTransform == null)
                fcachedTransform = base.GetComponent<Transform>();

            return fcachedTransform;
        }
    }

    public new GameObject gameObject
    {
        get
        {
            if (fcachedGameObject == null)
                fcachedGameObject = base.gameObject;

            return fcachedGameObject;
        }
    }

    public SpriteRenderer spriteRenderer
    {
        get
        {
            if (fcachedSpriteRenderer == null)
                fcachedSpriteRenderer = base.GetComponent<SpriteRenderer>();

            return fcachedSpriteRenderer;
        }
    }
}
