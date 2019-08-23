using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BuildingObject_Script_PlacePoint
{
    public Vector2Int CellPosition;
    public MapCell_Script Cell;
    public bool ValidToPlace;
}

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(UnitObject_Script))]
[RequireComponent(typeof(BuildingMovingObject_Script))]
public class BuildingObject_Script : MonoBehaviour {

        public Vector2Int Default_Size = Vector2Int.one;
        public bool Default_UseHouseIndicator = false;
        public Vector2 Default_HouseIndicatorSpacing = new Vector2(8, 8);
       
        public NotifyEvent<BuildingObject_Script> OnProducedBuildingsListChanged = new NotifyEvent<BuildingObject_Script>();
        public NotifyEvent<BuildingObject_Script> OnCurrentProducedBuldingPlaced = new NotifyEvent<BuildingObject_Script>();
        public NotifyEvent<BuildingObject_Script> OnCurrentProducedBuildingBeginPlacing = new NotifyEvent<BuildingObject_Script>();
        public NotifyEvent<BuildingObject_Script> OnCurrentProducedBuildingCancelPlacing = new NotifyEvent<BuildingObject_Script>();

        protected Vector2Int fSize = Vector2Int.zero;
        protected Vector2 fWorldSize = Vector2.zero;

        protected bool fReady = false;
        protected UnitObject_Script fUnit = null;
        protected BuildingHouseIndicator_Script fHouseIndicator = null;
        protected ProductionObject_Script fProductionComp = null;
        protected MapCell_Script[] fCellsBuffer = null;
        protected int fCellsBufferFactSize = 0;
        protected CarrierLoadingPlatform_Script fLoadingPlatform = null;
        protected List<BuildingObject_Script> fProducedBuildings = new List<BuildingObject_Script>();
        protected bool fProducedBuldingsPlacing = false;
        protected BuildingObject_Script fCurrentProducedBuilding = null;
        protected bool fOkPlacingActive = false;
        protected GameObject fOkPlacingIndicator = null;
    


    public UnitObject_Script Unit
    {
        get
        {
            return fUnit;
        }
    }

    public ProductionObject_Script Production_Comp
    {
        get
        {
            return fProductionComp;
        }
    }

    public Vector2Int Size
    {
        get
        {
            return fSize;
        }

        set
        {
            fSize = value;

            if (!fReady)
                return;

            Vector2 nsize = new Vector2(fSize.x * GlobalCollector.Cell_Size, fSize.y * GlobalCollector.Cell_Size);
            fWorldSize = nsize;
            fUnit.CompCollection.GameObject_Comp.Body_Collider.size = nsize;

            if (fUnit.CompCollection.EnduranceIndicator_Comp != null)
            {
                fUnit.CompCollection.EnduranceIndicator_Comp.Spacing = nsize.y / 2;
                fUnit.CompCollection.EnduranceIndicator_Comp.WidthScalling = nsize.x / GlobalCollector.Cell_Size;
            }

            if (fHouseIndicator != null)
                fHouseIndicator.Body.localPosition = new Vector2(-nsize.x / 2, -nsize.y / 2) + Default_HouseIndicatorSpacing;

            fUnit.CompCollection.MovingObject_Comp.ResetMapPosition();
            fCellsBuffer = new MapCell_Script[((fSize.x + 2) * 2) + ((fSize.y + 2) * 2)];
        }
    }

    public Vector2 WorldSize
    {
        get
        {
            return fWorldSize;
        }
    }

    public RectInt Bounds
    {
        get
        {
            return new RectInt(fUnit.CompCollection.MovingObject_Comp.MapPosition, fSize);
        }
    }

    public CarrierLoadingPlatform_Script LoadingPlatform
    {
        get
        {
            return fLoadingPlatform;
        }

        set
        {
            fLoadingPlatform = value;
        }
    }

