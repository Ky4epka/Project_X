using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Player_Script_House
{
    Unnamed = 0,
    Freeman,
    Atreides,
    Ordos,
    Harkonnen,
    Imperator
}

public enum Player_Script_SpiceAddType
{
    Harvested,
    CostReturn,
    Unknown
}

[System.Serializable]
public class Player_Script_HouseInfo
{
    public string Name = "UnnamedHouse";
    public Player_Script_House House = Player_Script_House.Unnamed;
    public Color UnitsCellColor = Color.white;
    public Material UnitsMaterial = null;
}


public class Player_Script_Statistics
{

    public void UnitConstructed()
    {

    }

    public void UnitKilled(bool self)
    {

    }
    
    public void BuildingConstructed()
    {

    }

    public void BuildingDestroyed(bool self)
    {

    }

    public void AddSpice(float value, Player_Script_SpiceAddType add_type)
    {

    }
}

[System.Serializable]
public class Player_Script_ParametersValues
{
    public Player_Script Player = null;

    public NotifyEvent<float> OnEnergyLimitChanged = new NotifyEvent<float>();
    public NotifyEvent<float> OnEnergyChanged = new NotifyEvent<float>();
    public NotifyEvent<Player_Script> OnEnergyParamsChanged = new NotifyEvent<Player_Script>();
    public NotifyEvent<float> OnSpiceLimitChanged = new NotifyEvent<float>();
    public NotifyEvent_2P<float, float> OnSpiceChanged = new NotifyEvent_2P<float, float>();


    [SerializeField]
    public ValuesCollection<UnitObject_Script> EnergyLimitRepository = new ValuesCollection<UnitObject_Script>();
    public ValuesCollection<UnitObject_Script> EnergyRepository = new ValuesCollection<UnitObject_Script>();
    public ValuesCollection<UnitObject_Script> SpiceLimitRepository = new ValuesCollection<UnitObject_Script>();


    [SerializeField]
    protected bool fEnergyOverflows = false;
    [SerializeField]
    protected bool fSpiceOverflows = false;
    [SerializeField]
    protected LimitedValueFloat fCurrentSpice = new LimitedValueFloat();

    public LimitedValueFloat CurrentSpice
    {
        get
        {
            return fCurrentSpice;
        }
    }

    public bool EnergyOverflows
    {
        get
        {
            return fEnergyOverflows;
        }
    }

    public bool SpiceOverflows
    {
        get
        {
            return fSpiceOverflows;
        }
    }

    public Player_Script_ParametersValues()
    {
        EnergyLimitRepository.OnChanged.AddListener(Event_OnEnergyLimitChanged);
        EnergyRepository.OnChanged.AddListener(Event_OnEnergyChanged);
        SpiceLimitRepository.OnChanged.AddListener(Event_OnSpiceLimitChanged);
        fCurrentSpice.OnChanged.AddListener(Event_OnSpiceChanged);
        fCurrentSpice.OnMinBorder.AddListener(Event_OnSpiceMinBorder);
        fCurrentSpice.OnMaxBorder.AddListener(Event_OnSpiceMaxBorder);
        fCurrentSpice.MinValue = 0;
        fCurrentSpice.MaxValue = SpiceLimitRepository.Repository;
        EnergyParamsChanged();
    }

    public void Event_OnEnergyLimitChanged(float value)
    {
        OnEnergyLimitChanged.Invoke(value);
        EnergyParamsChanged();
    }

    public void Event_OnEnergyChanged(float value)
    {
        OnEnergyChanged.Invoke(value);
        EnergyParamsChanged();
    }

    public void Event_OnSpiceLimitChanged(float value)
    {
        OnSpiceLimitChanged.Invoke(value);
        fCurrentSpice.MinValue = 0f;
        fCurrentSpice.MaxValue = value;
    }

    public void Event_OnSpiceChanged(float old_value, float new_value)
    {
        OnSpiceChanged.Invoke(old_value, new_value);
    }

    public void Event_OnSpiceMinBorder()
    {

    }

    public void Event_OnSpiceMaxBorder()
    {

    }

    protected void EnergyParamsChanged()
    {
        fEnergyOverflows = EnergyRepository.Repository >= EnergyLimitRepository.Repository;
        OnEnergyParamsChanged.Invoke(Player);
    }

}

