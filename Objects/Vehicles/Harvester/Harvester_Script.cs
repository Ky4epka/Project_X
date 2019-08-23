using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitObject_Script))]
public class Harvester_Script : MonoBehaviour {

    public float Default_HarvestedSpice = 0;
    public float Default_SpiceRepositoryCapacity = 0;
    public float Default_HarvestingSpeed = 1;

    public NotifyEvent<Harvester_Script> OnSpiceStorageEmpty = new NotifyEvent<Harvester_Script>();
    public NotifyEvent<Harvester_Script> OnSpiceStorageFilled = new NotifyEvent<Harvester_Script>();
    public NotifyEvent_3P<Harvester_Script, float, float> OnSpiceChanged = new NotifyEvent_3P<Harvester_Script, float, float>();

    [SerializeField]
    protected UnitObject_Script fUnit_Comp = null;
    [SerializeField]
    protected LimitedValueFloat fHarvestedSpice = new LimitedValueFloat();
    [SerializeField]
    protected float fHarvestingSpeed = 1;
    [SerializeField]
    protected float fHarvestingTick = 0;
    [SerializeField]
    protected MapCell_Script fHarvestingCell = null;
    [SerializeField]
    protected SpiceRefinery_Script fAimingRefinery = null;
    [SerializeField]
    protected bool fRefineryReleaseWaiting = false;

    protected static string REFINERY_TYPE_NAME = "SpiceRefinery";
    protected static float HARVESTING_TIME_FREQUENCY = 0.1f;
    protected static float REFINERY_RELEASE_WAITING_CHECK_FREQUENCY = 1f;
    protected OrderManager_Script_OrderData fOrderData = new OrderManager_Script_OrderData();

    protected static UnitGroup_Script fBufferGroup = new UnitGroup_Script();


    public LimitedValueFloat HarvestedSpice
    {
        get
        {
            return fHarvestedSpice;
        }
    }

    public float SpiceRepositoryCapacity
    {
        get
        {
            return fHarvestedSpice.MaxValue;
        }

        set
        {
            fHarvestedSpice.MinValue = 0;
            fHarvestedSpice.MaxValue = value;
        }
    }

    public float HarvestingSpeed
    {
        get
        {
            return fHarvestingSpeed;
        }

        set
        {
            if (fHarvestingSpeed == value)
                return;

            fHarvestingSpeed = value;

            if (fHarvestingSpeed < 1)
                fHarvestingSpeed = 1;
        }
    }

    public static bool MatchCell_Method(MapCell_Script cell, object arg)
    {
        return (cell.SpiceValue > 0) &&
               (GlobalCollector.Instance.Current_Map.IsCellAllowedToGroundObjects(cell.GetMapCoord()) || 
                ((arg != null) && (cell.ContainingObjects.Contains(arg as GameObject))));
    }

    public static MapCell_Script FindSpiceCell(Vector3 at, GameObject arg)
    {
        return GlobalCollector.Instance.Current_Map.MatchCellAt(at, MatchCell_Method, arg);
    }

    public UnitObject_Script Unit
    {
        get
        {
            return fUnit_Comp;
        }
    }

    public void DoHarvest()
    {
        MapCell_Script spice_cell = FindSpiceCell(fUnit_Comp.Position, fUnit_Comp.CompCollection.GameObject_Comp.Unit);

        if (spice_cell == null)
        {
            if (!MathKit.NumbersEquals(HarvestedSpice.Value, 0))
            {
                SendToFreeSpiceRefinery();
            }

            return;
        }
        
        if (GlobalCollector.CheckDistanceBetweenOnCarrierLoadRadius(fUnit_Comp.Position, spice_cell.WordlPosition))
        {
            fOrderData.OrderType = OrderManager_Script_OrderType.Follow;
            fOrderData.SetTargetType(spice_cell.WordlPosition);
            fUnit_Comp.Owner.CarrierManager.Request_TranslateObjectToPoint(fUnit_Comp, spice_cell.WordlPosition);
        }
        else
        {
            fUnit_Comp.Order_Follow(spice_cell.WordlPosition);
        }
    }

    public void Event_OnSpiceValueChanged(float old_value, float new_value)
    {
        OnSpiceChanged.Invoke(this, old_value, new_value);
    }

    public void Event_OnSpiceMinBorder()
    {
        OnSpiceStorageEmpty.Invoke(this);
    }