    protected bool UnitNearBuildingBorderMatch(MapCell_Script cell, object arg)
    {
        UnitObject_Script unit = arg as UnitObject_Script;

        return cell.ContainsPoint(unit.Position);
    }

    public bool UnitNearBuildingBorder(UnitObject_Script unit)
    {
        RectInt rect = Bounds;
        rect.max -= Vector2Int.one;
        return GlobalCollector.Instance.Current_Map.MatchCellAroundRect(rect, UnitNearBuildingBorderMatch, unit) != null;
    }

    public MapCell_Script FindAllowedCellAround()
    {
        RectInt rect = Bounds;
        rect.max -= Vector2Int.one;
        fCellsBufferFactSize = GlobalCollector.Instance.Current_Map.FindAllowedCellsToGroundObjectAroundRect(rect, fCellsBuffer);

        if (fCellsBufferFactSize > 0)
            return fCellsBuffer[0];
        else
            return null;
    }
    
    public List<MapCell_Script> FindAllowedCellsAround()
    {
        RectInt rect = Bounds;
        rect.max -= Vector2Int.one;
        fCellsBufferFactSize = GlobalCollector.Instance.Current_Map.FindAllowedCellsToGroundObjectAroundRect(rect, fCellsBuffer);

        if (fCellsBufferFactSize > 0)
        {
            List<MapCell_Script> result = new List<MapCell_Script>(fCellsBufferFactSize);

            for (int i = 0; i < fCellsBufferFactSize; i++)
            {
                result.Add(fCellsBuffer[i]);
            }

            return result;
        }
        else
            return null;
    }

    public bool IsPivotPointAvailableForPlacing(Vector3 point)
    {
        Vector2Int pivot = GlobalCollector.Instance.Current_Map.WorldToMapCoord(point);
        Vector2Int cur_pos = new Vector2Int();
        MapCell_Script cell;

        for (int i = 0; i < fSize.y; i++)
        {
            for (int j = 0; j < fSize.x; j++)
            {
                cur_pos.x = pivot.x + j;
                cur_pos.y = pivot.y + i;
                cell = GlobalCollector.Instance.Current_Map.GetMapCell(j, i);

                if ((cell == null) ||
                    (!cell.IsAvailableToBuildings))
                    return false;
            }
        }

        return true;
    }

    public int MatchPivotPointForPlacing(Vector3 point, ref BuildingObject_Script_PlacePoint[] result)
    {
        Vector2Int pivot = GlobalCollector.Instance.Current_Map.WorldToMapCoord(point);
        Vector2Int cur_pos = new Vector2Int();
        int count = 0;
        MapCell_Script cell;

        for (int i = 0; i<fSize.y; i++)
        {
            for (int j=0; j<fSize.x; j++)
            {
                cur_pos.x = pivot.x + j;
                cur_pos.y = pivot.y + i;
                cell = GlobalCollector.Instance.Current_Map.GetMapCell(j, i);

                result[count].CellPosition = cur_pos;
                result[count].Cell = cell;
                result[count].ValidToPlace = (cell != null) && 
                                             cell.IsAvailableToBuildings;
            }
        }

        return count;
    }

    public virtual BuildingObject_Script TakeFirstProducedBuildingForPlace()
    {
        if (fProducedBuildings.Count == 0)
            return null;

        if (fCurrentProducedBuilding != null)
            return fCurrentProducedBuilding;

        fCurrentProducedBuilding = fProducedBuildings[0];
        OkPlacingActive = false;

        OnCurrentProducedBuildingBeginPlacing.Invoke(this);
        return fCurrentProducedBuilding;
    }

