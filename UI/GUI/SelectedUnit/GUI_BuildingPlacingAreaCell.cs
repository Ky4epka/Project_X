using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class GUI_BuildingPlacingAreaCell : MonoBehaviour
{
        public Color Available_Color = Color.green;
        public Color Unavailable_Color = Color.red;

    [SerializeField]
    protected GameObject fUnit = null;
    [SerializeField]
    protected SpriteRenderer fRenderer = null;
    [SerializeField]
    protected BoxCollider2D fBoxCollider = null;
    [SerializeField]
    protected Transform fBody = null;
    [SerializeField]
    protected Vector2Int fPosition = Vector2Int.zero;
    [SerializeField]
    protected GUI_BuildingPlacingArea fAreaOwner = null;

    protected static int Update_FrameSkipModule = 5;


    public GUI_BuildingPlacingArea AreaOwner
    {
        get
        {
            return fAreaOwner;
        }

        set
        {
            fAreaOwner = value;
        }
    }

    public bool AvailableToBuildingPlace
    {
        get
        {
            MapCell_Script cell = GlobalCollector.Instance.Current_Map.GetMapCell(Body.position);
            
            return (cell != null) &&
                   (cell.IsAvailableToBuildings);
        }
    }

    public Vector2Int Position
    {
        get
        {
            return fPosition;
        }

        set
        {
            fPosition = value;
            Body.localPosition = new Vector3(fPosition.x * GlobalCollector.Cell_Size, -fPosition.y * GlobalCollector.Cell_Size, 0);
        }
    }

    public bool Visible
    {
        get
        {
            return Unit.activeSelf;
        }

        set
        {
            Unit.SetActive(value);
        }
    }

    public Transform Body
    {
        get
        {
            if (fBody == null)
                fBody = transform;

            return fBody;
        }
    }

    public SpriteRenderer Renderer
    {
        get
        {
            if (fRenderer == null)
                fRenderer = this.GetComponent<SpriteRenderer>();

            return fRenderer;
        }
    }

    public BoxCollider2D BoxCollider
    {
        get
        {
            if (fBoxCollider == null)
                fBoxCollider = this.GetComponent<BoxCollider2D>();

            return fBoxCollider;
        }
    }

    public GameObject Unit
    {
        get
        {
            if (fUnit == null)
                fUnit = this.gameObject;

            return fUnit;
        }
    }

    protected void UpdateUI()
    {
        if (AvailableToBuildingPlace)
        {
            Renderer.color = Available_Color;
        }
        else
        {
            Renderer.color = Unavailable_Color;
        }
    }
    
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % Update_FrameSkipModule == 0)
        {
            UpdateUI();
        }
    }
}