    public void Event_OnSpiceMaxBorder()
    {
        if (fHarvestingCell != null)
        {
            fHarvestingCell = null;
            SendToFreeSpiceRefinery();
        }

        OnSpiceStorageFilled.Invoke(this);
    }
    
    public void Event_OnOrderComplete(OrderManager_Script sender, OrderManager_Script_OrderData data)
    {
        fRefineryReleaseWaiting = false;
        switch (data.TargetType)
        {
            case OrderManager_Script_OrderTargetType.Point:
                MapCell_Script cell = GlobalCollector.Instance.Current_Map.GetMapCell(data.Point);

                if ((cell != null) &&
                    (cell.SpiceValue > 0))
                {
                    fHarvestingCell = cell;
                }

                break;
            case OrderManager_Script_OrderTargetType.Object:
                SpiceRefinery_Script refinery = data.Unit.GetComponent<SpiceRefinery_Script>();

                if (refinery != null)
                {
                    if (refinery.Building.UnitNearBuildingBorder(fUnit_Comp))
                    {
                        if (!refinery.TryLoadHarvester(this))
                            SendToFreeSpiceRefinery();
                    }
                    else
                    {
                        if (refinery.Building.LoadingPlatform.HasFreeSpace)
                            fUnit_Comp.Owner.CarrierManager.Request_LoadObjectToPlatform(fUnit_Comp, refinery.Building.LoadingPlatform);
                        else
                            SendToFreeSpiceRefinery();
                    }
                }

                break;
        }
    }

    public bool Event_PerformOrder(OrderManager_Script sender, ref OrderManager_Script_OrderData data)
    {
        switch (data.TargetType)
        {
            case OrderManager_Script_OrderTargetType.Object:
                SpiceRefinery_Script refinery = data.Unit.GetComponent<SpiceRefinery_Script>();

                if ((refinery != null) &&
                    (refinery.Building.LoadingPlatform.HasFreeSpace))
                {
                    SetAimingRefinery(refinery);
                }

                break;
        }

        return true;
    }

    public void GlobalEvent_OnSpiceRefineryUnloadHarvester(SpiceRefinery_Script sender, Harvester_Script harvester)
    {
        if ((harvester != this) ||
            (harvester.Unit.OnCarrier))
            return;
    }

    public void Event_OrderCancel(OrderManager_Script sender, OrderManager_Script_OrderData data)
    {
        SetAimingRefinery(null);
    }

    protected void SetAimingRefinery(SpiceRefinery_Script refinery)
    {
        if (fAimingRefinery != null)
        {
            fAimingRefinery.Building.LoadingPlatform.StopLandingLightsAnim(this);
        }

        fAimingRefinery = refinery;

        if (fAimingRefinery != null)
        {
            fAimingRefinery.Building.LoadingPlatform.PlayLandingLightsAnim(this);
        }
    }

    public void Event_OnDie(UnitObject_Script sender)
    {
        GlobalCollector.Instance.Current_Map.DistributeSpice(fUnit_Comp.Position, fHarvestedSpice.Value);
    }

    IEnumerator DoProcessUnload()
    {
        yield return new WaitForSeconds(0.1f);

        if (!fUnit_Comp.OnPlatform)
        {
            DoHarvest();
        }
    }
    
    public void GlobalEvent_OnCarrierUnloadObject(CarrierCapability_Script sender, UnitObject_Script _object)
    {
        if ((_object == fUnit_Comp))
        {
            StartCoroutine(DoProcessUnload());
        }
    }

    protected UnitObject_Script FindAllowedSpiceRefinery()
    {
        fUnit_Comp.Owner.Units.MatchUnitsByTypeName(REFINERY_TYPE_NAME, ref fBufferGroup);
        SpiceRefinery_Script refinery;

        foreach (UnitObject_Script unit in fBufferGroup)
        {
            refinery = unit.GetComponent<SpiceRefinery_Script>();
            if ((refinery != null) &&
                !refinery.IsBusy)
            {
                return unit;
            }
        }

        return null;
    }

    protected void SendToSpiceRefinery(UnitObject_Script refinery)
    {
        if (refinery == null)
            return;

        if (GlobalCollector.CheckDistanceBetweenOnCarrierLoadRadius(fUnit_Comp.Position, refinery.Position))
        {
            BuildingObject_Script bcomp = refinery.GetComponent<BuildingObject_Script>();
            fOrderData.OrderType = OrderManager_Script_OrderType.Follow;
            fOrderData.SetTargetType(refinery);
            fUnit_Comp.Owner.CarrierManager.Request_LoadObjectToPlatform(fUnit_Comp, bcomp.LoadingPlatform);
        }
        else
        {
            fUnit_Comp.Order_Follow(refinery);
        }
    }

