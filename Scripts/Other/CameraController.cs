using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

    public bool Default_UseCameraControl = false;
    public float Default_SeekTime = 1f;
    public float Default_OffsetPerKey = 1f;

    protected Transform fBody = null;
    protected Camera fCamera = null;
    protected Vector2 fWorldBoundsTopLeftViewport = Vector2.zero;
    protected Vector2 fWorldBoundsBottomRightViewport = Vector2.one;
    protected Vector3 fWorldBoundsScaleOffset = new Vector3(GlobalCollector.Cell_Size, GlobalCollector.Cell_Size, 0);
    protected bool fWorldBoundsCacheDeprecated = false;
    protected Rect fWorldBoundsCache = Rect.zero;
    [SerializeField]
    protected Vector3 fTargetPoint = Vector3.zero;
    [SerializeField]
    protected Vector3 fPosition = Vector3.zero;
    [SerializeField]
    protected float fCalcedSpeed = 0f;
    [SerializeField]
    protected float fSeekTime = 0f;
    [SerializeField]
    protected bool fUseCameraControl = false;
    [SerializeField]
    protected float fOffsetPerKey = 0f;

    protected bool fReady = false;

    public Camera Camera
    {
        get
        {
            return fCamera;
        }
    }

    public Rect WorldBounds
    {
        get
        {
            if (fWorldBoundsCacheDeprecated)
            {
                fWorldBoundsCache.min = fCamera.ViewportToWorldPoint(fWorldBoundsTopLeftViewport) - fWorldBoundsScaleOffset;
                fWorldBoundsCache.max = fCamera.ViewportToWorldPoint(fWorldBoundsBottomRightViewport) + fWorldBoundsScaleOffset;
                fWorldBoundsCacheDeprecated = false;
            }

            return fWorldBoundsCache;
        }
    }

    public float SeekTime
    {
        get
        {
            return fSeekTime;
        }

        set
        {
            fSeekTime = value;
        }
    }

    public float OffsetPerKey
    {
        get
        {
            return fOffsetPerKey;
        }

        set
        {
            fOffsetPerKey = value;
        }
    }

    public bool UseCameraControl
    {
        get
        {
            return fUseCameraControl;
        }

        set
        {
            if (fUseCameraControl == value)
                return;
            
            fUseCameraControl = value;
        }
    }

    public void MoveTo(Vector3 point)
    {
        Vector3 delta = point - Position;
        fTargetPoint = point;
        fCalcedSpeed = delta.magnitude / fSeekTime;
    }

    public void OffsetTo(Vector3 offset)
    {
        MoveTo(fTargetPoint + offset);
    }

    public void Event_KeyPress(KeyCode key)
    {
        if (!fUseCameraControl)
            return;

        switch (key)
        {
            case KeyCode.LeftArrow:
                OffsetTo(Vector3.left * fOffsetPerKey * Time.deltaTime);
                break;
            case KeyCode.RightArrow:
                OffsetTo(Vector3.right * fOffsetPerKey * Time.deltaTime);
                break;
            case KeyCode.UpArrow:
                OffsetTo(Vector3.up * fOffsetPerKey * Time.deltaTime);
                break;
            case KeyCode.DownArrow:
                OffsetTo(Vector3.down * fOffsetPerKey * Time.deltaTime);
                break;
        }
    }

    public Vector3 Position
    {
        get
        {
            return fPosition;
        }

        set
        {
            if (MathKit.Vectors3DEquals(value, fPosition) &&
                fReady)
                return;

            fPosition = value;
            fWorldBoundsCacheDeprecated = true;

            if (!fReady)
                return;

            fBody.position = fPosition;
        }
    }
    
    private void Awake()
    {
        fBody = transform;
        fCamera = GetComponent<Camera>();
    }

    IEnumerator DoInit()
    {
        yield return null;
        fReady = true;
        GlobalCollector.Instance.InputMan.OnKeyPress.AddListener(Event_KeyPress);
        UseCameraControl = Default_UseCameraControl;
        SeekTime = Default_SeekTime;
        OffsetPerKey = Default_OffsetPerKey;
        Position = Position;
        fTargetPoint = Position;
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(DoInit());
        Position = fBody.position;
	}
	
	// Update is called once per frame
	void Update () {
        if (!fUseCameraControl)
            return;

        Vector3 delta = fTargetPoint - Position;
        Vector3 pos = Position;

        if (delta.magnitude > fCalcedSpeed * Time.deltaTime)
        {
            pos += delta.normalized * fCalcedSpeed * Time.deltaTime;
        }
        else
            pos = fTargetPoint;

        pos = MathKit.EnsureVectorRectRange(pos, GlobalCollector.Instance.Current_Map.WorldBounds);
        pos.z = -1;
        Position = pos;
    }

}