[System.Serializable]
public class Player_Script_CarrierManager
{
    [SerializeField]
    protected List<CarrierCapability_Script> fCarriers = new List<CarrierCapability_Script>();
    [SerializeField]
    protected List<CarrierCapability_Script> fActiveCarriers = new List<CarrierCapability_Script>();
    [SerializeField]
    protected List<CarrierCapability_Script> fFreeCarriers = new List<CarrierCapability_Script>();
    [SerializeField]
    protected Player_Script fOwner = null;
    [SerializeField]
    protected List<CarrierCapability_Script_BaseCommandPreset> fAddressedRequestList = new List<CarrierCapability_Script_BaseCommandPreset>();
    [SerializeField]
    protected List<CarrierCapability_Script_BaseCommandPreset> fCommonRequestList = new List<CarrierCapability_Script_BaseCommandPreset>();


    public Player_Script_CarrierManager(Player_Script owner)
    {
        fOwner = owner;
        Initialize();
    }

    public CarrierCapability_Script_BaseCommandPreset Request_LoadFromPlatformToPoint(CarrierLoadingPlatform_Script platform, Vector3 point)
    {
        CarrierCapability_Script_BaseCommandPreset preset = new CarrierCapability_Script_LoadFromPlatformToPoint(null, platform, point);
        AddRequest(preset);
        return preset;
    }

    public CarrierCapability_Script_BaseCommandPreset Request_LoadObjectToPlatform(UnitObject_Script _object, CarrierLoadingPlatform_Script platform)
    {
        CarrierCapability_Script_BaseCommandPreset preset = new CarrierCapability_Script_LoadObjectToPlatform(null, _object, platform);
        AddRequest(preset);
        return preset;
    }

    public CarrierCapability_Script_BaseCommandPreset Request_OrderToPlatform(CarrierLoadingPlatform_Script platform, List<string> order_list)
    {
        CarrierCapability_Script_BaseCommandPreset preset = new CarrierCapability_Script_OrderToPlatform(null, platform, order_list);
        AddRequest(preset);
        return preset;
    }

    public CarrierCapability_Script_BaseCommandPreset Request_OrderToPoint(Vector3 point, List<string> order_list)
    {
        CarrierCapability_Script_BaseCommandPreset preset = new CarrierCapability_Script_OrderToPoint(null, point, order_list);
        AddRequest(preset);
        return preset;
    }

    public CarrierCapability_Script_BaseCommandPreset Request_TranslateObjectToPoint(UnitObject_Script _object, Vector3 point)
    {
        CarrierCapability_Script_BaseCommandPreset preset = new CarrierCapability_Script_TranslateObjectToPoint(null, _object, point);
        AddRequest(preset);
        return preset;
    }

    public CarrierCapability_Script_BaseCommandPreset Request_UnloadObjectsToPoint(CarrierCapability_Script carrier, Vector3 point)
    {
        if (!fCarriers.Contains(carrier))
            return null;

        CarrierCapability_Script_BaseCommandPreset preset = new CarrierCapability_Script_UnloadObjectsToPoint(carrier, point);
        AddRequest(preset);
        return preset;
    }

    public void AddCarrier(CarrierCapability_Script carrier)
    {
        if (fCarriers.Contains(carrier))
            return;

        if (carrier.Unit.Owner != fOwner)
            carrier.Unit.Owner = fOwner;

        fCarriers.Add(carrier);
        carrier.OnCarrierRelease.AddListener(Event_OnCarrierRelease);
        AddFreeCarrier(carrier);
    }

    public void RemoveCarrier(CarrierCapability_Script carrier)
    {
        carrier.OnCarrierRelease.RemoveListener(Event_OnCarrierRelease);
        fFreeCarriers.Remove(carrier);
        fActiveCarriers.Remove(carrier);
        fCarriers.Remove(carrier);

        for (int i = fAddressedRequestList.Count - 1; i >= 0; i--)
            if (fAddressedRequestList[i].Carrier == carrier)
                fAddressedRequestList.RemoveAt(i);
    }

    public void Event_CarrierAdd(Player_Script sender, UnitObject_Script unit)
    {
        CarrierCapability_Script carrier = unit.GetComponent<CarrierCapability_Script>();

        if (carrier != null)
            AddCarrier(carrier);
    }

    public void Event_CarrierRemove(Player_Script sender, UnitObject_Script unit)
    {
        CarrierCapability_Script carrier = unit.GetComponent<CarrierCapability_Script>();

        if (carrier != null)
            RemoveCarrier(carrier);
    }

    public void Event_OnCarrierRelease(CarrierCapability_Script sender)
    {
        AddFreeCarrier(sender);
    }

    protected IEnumerator CarrierInit(CarrierCapability_Script new_carrier)
    {
        yield return null;

        new_carrier.Unit.Owner = fOwner;
        AddCarrier(new_carrier);
    }

