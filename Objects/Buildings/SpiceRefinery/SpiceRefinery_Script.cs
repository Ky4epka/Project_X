using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BuildingObject_Script))]
public class SpiceRefinery_Script : MonoBehaviour {


    public static NotifyEvent_2P<SpiceRefinery_Script, Harvester_Script> Global_OnHarvesterLoadedToRefinery = new NotifyEvent_2P<SpiceRefinery_Script, Harvester_Script>();
    public static NotifyEvent_2P<SpiceRefinery_Script, Harvester_Script> Global_OnHarvesterUnloadedFromRefinery = new NotifyEvent_2P<SpiceRefinery_Script, Harvester_Script>();


    public float Default_HarvesterEmptyingSpeed = 1;

    public NotifyEvent_2P<SpiceRefinery_Script, Harvester_Script> OnHarvesterLoadedToRefinery = new NotifyEvent_2P<SpiceRefinery_Script, Harvester_Script>();
    public NotifyEvent_2P<SpiceRefinery_Script, Harvester_Script> OnHarvesterUnloadedFromRefinery = new NotifyEvent_2P<SpiceRefinery_Script, Harvester_Script>();


    protected List<Harvester_Script> fLoadedHarvesters = new List<Harvester_Script>();
    [SerializeField]
    protected BuildingObject_Script fBuilding_Comp = null;
    [SerializeField]
    protected SpiceDependentObject_Script fSpiceDependtent_Comp = null;
    [SerializeField]
    protected float fHarvesterEmptyingSpeed = 1;
    [SerializeField]
    protected float fHarvesterEmptyingTick = 0;

    protected static float HARVESTER_EMPTYING_FREQUENCY = 0.1f;


    public float HarvesterEmptyingSpeed
    {
        get
        {
            return fHarvesterEmptyingSpeed;
        }

        set
        {
            if (value == fHarvesterEmptyingSpeed)
                return;

            fHarvesterEmptyingSpeed = value;

            if (fHarvesterEmptyingSpeed < 1)
                fHarvesterEmptyingSpeed = 1;
        }
    }

    public BuildingObject_Script Building
    {
        get
        {
            return fBuilding_Comp;
        }
    }

    public bool TryLoadHarvester(Harvester_Script harvester)
    {
        if (fBuilding_Comp.LoadingPlatform.HasFreeSpace)
        {
            fBuilding_Comp.LoadingPlatform.PushObjectToPlatform(harvester.Unit);
            return true;
        }

        return false;
    }

    public void Event_OnHarvesterStorageEmpty(Harvester_Script sender)
    {
        fBuilding_Comp.LoadingPlatform.UnloadFirstNearBuilding();
    }

    public void Event_OnPlatformLoadObject(CarrierLoadingPlatform_Script sender, UnitObject_Script _object)
    {
        Harvester_Script harvester = _object.GetComponent<Harvester_Script>();

        if (harvester == null)
            return;

        harvester.OnSpiceStorageEmpty.AddListener(Event_OnHarvesterStorageEmpty);
        OnHarvesterLoadedToRefinery.Invoke(this, harvester);
        Global_OnHarvesterLoadedToRefinery.Invoke(this, harvester);
        fLoadedHarvesters.Add(harvester);
    }

    IEnumerator OnUnload(Harvester_Script harvester)
    {
        yield return null;

        OnHarvesterUnloadedFromRefinery.Invoke(this, harvester);
        Global_OnHarvesterUnloadedFromRefinery.Invoke(this, harvester);
        harvester.DoHarvest();
    }

    public void Event_OnPlatformUnloadObject(CarrierLoadingPlatform_Script sender, UnitObject_Script _object)
    {
        Harvester_Script harvester = _object.GetComponent<Harvester_Script>();

        if (harvester == null)
            return;

        harvester.OnSpiceStorageEmpty.RemoveListener(Event_OnHarvesterStorageEmpty);
        StartCoroutine(OnUnload(harvester));

        fLoadedHarvesters.Remove(harvester);
    }
    
    public bool Delegate_UnloadNearBuildingHasFreeSpace(CarrierLoadingPlatform_Script sender)
    {
        MapCell_Script spice_cell = Harvester_Script.FindSpiceCell(sender.Position, null);

        if (spice_cell == null)
            return false;

        if (GlobalCollector.CheckDistanceBetweenOnCarrierLoadRadius(sender.Position, spice_cell.WordlPosition))
        {
            fBuilding_Comp.Unit.Owner.CarrierManager.Request_LoadFromPlatformToPoint(sender, spice_cell.WordlPosition);
            return true;
        }

        return false;
    }

    public bool Delegate_UnloadNearBuildingNoFreeSpace(CarrierLoadingPlatform_Script sender, Vector3 recommended_point)
    {
        MapCell_Script spice_cell = Harvester_Script.FindSpiceCell(sender.Position, null);

        if (spice_cell != null)
            return false;

        fBuilding_Comp.Unit.Owner.CarrierManager.Request_LoadFromPlatformToPoint(sender, spice_cell.WordlPosition);
        return true;
    }

    public bool IsBusy
    {
        get
        {
            return !fBuilding_Comp.LoadingPlatform.HasFreeSpace;
        }
    }

    void Order_Harvester()
    {
        List<string> order_list = new List<string>();
        order_list.Add("Harvester");
        fBuilding_Comp.Unit.Owner.CarrierManager.Request_OrderToPlatform(fBuilding_Comp.LoadingPlatform, order_list);
    }

    IEnumerator DoPostInit()
    {
        yield return null;
        fSpiceDependtent_Comp = GetComponent<SpiceDependentObject_Script>();
        HarvesterEmptyingSpeed = Default_HarvesterEmptyingSpeed;
        fBuilding_Comp.LoadingPlatform.OnLoadObject.AddListener(Event_OnPlatformLoadObject);
        fBuilding_Comp.LoadingPlatform.OnUnloadObject.AddListener(Event_OnPlatformUnloadObject);
        fBuilding_Comp.LoadingPlatform.UnloadNearBuildingHasFreeSpace = Delegate_UnloadNearBuildingHasFreeSpace;
        fBuilding_Comp.LoadingPlatform.UnloadNearBuildingNoFreeSpace = Delegate_UnloadNearBuildingNoFreeSpace;
        Order_Harvester();
    }

	// Use this for initialization
	void Start () {
        fBuilding_Comp = this.GetComponent<BuildingObject_Script>();
        StartCoroutine(DoPostInit());
	}
	
	// Update is called once per frame
	void Update () {
        fHarvesterEmptyingTick += Time.deltaTime;

		if (fHarvesterEmptyingTick >= HARVESTER_EMPTYING_FREQUENCY)
        {
            fHarvesterEmptyingTick = 0;

            for (int i = fLoadedHarvesters.Count - 1; i >= 0; i--)
            {
                Harvester_Script harvester = fLoadedHarvesters[i];
                
                if (MathKit.NumbersEquals(harvester.HarvestedSpice.Value, 0))
                {
                    Event_OnHarvesterStorageEmpty(harvester);
                    fLoadedHarvesters.Remove(harvester);
                }
                else
                {
                    float value = harvester.HarvestedSpice.Sub(fHarvesterEmptyingSpeed * HARVESTER_EMPTYING_FREQUENCY);
                    LimitedValueFloat cur_spice = fBuilding_Comp.Unit.Owner.ParametersValues.CurrentSpice;
                    cur_spice.Add(value);
                }
            }
        }
	}
}