    public virtual void PlaceCurrentProducedBuilding(Vector3 pos)
    {
        if ((fCurrentProducedBuilding == null) ||
            !IsPivotPointAvailableForPlacing(pos))
            return;

        fCurrentProducedBuilding.Unit.MapPosition = GlobalCollector.Instance.Current_Map.WorldToMapCoord(pos);
        fCurrentProducedBuilding.Unit.OnPlatform = false;
        fCurrentProducedBuilding.Unit.Visible = true;

        DeleteProducedBuilding(fCurrentProducedBuilding);

        OkPlacingActive = fProducedBuildings.Count > 0;

        try
        {
            OnCurrentProducedBuldingPlaced.Invoke(this);
        }
        finally
        {
            fCurrentProducedBuilding = null;
        }
    }

    public virtual void CancelPlacingCurrentProducedBuilding()
    {
        if (fCurrentProducedBuilding == null)
            return;

        try
        {
            OnCurrentProducedBuildingCancelPlacing.Invoke(this);
        }
        finally
        {
            fCurrentProducedBuilding = null;
            OkPlacingActive = true;
        }
    }

    public BuildingObject_Script CurrentProducedBuilding
    {
        get
        {
            return fCurrentProducedBuilding;
        }
    }

    protected virtual bool OkPlacingActive
    {
        get
        {
            return fOkPlacingActive;
        }

        set
        {
            fOkPlacingActive = value;

            if (fOkPlacingIndicator != null)
            {
                fOkPlacingIndicator.SetActive(fOkPlacingActive);
            }
        }
    }

    IEnumerator DoProducedItem(UnitObject_Script unit)
    {
        yield return null;

        if (unit.IsGroundUnit())
        {
            if (unit.IsBuilding())
            {
                unit.OnPlatform = true;
                unit.Visible = false;
                AddProducedBuilding(unit.Building_Comp);
            }
            else
            {
                fLoadingPlatform.PushObjectToPlatform(unit);
                fLoadingPlatform.UnloadFirstNearBuilding();
            }
        }
    }

    public void Event_OnProducedItem(ProductionObject_Script_ProductionQuery query, ProductionObject_Script_ProductionItem item)
    {
        if (item is ProductionItem_ObjectType)
        {
            UnitObject_Script unit = (item as ProductionItem_ObjectType).ProducedObject.GetComponent<UnitObject_Script>();

            if (unit != null)
            {
                unit.gameObject.SetActive(true);
                StartCoroutine(DoProducedItem(unit));
            }
        }
    }

    protected virtual void AddProducedBuilding(BuildingObject_Script building)
    {
        if ((building == null) ||
            fProducedBuildings.Contains(building))
            return;

        fProducedBuildings.Add(building);
        OnProducedBuildingsListChanged.Invoke(this);
    }

    protected virtual void DeleteProducedBuilding(BuildingObject_Script building)
    {
        fProducedBuildings.Remove(building);
        OnProducedBuildingsListChanged.Invoke(this);
    }

    IEnumerator Initialize()
    {
        for (int i = 0; i < 2; i++)
            yield return null;

        fReady = true;

        if (Default_UseHouseIndicator)
        {
            fHouseIndicator = BuildingHouseIndicator_Script.Instantiate(GlobalCollector.Instance.Default_HouseIndicator, fUnit.CompCollection.GameObject_Comp.Body);
            fHouseIndicator.Unit.SetActive(true);
        }

        Size = Size;

        if (GlobalCollector.Instance.Default_OkPlaceIndicatorSample != null)
        {
            fOkPlacingIndicator = Instantiate(GlobalCollector.Instance.Default_OkPlaceIndicatorSample, fUnit.CompCollection.GameObject_Comp.Body);
            OkPlacingActive = OkPlacingActive;
        }
    }

	// Use this for initialization
	void Start () {
        fUnit = this.GetComponent<UnitObject_Script>();
        fProductionComp = this.GetComponent<ProductionObject_Script>();
        Size = Default_Size;

        if (fProductionComp != null)
        {
            fProductionComp.ProductionQuery.OnCurrentItemProduced.AddListener(Event_OnProducedItem);
        }

        StartCoroutine(Initialize());
	}
	
	// Update is called once per frame
	void Update () {
	}
}
