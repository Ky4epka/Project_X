using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CarrierLoadingPlatform_Script_TypeListUse
{
    AllowedList,
    BannedList,
    AllTypes
}

[RequireComponent(typeof(GameObject_Script))]
public class CarrierLoadingPlatform_Script : MonoBehaviour
{
    public delegate bool UnloadNearBuildingHasFreeSpaceDelegate(CarrierLoadingPlatform_Script sender);
    public delegate bool UnloadNearBuildingNoFreeSpaceDelegate(CarrierLoadingPlatform_Script sender, Vector3 recommended_point);

    public BuildingObject_Script Direct_Building_Comp = null;
    public Transform Direct_Pivot = null;
    public Transform Direct_UnloadingPosition = null;
    public int Default_LoadLimit = 0;
    public bool Default_HasLandingLightsAnim = false;
    public bool Default_VisibleLoad = false;
    public List<string> Default_TypeList = new List<string>();
    public CarrierLoadingPlatform_Script_TypeListUse Default_TypeListUse = CarrierLoadingPlatform_Script_TypeListUse.AllowedList;

    public NotifyEvent_2P<CarrierLoadingPlatform_Script, UnitObject_Script> OnLoadObject = new NotifyEvent_2P<CarrierLoadingPlatform_Script, UnitObject_Script>();
    public NotifyEvent_2P<CarrierLoadingPlatform_Script, UnitObject_Script> OnUnloadObject = new NotifyEvent_2P<CarrierLoadingPlatform_Script, UnitObject_Script>();
    public NotifyEvent<CarrierLoadingPlatform_Script> OnDestroy = new NotifyEvent<CarrierLoadingPlatform_Script>();

    public UnloadNearBuildingHasFreeSpaceDelegate UnloadNearBuildingHasFreeSpace = null;
    public UnloadNearBuildingNoFreeSpaceDelegate UnloadNearBuildingNoFreeSpace = null;

    [SerializeField]
    protected GameObject_Script fGameObject_Comp = null;
    [SerializeField]
    protected BuildingObject_Script fBuilding_Comp = null;
    [SerializeField]
    protected List<UnitObject_Script> fLoadList = new List<UnitObject_Script>();
    [SerializeField]
    protected List<string> fTypeList = new List<string>();
    [SerializeField]
    protected CarrierLoadingPlatform_Script_TypeListUse fTypeListUse = CarrierLoadingPlatform_Script_TypeListUse.AllowedList;
    [SerializeField]
    protected List<UnitObject_Script> fTakeAimList = new List<UnitObject_Script>();
    [SerializeField]
    protected int fLoadLimit = 0;
    [SerializeField]
    protected bool fHasLandingLightsAnim = false;
    [SerializeField]
    protected bool fVisibleLoad = false;
    [SerializeField]
    protected List<object> fLandingLightsAnimLockList = new List<object>();

    protected const int fBufferCapacity = GlobalCollector.LoadingPlatform_FreeCellToUnloadRadius * GlobalCollector.LoadingPlatform_FreeCellToUnloadRadius * 4;
    protected static MapCell_Script [] fCellsBuffer = new MapCell_Script[fBufferCapacity];
    protected static int fCellsBufferFactSize = 0;
    protected static List<Vector3> fVectorsBufferList = new List<Vector3>(fBufferCapacity);


    public virtual UnitObject_Script PopObjectToCarrier(CarrierCapability_Script carrier)
    {
        UnitObject_Script temp = PopObjectFromPlatform();
        carrier.AddLoadObject(temp);

        return temp;
    }

    public virtual bool PushObjectToPlatform(UnitObject_Script _object)
    {
        if ((_object == null) ||
            ((fLoadList.Count >= fLoadLimit) && (fLoadLimit != 0)) ||
            fLoadList.Contains(_object))
            return false;

        _object.OnPlatform = true;
        _object.Visible = fVisibleLoad;
        _object.Position = fGameObject_Comp.Body.position;
        _object.Direction = Direct_Pivot.right;
        fLoadList.Add(_object);

        OnLoadObject.Invoke(this, _object);
        return true;
    }