    protected void CarrierNeeded()
    {
        CarrierCapability_Script new_carrier = TypeObjectManager_Script.CreateObjectInstance("Carrier", Vector3.zero, Vector3.right, true).GetComponent<CarrierCapability_Script>();
        fOwner.StartCoroutine(CarrierInit(new_carrier));
    }

    protected void AddFreeCarrier(CarrierCapability_Script carrier)
    {
        fActiveCarriers.Remove(carrier);
        fFreeCarriers.Add(carrier);
        DistributeRequests();
    }
        
    protected void AddRequest(CarrierCapability_Script_BaseCommandPreset request)
    {
        if (fAddressedRequestList.Contains(request) ||
            fCommonRequestList.Contains(request))
            return;

        if (request.Carrier == null)
            fCommonRequestList.Add(request);
        else
            fAddressedRequestList.Add(request);

        DistributeRequests();
    }

    protected void RemoveRequest(CarrierCapability_Script_BaseCommandPreset request)
    {
        if (request.Carrier == null)
            fCommonRequestList.Remove(request);
        else
            fAddressedRequestList.Remove(request);
    }

    protected void DistributeRequests()
    {
        if (fCarriers.Count == 0)
        {
            CarrierNeeded();
            return;
        }

        CarrierCapability_Script carrier;
        bool addressed = false;

        while (fFreeCarriers.Count > 0)
        {
            carrier = fFreeCarriers[0];

            for (int i = 0; i < fAddressedRequestList.Count; i++)
            {
                if (fAddressedRequestList[i].Carrier == carrier)
                {
                    fAddressedRequestList[i].PushPreset();
                    fAddressedRequestList.RemoveAt(i);
                    addressed = true;
                    break;
                }
            }

            if (addressed)
            {
                fFreeCarriers.RemoveAt(0);
                fActiveCarriers.Add(carrier);
                continue;
            }

            if (fCommonRequestList.Count > 0)
            {
                fCommonRequestList[0].Carrier = carrier;
                fCommonRequestList[0].PushPreset();
                fCommonRequestList.RemoveAt(0);
                fFreeCarriers.RemoveAt(0);
                fActiveCarriers.Add(carrier);
            }
            else
                break;
        }
    }

    protected void Initialize()
    {
        fOwner.OnAddUnit.AddListener(Event_CarrierAdd);
        fOwner.OnRemoveUnit.AddListener(Event_CarrierRemove);
    }
}

public class Player_Script : MonoBehaviour {


    public string Default_Name = "default";
    public Player_Script_House Default_House = Player_Script_House.Unnamed;

    public NotifyEvent_2P<Player_Script, UnitObject_Script> OnAddUnit = new NotifyEvent_2P<Player_Script, UnitObject_Script>();
    public NotifyEvent_2P<Player_Script, UnitObject_Script> OnRemoveUnit = new NotifyEvent_2P<Player_Script, UnitObject_Script>();
    public NotifyEvent<Player_Script> OnDestroyInstance = new NotifyEvent<Player_Script>();
    public NotifyEvent_2P<Player_Script, UnitObject_Script> OnUnitSelected = new NotifyEvent_2P<Player_Script, UnitObject_Script>();
    public NotifyEvent_2P<Player_Script, UnitObject_Script> OnUnitUnselected = new NotifyEvent_2P<Player_Script, UnitObject_Script>();
    public NotifyEvent<Player_Script> OnSelectionChanged = new NotifyEvent<Player_Script>();
    public NotifyEvent_2P<Player_Script, Player_Script_House> OnHouseChanged = new NotifyEvent_2P<Player_Script, Player_Script_House>();

    [SerializeField]
    protected string fName = "no_name";
    [SerializeField]
    protected Player_Script_House fHouse = Player_Script_House.Freeman;
    [SerializeField]
    protected Player_Script_HouseInfo fHouseInfo = null;
    [SerializeField]
    protected PlayerGroup_Script fAllies = new PlayerGroup_Script();
    [SerializeField]
    protected UnitGroup_Script fSelected = new UnitGroup_Script();
    [SerializeField]
    protected UnitGroup_Script fUnits = new UnitGroup_Script();
    [SerializeField]
    protected Player_Script_Statistics fStatistics = new Player_Script_Statistics();
    [SerializeField]
    protected Player_Script_ParametersValues fParametersValues = new Player_Script_ParametersValues();
    [SerializeField]
    protected Player_Script_CarrierManager fCarrierManager = null;
    [SerializeField]
    protected bool fInit = true;