    protected void SendToFreeSpiceRefinery()
    {
        UnitObject_Script refinery = FindAllowedSpiceRefinery();

        Debug.Log("Free refinery: " + refinery);
        fRefineryReleaseWaiting = refinery == null;
        SendToSpiceRefinery(refinery);
    }

    IEnumerator DoPostInit()
    {
        yield return null;

        fHarvestedSpice.OnChanged.AddListener(Event_OnSpiceValueChanged);
        fHarvestedSpice.OnMinBorder.AddListener(Event_OnSpiceMinBorder);
        fHarvestedSpice.OnMaxBorder.AddListener(Event_OnSpiceMaxBorder);
        CarrierCapability_Script.Global_OnCarrierUnloadObject.AddListener(GlobalEvent_OnCarrierUnloadObject);
        fUnit_Comp.CompCollection.OrderManager_Comp.OnOrderComplete.AddListener(Event_OnOrderComplete);
        fUnit_Comp.OnDie.AddListener(Event_OnDie);
        fUnit_Comp.CompCollection.OrderManager_Comp.OnOrderPerform = Event_PerformOrder;
        fUnit_Comp.CompCollection.OrderManager_Comp.OnOrderCancel.AddListener(Event_OrderCancel);
        SpiceRepositoryCapacity = Default_SpiceRepositoryCapacity;
        HarvestedSpice.Value = Default_HarvestedSpice;
        HarvestingSpeed = Default_HarvestingSpeed;
    }

    private void OnDestroy()
    {
        CarrierCapability_Script.Global_OnCarrierUnloadObject.RemoveListener(GlobalEvent_OnCarrierUnloadObject);
    }

    // Use this for initialization
    void Start () {
        fUnit_Comp = this.GetComponent<UnitObject_Script>();
        StartCoroutine(DoPostInit());
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (fRefineryReleaseWaiting)
        {
            SendToFreeSpiceRefinery();
        }
        else if (fHarvestingCell != null)
        {
            fHarvestingTick += Time.deltaTime;

            if (fHarvestingTick >= HARVESTING_TIME_FREQUENCY)
            {
                fHarvestingTick = 0;
                float value = fHarvestingCell.TakeSpice(HarvestingSpeed * HARVESTING_TIME_FREQUENCY);
                fHarvestingCell.ReturnSpice(value - HarvestedSpice.Add(value));

                if (MathKit.NumbersEquals(value, 0))
                {
                    fHarvestingCell = null;
                    DoHarvest();
                }
            }
        }
    }
}


/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitObject_Script))]
public class Harvester_Script : MonoBehaviour {

    public float Default_HarvestedSpice = 0;
    public float Default_SpiceRepositoryCapacity = 0;
    public float Default_HarvestingSpeed = 1;

    public NotifyEvent<Harvester_Script> OnSpiceStorageEmpty = new NotifyEvent<Harvester_Script>();
    public NotifyEvent<Harvester_Script> OnSpiceStorageFilled = new NotifyEvent<Harvester_Script>();
    public NotifyEvent_3P<Harvester_Script, float, float> OnSpiceChanged = new NotifyEvent_3P<Harvester_Script, float, float>();

    [SerializeField]
    protected UnitObject_Script fUnit_Comp = null;
    [SerializeField]
    protected LimitedValueFloat fHarvestedSpice = new LimitedValueFloat();
    [SerializeField]
    protected float fHarvestingSpeed = 1;
    [SerializeField]
    protected float fHarvestingTick = 0;
    [SerializeField]
    protected MapCell_Script fHarvestingCell = null;
    [SerializeField]
    protected SpiceRefinery_Script fAimingRefinery = null;

    protected static string REFINERY_TYPE_NAME = "SpiceRefinery";
    protected static float HARVESTING_TIME_FREQUENCY = 0.1f;
    protected OrderManager_Script_OrderData fOrderData = new OrderManager_Script_OrderData();

    protected static UnitGroup_Script fBufferGroup = new UnitGroup_Script();


    public LimitedValueFloat HarvestedSpice
    {
        get
        {
            return fHarvestedSpice;
        }
    }

    public float SpiceRepositoryCapacity
    {
        get
        {
            return fHarvestedSpice.MaxValue;
        }

        set
        {
            fHarvestedSpice.MinValue = 0;
            fHarvestedSpice.MaxValue = value;
        }
    }