    public virtual UnitObject_Script PopObjectFromPlatform()
    {
        if (fLoadList.Count == 0)
            return null;

        UnitObject_Script _object = fLoadList[0];
        fLoadList.RemoveAt(0);
        _object.OnPlatform = false;
        _object.Visible = true;
        OnUnloadObject.Invoke(this, _object);
        
        if (fTakeAimList.Count > 0)
            for (int i=0; i<fTakeAimList.Count; i++)
                fTakeAimList[i].Order_Follow(fBuilding_Comp.Unit);

        return _object;
    }

    public virtual void PushObjectListToPlatformFromCarrier(CarrierCapability_Script carrier)
    {
        UnitObject_Script _object = null;

        do
        {
            if (HasFreeSpace)
            {
                _object = carrier.UnloadFirstObject();
                PushObjectToPlatform(_object);
            }
            else
            {
                fCellsBufferFactSize = GlobalCollector.Instance.Current_Map.FindAllowedCelllsToGroundObjectAroundPointInRadius(fGameObject_Comp.Body.position,
                                                                                                                               GlobalCollector.LoadingPlatform_FreeCellToUnloadRadius,
                                                                                                                               fCellsBuffer);
                
                if (fCellsBufferFactSize > 0)
                {
                    fBuilding_Comp.Unit.Owner.CarrierManager.Request_UnloadObjectsToPoint(carrier, fCellsBuffer[0].WordlPosition);
                }
                else
                {
                    carrier.DestroyLoad();
                }
            }
        } while (_object != null);
    }

    public virtual void PopObjectListFromPlatformToCarrier(CarrierCapability_Script carrier)
    {
        UnitObject_Script _object = null;

        do
        {
            _object = PopObjectFromPlatform();
            carrier.AddLoadObject(_object);
        } while (_object != null);
    }

    public virtual void PushObjectListToPlatform(List<UnitObject_Script> list)
    {
        foreach (UnitObject_Script _object in list)
            PushObjectToPlatform(_object);
    }

    public virtual void PopObjectListToCarrier(CarrierCapability_Script carrier)
    {
        while (fLoadList.Count > 0)
            PopObjectToCarrier(carrier);
    }

    public virtual UnitObject_Script UnloadFirstNearBuilding()
    {
        UnitObject_Script _object = null;
        List<MapCell_Script> cells = Building_Comp.FindAllowedCellsAround();

        if (cells == null)
        {
            fCellsBufferFactSize = GlobalCollector.Instance.Current_Map.FindAllowedCelllsToGroundObjectAroundPointInRadius(fGameObject_Comp.Body.position,
                                                                                                                           GlobalCollector.LoadingPlatform_FreeCellToUnloadRadius,
                                                                                                                           fCellsBuffer);

            if (fCellsBufferFactSize > 0)
            { 

                if ((UnloadNearBuildingNoFreeSpace == null) ||
                    !UnloadNearBuildingNoFreeSpace(this, fCellsBuffer[0].WordlPosition))
                    fBuilding_Comp.Unit.Owner.CarrierManager.Request_LoadFromPlatformToPoint(this, fCellsBuffer[0].WordlPosition);
            }
            else
            {
                return null;
            }
        }
        else
        {
            if ((UnloadNearBuildingHasFreeSpace == null) ||
                !UnloadNearBuildingHasFreeSpace(this))
            {
                fVectorsBufferList.Clear();
                for (int i = 0; i < cells.Count; i++)
                {
                    fVectorsBufferList.Add(cells[i].WordlPosition);
                }

                Vector3 pos = fGameObject_Comp.Position;

                if (Direct_UnloadingPosition != null)
                    pos = Direct_UnloadingPosition.position;

                Kits.SortVector3OnNearestPoint(fVectorsBufferList, pos);
                _object = PopObjectFromPlatform();
                _object.Position = fVectorsBufferList[0];
            }
        }

        return _object;
    }


    public virtual void UnloadAllNearBuilding()
    {
        UnitObject_Script _object = UnloadFirstNearBuilding();

        while (_object != null)
            _object = UnloadFirstNearBuilding();
    }