    public string PlayerName
    {
        get
        {
            return fName;
        }

        set
        {
            fName = value;
        }
    }

    public Player_Script_House House
    {
        get
        {
            return fHouse;
        }

        set
        {
            fHouse = value;
            fHouseInfo = GlobalCollector.Instance.HouseInfoById(value);
            OnHouseChanged.Invoke(this, fHouse);
        }
    }

    public Player_Script_HouseInfo HouseInfo
    {
        get
        {
            return fHouseInfo;
        }
    }

    public Player_Script_Statistics Statistics
    {
        get
        {
            return fStatistics;
        }
    }

    public Player_Script_ParametersValues ParametersValues
    {
        get
        {
            return fParametersValues;
        }
    }

    public Player_Script_CarrierManager CarrierManager
    {
        get
        {
            return fCarrierManager;
        }
    }

    public bool IsAlly(Player_Script player)
    {
        return fAllies.Contains(player) && (player != this);
    }

    public bool IsEnemy(Player_Script player)
    {
        return !IsAlly(player) && (player != this);
    }

    public void SelectUnit(UnitObject_Script unit)
    {
        if (IsUnitSelected(unit))
            return;

        if (
            fSelected.Count > 0
// Disabled for beta test long
//            (unit.IsBuilding() && !IsSelectedBuildings()) ||
//            (!unit.IsBuilding() && IsSelectedBuildings()) ||
//            (!IsSelectedSelfUnits())
           )
        {
            ClearSelection();
        }

        if (unit.Selectable)
        {
            unit.SetSelected(this, true);
            fSelected.Add(unit);
            OnUnitSelected.Invoke(this, unit);
            OnSelectionChanged.Invoke(this);
        }
    }

    public void UnselectUnit(UnitObject_Script unit)
    {
        if (fSelected.Remove(unit))
        {
            unit.SetSelected(this, false);
            OnUnitUnselected.Invoke(this, unit);
            OnSelectionChanged.Invoke(this);
        }
    }

    public UnitGroup_Script Selected
    {
        get
        {
            return fSelected;
        }
    }

    public int SelectedCount
    {
        get
        {
            return fSelected.Count;
        }
    }

    public void ClearSelection()
    {
        while (fSelected.Count > 0)
            UnselectUnit(fSelected[0]);
    }

    public UnitObject_Script FirstSelected
    {
        get
        {
            return fSelected.First;
        }
    }

    public UnitObject_Script LastSelected
    {
        get
        {
            return fSelected.Last;
        }
    }

    public UnitGroup_Script Units
    {
        get
        {
            return fUnits;
        }
    }

    public bool IsUnitSelected(UnitObject_Script unit)
    {
        return fSelected.Contains(unit);
    }

    public bool HasSelected()
    {
        return fSelected.Count > 0;
    }

    public bool IsSelectedBuildings()
    {
        foreach (UnitObject_Script unit in fSelected)
            if (!unit.IsBuilding())
                return false;

        return true;
    }

    public bool IsSelectedSelfUnits()
    {
        foreach (UnitObject_Script unit in fSelected)
            if (unit.Owner != this)
                return false;

        return true;
    }

    public bool IsSelectedAllyUnits()
    {
        foreach (UnitObject_Script unit in fSelected)
            if (!IsAlly(unit.Owner))
                return false;

        return true;
    }

    public bool IsSelectedEnemyUnits()
    {
        foreach (UnitObject_Script unit in fSelected)
            if (IsAlly(unit.Owner) || 
                (unit.Owner == this))
                return false;

        return true;
    }

    public void AddUnit(UnitObject_Script unit)
    {
        if (fUnits.Contains(unit))
            return;

        fUnits.Add(unit);
        OnAddUnit.Invoke(this, unit);
    }

    public void RemoveUnit(UnitObject_Script unit)
    {
        fSelected.Remove(unit);
        if (fUnits.Remove(unit))
        {
            OnRemoveUnit.Invoke(this, unit);
        }
    }

    private void OnDestroy()
    {
        OnDestroyInstance.Invoke(this);
    }

    private void Awake()
    {
        fCarrierManager = new Player_Script_CarrierManager(this);
    }

    IEnumerator DoInit()
    {
        yield return null;

        House = Default_House;
    }

    // Use this for initialization
    void Start () {
        ParametersValues.Player = this;
        PlayerName = Default_Name;
        StartCoroutine(DoInit());
	}
	
	// Update is called once per frame
	void Update () {
	}
}
