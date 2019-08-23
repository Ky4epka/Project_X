using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GUI_BuildingPlacingArea : MonoBehaviour
{

    public GUI_BuildingPlacingAreaCell Cell_Sample = null;

    public NotifyEvent<GUI_BuildingPlacingArea> OnPlace = new NotifyEvent<GUI_BuildingPlacingArea>();
    public NotifyEvent<GUI_BuildingPlacingArea> OnCancel = new NotifyEvent<GUI_BuildingPlacingArea>();

    [SerializeField]
    protected bool fReady = false;
    [SerializeField]
    protected GameObject fUnit = null;
    [SerializeField]
    protected Transform fBody = null;
    [SerializeField]
    protected Vector2Int fCachedSize = Vector2Int.zero;
    [SerializeField]
    protected List<GUI_BuildingPlacingAreaCell> fCells = new List<GUI_BuildingPlacingAreaCell>();

    protected static BuildingObject_Script_PlacePoint[] PlacePointsBuffer = new BuildingObject_Script_PlacePoint[GlobalCollector.MaxBuildingSize.x *
                                                                                                                 GlobalCollector.MaxBuildingSize.y];

    public Vector3 PlacingPivot
    {
        get
        {
            return Body.position;
        }

        set
        {
            Body.position = GlobalCollector.Instance.Current_Map.MapCoordToWorld(GlobalCollector.Instance.Current_Map.WorldToMapCoord(value));
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

    public Transform Body
    {
        get
        {
            if (fBody == null)
                fBody = transform;

            return fBody;
        }
    }

    public bool PlacingAvailable
    {
        get
        {
            if (!fReady)
                return false;

            for (int i = 0; i<fCachedSize.y; i++)
            {
                for (int j = 0; j<fCachedSize.x; j++)
                {
                    if (!fCells[i * GlobalCollector.MaxBuildingSize.x + j].AvailableToBuildingPlace)
                        return false;
                }
            }

            return true;
        }
    }

    public void ShowPlacingArea(Vector2Int size)
    {
        size.x = MathKit.EnsureRange(size.x, 0, GlobalCollector.MaxBuildingSize.x);
        size.y = MathKit.EnsureRange(size.y, 0, GlobalCollector.MaxBuildingSize.y);

        fCachedSize = size;

        if (!fReady)
            return;
        
        Unit.SetActive(true);
        Debug.Log("Show placing area: " + size);

        for (int i=0; i<GlobalCollector.MaxBuildingSize.y; i++)
        {
            for (int j=0; j< GlobalCollector.MaxBuildingSize.x; j++)
            {
                fCells[i * GlobalCollector.MaxBuildingSize.x + j].Visible = (j < fCachedSize.x) &&
                                                                            (i < fCachedSize.y);
            }
        }
    }

    public void HidePlacingArea()
    {
        ShowPlacingArea(Vector2Int.zero);
        Unit.SetActive(false);
    }

    public void OnDestroy()
    {
        GlobalCollector.Instance.InputMan.OnMapCellPointerDown.RemoveListener(Event_OnMapCellPointerDown);
    }

    public void Event_OnMapCellPointerDown(MapCell_Script sender, PointerEventData eventData)
    {
        if (!Unit.activeSelf)
            return;

        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                if (PlacingAvailable)
                    OnPlace.Invoke(this);
                break;
            case PointerEventData.InputButton.Right:
                OnCancel.Invoke(this);
                break;
        }
    }

    IEnumerator DoInit()
    {
        yield return null;
        Vector2Int cpos = Vector2Int.zero;
        GlobalCollector.Instance.InputMan.OnMapCellPointerDown.AddListener(Event_OnMapCellPointerDown);

        for (int i=0; i<GlobalCollector.MaxBuildingSize.y; i++)
        {
            for (int j=0; j<GlobalCollector.MaxBuildingSize.x; j++)
            {
                cpos.x = j;
                cpos.y = i;
                GUI_BuildingPlacingAreaCell cell = GUI_BuildingPlacingAreaCell.Instantiate(Cell_Sample, Body);
                cell.AreaOwner = this;
                cell.Visible = false;
                cell.Position = cpos;
                fCells.Add(cell);
            }
        }

        fReady = true;

        if (fCachedSize != Vector2Int.zero)
            ShowPlacingArea(fCachedSize);
        else
            HidePlacingArea();
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(DoInit());
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mpos = Input.mousePosition;
        mpos.z = 0;
        PlacingPivot = GlobalCollector.Instance.MainCamera.Camera.ScreenToWorldPoint(mpos);
    }
}