    public virtual void PlayLandingLightsAnim(object lock_object)
    {
        if (!fHasLandingLightsAnim)
            return;

        try
        {
            if (fLandingLightsAnimLockList.Count == 0)
                fBuilding_Comp.Unit.CompCollection.AnimController_Comp.SetAnimationId(AnimationController_Script_AnimationId.Platform, true);
        }
        finally
        {
            if (!fLandingLightsAnimLockList.Contains(lock_object))
                fLandingLightsAnimLockList.Add(lock_object);
        }
    }

    public virtual void StopLandingLightsAnim(object lock_object)
    {
        if (!fHasLandingLightsAnim)
            return;

        try
        {
            if ((fLandingLightsAnimLockList.Count == 1) &&
                (fBuilding_Comp.Unit.CompCollection.AnimController_Comp != null))
                fBuilding_Comp.Unit.CompCollection.AnimController_Comp.SetAnimationId(AnimationController_Script_AnimationId.None, false);
        }
        finally
        {
            fLandingLightsAnimLockList.Remove(lock_object);
        }
    }

    public virtual void DestroyLoad()
    {
        foreach (UnitObject_Script load in fLoadList)
        {
            load.Kill();
        }

        fLoadList.Clear();
    }

    public virtual Vector3 Position
    {
        get
        {
            return Direct_Pivot.position;
        }
    }

    public virtual Vector3 Direction
    {
        get
        {
            return Direct_Pivot.right;
        }
    }

    public virtual bool HasLoad
    {
        get
        {
            return fLoadList.Count > 0;
        }
    }

    public virtual bool HasFreeSpace
    {
        get
        {
            return fLoadLimit - fLoadList.Count > 0;
        }
    }
    
    public virtual BuildingObject_Script Building_Comp
    {
        get
        {
            return fBuilding_Comp;
        }

        set
        {
            if (fBuilding_Comp != null)
            {
                fBuilding_Comp.LoadingPlatform = null;
                fBuilding_Comp.Unit.OnDie.RemoveListener(Event_OnDie);
            }

            fBuilding_Comp = value;

            if (fBuilding_Comp != null)
            {
                fBuilding_Comp.LoadingPlatform = this;
                fBuilding_Comp.Unit.OnDie.AddListener(Event_OnDie);
            }
        }
    }

    public virtual bool HasLandingLightsAnim
    {
        get
        {
            return fHasLandingLightsAnim;
        }
        
        set
        {
            fHasLandingLightsAnim = value;

            if (value)
                fLandingLightsAnimLockList.Clear();
        }
    }

    public virtual List<UnitObject_Script> LoadList
    {
        get
        {
            return fLoadList;
        }
    }

    public virtual int LoadLimit
    {
        get
        {
            return fLoadLimit;
        }

        set
        { 
            if (fLoadLimit == value)
                return;
            
            for (int i=fLoadList.Count - 1; i>=fLoadLimit; i--)
            {
                UnitObject_Script _object = fLoadList[i];
                _object.Kill();
                fLoadList.RemoveAt(fLoadList.Count);
            }

            fLoadLimit = value;
        }
    }

    public virtual bool VisibleLoad
    {
        get
        {
            return fVisibleLoad;
        }

        set
        {
            if (fVisibleLoad == value)
                return;

            fVisibleLoad = value;

            foreach (UnitObject_Script _object in fLoadList)
            {
                _object.Visible = value;
            }
        }
    }

    public virtual List<string> TypeList
    {
        get
        {
            return fTypeList;
        }

        set
        {
            fTypeList.Clear();
            fTypeList.AddRange(value);
        }
    }

    public virtual CarrierLoadingPlatform_Script_TypeListUse TypeListUse
    {
        get
        {
            return fTypeListUse;
        }

        set
        {
            fTypeListUse = value;
        }
    }

    public void Event_OnDie(UnitObject_Script sender)
    {
        DestroyLoad();
    }



    // Use this for initialization
    protected virtual void Start()
    {
        fGameObject_Comp = this.GetComponent<GameObject_Script>();
        Building_Comp = Direct_Building_Comp;
        LoadLimit = Default_LoadLimit;
        HasLandingLightsAnim = Default_HasLandingLightsAnim;
        VisibleLoad = Default_VisibleLoad;
        TypeList = Default_TypeList;
        TypeListUse = Default_TypeListUse;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