    public float HarvestingSpeed
    {
        get
        {
            return fHarvestingSpeed;
        }

        set
        {
            if (fHarvestingSpeed == value)
                return;

            fHarvestingSpeed = value;

            if (fHarvestingSpeed < 1)
                fHarvestingSpeed = 1;
        }
    }

    public static bool MatchCell_Method(MapCell_Script cell, object arg)
    {
        return (cell.SpiceValue > 0) &&
               (GlobalCollector.Instance.Current_Map.IsCellAllowedToGroundObjects(cell.GetMapCoord()) || 
                ((arg != null) && (cell.ContainingObjects.Contains(arg as GameObject))));
    }

    public static MapCell_Script FindSpiceCell(Vector3 at, GameObject arg)
    {
        return GlobalCollector.Instance.Current_Map.MatchCellAt(at, MatchCell_Method, arg);
    }

    public UnitObject_Script Unit
    {
        get
        {
            return fUnit_Comp;
        }
    }

    public void DoHarvest()
    {
        MapCell_Script spice_cell = FindSpiceCell(fUnit_Comp.Position, fUnit_Comp.CompCollection.GameObject_Comp.Unit);

        if (spice_cell == null)
        {
            if (!MathKit.NumbersEquals(HarvestedSpice.Value, 0))
            {
                SendToFreeSpiceRefinery();
            }

            return;
        }
        
        if (GlobalCollector.CheckDistanceBetweenOnCarrierLoadRadius(fUnit_Comp.Position, spice_cell.WordlPosition))
        {
            fOrderData.OrderType = OrderManager_Script_OrderType.Follow;
            fOrderData.SetTargetType(spice_cell.WordlPosition);
            fUnit_Comp.Owner.CarrierManager.Request_TranslateObjectToPoint(fUnit_Comp, spice_cell.WordlPosition);
        }
        else
        {
            fUnit_Comp.Order_Follow(spice_cell.WordlPosition);
        }
    }

    public void Event_OnSpiceValueChanged(float old_value, float new_value)
    {
        OnSpiceChanged.Invoke(this, old_value, new_value);
    }

    public void Event_OnSpiceMinBorder()
    {
        OnSpiceStorageEmpty.Invoke(this);
    }

    public void Event_OnSpiceMaxBorder()
    {
        if (fHarvestingCell != null)
        {
            fHarvestingCell = null;
            SendToFreeSpiceRefinery();
        }

        OnSpiceStorageFilled.Invoke(this);
    }
    
    public void Event_OnOrderComplete(OrderManager_Script sender, OrderManager_Script_OrderData data)
    {        
        switch (data.TargetType)
        {
            case OrderManager_Script_OrderTargetType.Point:
                MapCell_Script cell = GlobalCollector.Instance.Current_Map.GetMapCell(data.Point);

                if ((cell != null) &&
                    (cell.SpiceValue > 0))
                {
                    fHarvestingCell = cell;
                }

                break;
            case OrderManager_Script_OrderTargetType.Object:
                SpiceRefinery_Script refinery = data.Unit.GetComponent<SpiceRefinery_Script>();

                if (refinery != null)
                {
                    Debug.Log("Order object complete " + refinery );
                    if (refinery.Building.UnitNearBuildingBorder(fUnit_Comp))
                    {
                        if (!refinery.TryLoadHarvester(this))
                            SendToFreeSpiceRefinery();
                    }
                    else
                    {
                        if (refinery.Building.LoadingPlatform.HasFreeSpace)
                            fUnit_Comp.Owner.CarrierManager.Request_LoadObjectToPlatform(fUnit_Comp, refinery.Building.LoadingPlatform);
                        else
                            SendToFreeSpiceRefinery();
                    }
                }

                break;
        }
    }

    public bool Event_PerformOrder(OrderManager_Script sender, ref OrderManager_Script_OrderData data)
    {
        switch (data.TargetType)
        {
            case OrderManager_Script_OrderTargetType.Object:
                SpiceRefinery_Script refinery = data.Unit.GetComponent<SpiceRefinery_Script>();

                if ((refinery != null) &&
                    (refinery.Building.LoadingPlatform.HasFreeSpace))
                {
                    SetAimingRefinery(refinery);
                }

                break;
        }

        return true;
    }

    public void Event_OrderCancel(OrderManager_Script sender, OrderManager_Script_OrderData data)
    {
        SetAimingRefinery(null);
    }

    protected void SetAimingRefinery(SpiceRefinery_Script refinery)
    {
        if (fAimingRefinery != null)
        {
            fAimingRefinery.Building.LoadingPlatform.StopLandingLightsAnim(this);
        }

        fAimingRefinery = refinery;

        if (fAimingRefinery != null)
        {
            fAimingRefinery.Building.LoadingPlatform.PlayLandingLightsAnim(this);
        }
    }

    public void Event_OnDie(UnitObject_Script sender)
    {
        GlobalCollector.Instance.Current_Map.DistributeSpice(fUnit_Comp.Position, fHarvestedSpice.Value);
    }

    IEnumerator DoProcessUnload()
    {
        yield return null;

        if (!fUnit_Comp.OnPlatform)
        {
            DoHarvest();
        }
    }
    
    public void GlobalEvent_OnCarrierUnloadObject(CarrierCapability_Script sender, UnitObject_Script _object)
    {
        if ((_object == fUnit_Comp))
        {
            StartCoroutine(DoProcessUnload());
        }
    }

    protected UnitObject_Script FindAllowedSpiceRefinery()
    {
        fUnit_Comp.Owner.Units.MatchUnitsByTypeName(REFINERY_TYPE_NAME, ref fBufferGroup);
        UnitObject_Script result = null;
        SpiceRefinery_Script refinery;

        foreach (UnitObject_Script unit in fBufferGroup)
        {
            refinery = unit.GetComponent<SpiceRefinery_Script>();
            if ((refinery != null) &&
                !refinery.IsBusy)
            {
                return unit;
            }
            else
                result = unit;
        }

        return result;
    }

    protected void SendToSpiceRefinery(UnitObject_Script refinery)
    {
        if (refinery == null)
            return;

        if (GlobalCollector.CheckDistanceBetweenOnCarrierLoadRadius(fUnit_Comp.Position, refinery.Position))
        {
            BuildingObject_Script bcomp = refinery.GetComponent<BuildingObject_Script>();
            fOrderData.OrderType = OrderManager_Script_OrderType.Follow;
            fOrderData.SetTargetType(refinery);
            fUnit_Comp.Owner.CarrierManager.Request_LoadObjectToPlatform(fUnit_Comp, bcomp.LoadingPlatform);
        }
        else
        {
            fUnit_Comp.Order_Follow(refinery);
        }
    }

    protected void SendToFreeSpiceRefinery()
    {
        SendToSpiceRefinery(FindAllowedSpiceRefinery());
    }

    IEnumerator DoPostInit()
    {
        yield return null;

        fHarvestedSpice.OnChanged.AddListener(Event_OnSpiceValueChanged);
        fHarvestedSpice.OnMinBorder.AddListener(Event_OnSpiceMinBorder);
        fHarvestedSpice.OnMaxBorder.AddListener(Event_OnSpiceMaxBorder);
        CarrierCapability_Script.Global_OnCarrierUnloadObject.AddListener(GlobalEvent_OnCarrierUnloadObject);
        fUnit_Comp.CompCollection.OrderManager_Comp.OnOrderComplete.AddListener(Event_OnOrderComplete);
        fUnit_Comp.OnDie.AddListener(Event_OnDie);
        fUnit_Comp.CompCollection.OrderManager_Comp.OnOrderPerform = Event_PerformOrder;
        fUnit_Comp.CompCollection.OrderManager_Comp.OnOrderCancel.AddListener(Event_OrderCancel);
        SpiceRepositoryCapacity = Default_SpiceRepositoryCapacity;
        HarvestedSpice.Value = Default_HarvestedSpice;
        HarvestingSpeed = Default_HarvestingSpeed;
    }

    private void OnDestroy()
    {
        CarrierCapability_Script.Global_OnCarrierUnloadObject.RemoveListener(GlobalEvent_OnCarrierUnloadObject);
    }

    // Use this for initialization
    void Start () {
        fUnit_Comp = this.GetComponent<UnitObject_Script>();
        StartCoroutine(DoPostInit());
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (fHarvestingCell == null)
            return;

        fHarvestingTick += Time.deltaTime;

        if (fHarvestingTick >= HARVESTING_TIME_FREQUENCY)
        {
            fHarvestingTick = 0;
            float value = fHarvestingCell.TakeSpice(HarvestingSpeed * HARVESTING_TIME_FREQUENCY);
            fHarvestingCell.ReturnSpice(value - HarvestedSpice.Add(value));
            
            if (MathKit.NumbersEquals(value, 0))
            {
                fHarvestingCell = null;
                DoHarvest();
            }
        }
    }
}


 
 */
