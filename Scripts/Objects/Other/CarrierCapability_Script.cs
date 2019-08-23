using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CarrierCapability_Script_CommandType
{
    None,
    LoadObject,
    LoadObjectFromLoadingPlatform,
    UnloadObjectToPoint,
    UnloadObjectToLoadingPlatform
}

public enum CarrierCapability_Script_NotifyOrderType
{
    None,
    Point,
    Object,
    Platform
}

public struct CarrierCapability_Script_NotifyOrderStruct
{
    public CarrierCapability_Script_NotifyOrderType OrderType;
    public Vector3 Point;
    public UnitObject_Script Object;
    public CarrierLoadingPlatform_Script Platform;
    public object Sender;


    public CarrierCapability_Script_NotifyOrderStruct(object sender)
    {
        OrderType = CarrierCapability_Script_NotifyOrderType.None;
        Point = Vector3.zero;
        Object = null;
        Platform = null;
        Sender = sender;
    }

    public CarrierCapability_Script_NotifyOrderStruct(object sender, Vector3 point)
    {
        OrderType = CarrierCapability_Script_NotifyOrderType.Point;
        Point = point;
        Object = null;
        Platform = null;
        Sender = sender;
    }

    public CarrierCapability_Script_NotifyOrderStruct(object sender, UnitObject_Script _object)
    {
        OrderType = CarrierCapability_Script_NotifyOrderType.Object;
        Point = Vector3.zero;
        Object = _object;
        Platform = null;
        Sender = sender;
    }

    public CarrierCapability_Script_NotifyOrderStruct(object sender, CarrierLoadingPlatform_Script platform)
    {
        OrderType = CarrierCapability_Script_NotifyOrderType.Platform;
        Point = Vector3.zero;
        Object = null;
        Platform = platform;
        Sender = sender;
    }
    
}

[System.Serializable]
public class CarrierCapability_Script_BaseCommand
{
        public NotifyEvent<CarrierCapability_Script_BaseCommand> OnCompleted = new NotifyEvent<CarrierCapability_Script_BaseCommand>();
        public NotifyEvent<CarrierCapability_Script_BaseCommand> OnCancel = new NotifyEvent<CarrierCapability_Script_BaseCommand>();

    [SerializeField]
    protected CarrierCapability_Script fCarrier = null;
    [SerializeField]
    protected bool fCompleted = false;
    [SerializeField]
    protected bool fIgnoreOrderCancel = false;
    

    public virtual CarrierCapability_Script Carrier
    {
        get
        {
            return fCarrier;
        }

        set
        {
            fCarrier = value;
        }
    }

    public virtual bool Completed
    {
        get
        {
            return fCompleted;
        }
    }

    public virtual void Perform()
    {
        Debug.Log(this+"_Pefrorm");
        fCarrier.OnOrderComplete.AddListener(Event_OnOrderComplete);
        fCarrier.OnOrderCanceled.AddListener(Event_OnOrderCanceled);
        fCarrier.OnCarrierDie.AddListener(Event_OnCarrierDie);
        fCompleted = false;
    }

    public virtual void Cancel()
    {
        fCarrier.OnOrderComplete.RemoveListener(Event_OnOrderComplete);
        fCarrier.OnOrderCanceled.RemoveListener(Event_OnOrderCanceled);
        fCarrier.OnCarrierDie.RemoveListener(Event_OnCarrierDie);

        if (!fCompleted)
            OnCancel.Invoke(this);
    }

    protected virtual void OrderComplete(CarrierCapability_Script_NotifyOrderStruct data)
    {
        OnCompleted.Invoke(this);
        fCompleted = true;
        Cancel();
    }

    protected virtual void OrderCanceled(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (fIgnoreOrderCancel)
            return;

        Cancel();
        OnCancel.Invoke(this);
    }

    protected virtual void CarrierDie()
    {
        Cancel();
    }

    public void Event_OnOrderComplete(CarrierCapability_Script sender, CarrierCapability_Script_NotifyOrderStruct data)
    {
        OrderComplete(data);
    }

    public void Event_OnOrderCanceled(CarrierCapability_Script sender, CarrierCapability_Script_NotifyOrderStruct data)
    {
        OrderCanceled(data);
    }

    public void Event_OnCarrierDie(CarrierCapability_Script sender)
    {
        CarrierDie();
    }
}

[System.Serializable]
public class CarrierCapability_Script_PickupObjectCommand: CarrierCapability_Script_BaseCommand
{
    [SerializeField]
    protected UnitObject_Script fTargetObject = null;

    public CarrierCapability_Script_PickupObjectCommand(CarrierCapability_Script carrier, UnitObject_Script target_object)
    {
        Carrier = carrier;
        TargetObject = target_object;
    }

    public UnitObject_Script TargetObject
    {
        get
        {
            return fTargetObject;
        }

        set
        {
            fTargetObject = value;
        }
    }

    public override void Perform()
    {
        base.Perform();

        if (fTargetObject.IsBuilding())
            Cancel();

        fTargetObject.OnDie.AddListener(Event_OnTargetDie);
        fIgnoreOrderCancel = true;
        fCarrier.FollowToObject(fTargetObject, this);
        fIgnoreOrderCancel = false;
    }

    public override void Cancel()
    {
        fTargetObject.OnDie.RemoveListener(Event_OnTargetDie);
        base.Cancel();
    }

    public void Event_OnTargetDie(UnitObject_Script sender)
    {
        Cancel();
    }

    protected override void OrderComplete(CarrierCapability_Script_NotifyOrderStruct data)
    {
        Debug.Log("OrderComplete: " + this + ", " + data.Sender);
        if (data.Sender != this)
            return;

        Debug.Log("OrderComplete2: " + this);
        fCarrier.AddLoadObject(data.Object);
        base.OrderComplete(data);
    }

    protected override void OrderCanceled(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        base.OrderCanceled(data);
    }

}

[System.Serializable]
public class CarrierCapability_Script_UnloadObjectToPointCommand: CarrierCapability_Script_BaseCommand
{
    [SerializeField]
    protected Vector3 fPoint = Vector3.zero;

    [SerializeField]
    protected static MapCell_Script[] fCellsBuffer = null;
    [SerializeField]
    protected static int fCellsBufferLength = 0;

    public CarrierCapability_Script_UnloadObjectToPointCommand(CarrierCapability_Script carrier, Vector3 target_point)
    {
        Carrier = carrier;
        Point = target_point;
    }

    public Vector3 Point
    {
        get
        {
            return fPoint;
        }

        set
        {
            fPoint = value;
        }
    }

    public override void Perform()
    {
        base.Perform();
        DoFollow(fPoint);
    }

    public override void Cancel()
    {
        base.Cancel();
    }

    protected virtual void DoFollow(Vector3 point)
    {
        fIgnoreOrderCancel = true;

        if (GlobalCollector.Instance.Current_Map.IsCellAllowedToGroundObjects(fPoint))
        {
            fPoint = point;
        }
        else
        {
            if (!GetFreePointAround(fPoint, out point))
            {
                fCarrier.DestroyLoad();
                Cancel();
                return;
            }

            fPoint = point;
        }

        fCarrier.FollowToPoint(point, this);
        fIgnoreOrderCancel = false;
    }

    protected override void CarrierDie()
    {
        base.CarrierDie();
    }

    protected override void OrderComplete(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        if (!fCarrier.HasLoad)
        {
            Cancel();
            return;
        }

        if (!GlobalCollector.Instance.Current_Map.IsCellAllowedToGroundObjects(fPoint))
        {
            base.OrderComplete(data);
            fCarrier.Unit.Owner.CarrierManager.Request_UnloadObjectsToPoint(fCarrier, fPoint);
            return;
        }

        if (fCarrier.CarriedObjects.Count > 0)
        {
            base.OrderComplete(data);
            fCarrier.UnloadFirstObject();

            if (fCarrier.HasLoad)
            {
                fCarrier.Unit.Owner.CarrierManager.Request_UnloadObjectsToPoint(fCarrier, fPoint);
            }
        }
        else
        {
            Cancel();
        }
    }

    protected bool GetFreePointAround(Vector3 at, out Vector3 result)
    {
        GetFreePointsAround(at);

        if (fCellsBufferLength > 0)
            result = fCellsBuffer[0].WordlPosition;
        else
            result = Vector3.zero;

        return fCellsBufferLength > 0;
    }

    protected void GetFreePointsAround(Vector3 at)
    {
        if (fCellsBuffer == null)
        {
            fCellsBuffer = new MapCell_Script[GlobalCollector.Carrier_UnloadRadius * GlobalCollector.Carrier_UnloadRadius * 4 + 1];
            fCellsBufferLength = 0;
        }

        fCellsBufferLength = GlobalCollector.Instance.Current_Map.FindAllowedCelllsToGroundObjectAroundPointInRadius(at, GlobalCollector.Carrier_UnloadRadius, fCellsBuffer);
    }

    protected override void OrderCanceled(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        base.OrderCanceled(data);
    }
}

public class CarrierCapability_Script_LoadObjectsFromPlatformCommand: CarrierCapability_Script_BaseCommand
{
    protected CarrierLoadingPlatform_Script fPlatform = null;

    public CarrierCapability_Script_LoadObjectsFromPlatformCommand(CarrierCapability_Script carrier, CarrierLoadingPlatform_Script platform)
    {
        fCarrier = carrier;
        fPlatform = platform;
    }

    public override void Perform()
    {
        base.Perform();
        fPlatform.OnDestroy.AddListener(Event_OnPlatformDestroy);
        DoFollow(fPlatform);
    }

    public override void Cancel()
    {
        fPlatform.OnDestroy.RemoveListener(Event_OnPlatformDestroy);
        base.Cancel();
    }

    public void DoFollow(CarrierLoadingPlatform_Script platform)
    {
        fIgnoreOrderCancel = true;
        fPlatform = platform;
        fCarrier.FollowToPlatform(platform, this);
        fIgnoreOrderCancel = false;
    }

    public void Event_OnPlatformDestroy(CarrierLoadingPlatform_Script sender)
    {
        Cancel();
    }

    protected override void CarrierDie()
    {
        base.CarrierDie();
    }

    protected override void OrderComplete(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        fPlatform.PopObjectListToCarrier(fCarrier);
        base.OrderComplete(data);
    }

    protected override void OrderCanceled(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        base.OrderCanceled(data);
    }
}

public class CarrierCapability_Script_UnloadObjectsToPlatformCommand : CarrierCapability_Script_BaseCommand
{
    protected CarrierLoadingPlatform_Script fPlatform = null;

    public CarrierCapability_Script_UnloadObjectsToPlatformCommand(CarrierCapability_Script carrier, CarrierLoadingPlatform_Script platform)
    {
        fCarrier = carrier;
        fPlatform = platform;
    }

    public override void Perform()
    {
        if (fPlatform == null)
        {
            Cancel();
            return;
        }

        base.Perform();
        fPlatform.OnDestroy.AddListener(Event_OnPlatformDestroy);
        fPlatform.PlayLandingLightsAnim(fCarrier);
        DoFollow(fPlatform);
    }

    public override void Cancel()
    {
        fPlatform.StopLandingLightsAnim(fCarrier);
        fPlatform.OnDestroy.RemoveListener(Event_OnPlatformDestroy);
        base.Cancel();
    }

    public void DoFollow(CarrierLoadingPlatform_Script platform)
    {
        fIgnoreOrderCancel = true;
        fPlatform = platform;
        fCarrier.FollowToPlatform(platform, this);
        fIgnoreOrderCancel = false;
    }

    public void Event_OnPlatformDestroy(CarrierLoadingPlatform_Script sender)
    {
        if (fCarrier.HasLoad)
        {
            fCarrier.Unit.Owner.CarrierManager.Request_UnloadObjectsToPoint(fCarrier, fCarrier.Unit.Position);
        }

        Cancel();
    }

    protected override void CarrierDie()
    {
        base.CarrierDie();
    }

    protected override void OrderComplete(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        while (fPlatform.HasFreeSpace)
        {
            UnitObject_Script _object = fCarrier.UnloadFirstObject();

            if (_object != null)
            {
                fPlatform.PushObjectToPlatform(_object);
            }
            else
                break;
        }

        if (fCarrier.HasLoad)
        {
            fCarrier.Unit.Owner.CarrierManager.Request_UnloadObjectsToPoint(fCarrier, fPlatform.Position);
        }

        base.OrderComplete(data);
    }

    protected override void OrderCanceled(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        base.OrderCanceled(data);
    }
}


public class CarrierCapability_Script_OrderObjects: CarrierCapability_Script_BaseCommand
{
    protected List<string> fOrderList = new List<string>();
    protected List<ObjectTypeInfo_Script> fBuffer = new List<ObjectTypeInfo_Script>();

    public CarrierCapability_Script_OrderObjects(CarrierCapability_Script carrier, IEnumerable<string> order_list)
    {
        fCarrier = carrier;
        fOrderList.Clear();
        fOrderList.AddRange(order_list);
    }

    public override void Perform()
    {
        base.Perform();

        if (fOrderList.Count == 0)
            Cancel();

        fBuffer.Capacity = fOrderList.Count;
        Vector2Int mpoint = Carrier.Unit.CompCollection.GameObject_Comp.MapPosition;
        RectInt mbounds = GlobalCollector.Instance.Current_Map.Bounds;
        Vector3 point;

        if (mpoint.x < mpoint.y)
        {
            int dir;

            if (mpoint.x / mbounds.size.x < 0.5f)
                dir = mbounds.xMin - 1;
            else
                dir = mbounds.xMax + 1;

            point = GlobalCollector.Instance.Current_Map.MapCoordToWorld(dir, mpoint.y);
        }
        else
        {
            int dir;

            if (mpoint.y / mbounds.size.y < 0.5f)
                dir = mbounds.yMin - 1;
            else
                dir = mbounds.yMax + 1;

            point = GlobalCollector.Instance.Current_Map.MapCoordToWorld(mpoint.x, dir);
        }

        fIgnoreOrderCancel = true;
        fCarrier.FollowToPoint(point, this);
        fIgnoreOrderCancel = false;
    }

    public override void Cancel()
    {
        base.Cancel();
    }

    public IEnumerator WaitForUpdateFrame(CarrierCapability_Script_NotifyOrderStruct data)
    {
        yield return null;

        for (int i = 0; i < fBuffer.Count; i++)
        {
            UnitObject_Script _object = fBuffer[i].GetComponent<UnitObject_Script>();

            if (_object != null)
            {
                _object.Owner = fCarrier.Unit.Owner;
                fCarrier.AddLoadObject(_object);
            }
            else
                TypeObjectManager_Script.DestroyObjectInstance(fBuffer[i]);
        }

        base.OrderComplete(data);
    }

    protected override void OrderComplete(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        fBuffer.Clear();

        for (int i = 0; i < fOrderList.Count; i++)
        {
            if (!TypeObjectManager_Script.HasType(fOrderList[i]))
                continue;

            ObjectTypeInfo_Script _objectInfo = TypeObjectManager_Script.CreateObjectInstance(fOrderList[i], Vector3.zero, Vector3.zero, true);
            fBuffer.Add(_objectInfo);
        }
        
        fCarrier.StartCoroutine(WaitForUpdateFrame(data));
    }

    protected override void OrderCanceled(CarrierCapability_Script_NotifyOrderStruct data)
    {
        if (data.Sender != this)
            return;

        base.OrderCanceled(data);
    }
}

[System.Serializable]
public class CarrierCapability_Script_BaseCommandPreset
{
    [SerializeField]
    protected CarrierCapability_Script fCarrier = null;
    [SerializeField]
    protected NotifyEvent<CarrierCapability_Script_BaseCommand> fOnComplete = new NotifyEvent<CarrierCapability_Script_BaseCommand>();
    [SerializeField]
    protected NotifyEvent<CarrierCapability_Script_BaseCommand> fOnCancel = new NotifyEvent<CarrierCapability_Script_BaseCommand>();
    [SerializeField]
    protected CarrierCapability_Script_BaseCommand fLastCommand = null;

    public CarrierCapability_Script_BaseCommandPreset(CarrierCapability_Script carrier)
    {
        fCarrier = carrier;
    }

    public virtual void PushPreset()
    {

    }

    public virtual CarrierCapability_Script Carrier
    {
        get
        {
            return fCarrier;
        }

        set
        {
            fCarrier = value;
        }
    }

    public NotifyEvent<CarrierCapability_Script_BaseCommand> OnComplete
    {
        get
        {
            if (fLastCommand == null)
                return null;

            return fLastCommand.OnCompleted;
        }
    }

    public NotifyEvent<CarrierCapability_Script_BaseCommand> OnCancel
    {
        get
        {
            if (fLastCommand == null)
                return null;

            return fLastCommand.OnCancel;
        }
    }

    protected virtual void AddCommand(CarrierCapability_Script_BaseCommand command)
    {
        fLastCommand = command;
        fCarrier.CommandProcessor.PushCommand(command);
    }
}

[System.Serializable]
public class CarrierCapability_Script_LoadFromPlatformToPoint: CarrierCapability_Script_BaseCommandPreset
{
    [SerializeField]
    protected CarrierLoadingPlatform_Script fPlatform = null;
    [SerializeField]
    protected Vector3 fPoint = Vector3.zero;

    public CarrierCapability_Script_LoadFromPlatformToPoint(CarrierCapability_Script carrier, CarrierLoadingPlatform_Script platform, Vector3 point) : base(carrier)
    {
        fPlatform = platform;
        fPoint = point;        
    }

    public override void PushPreset()
    {
        base.PushPreset();

        CarrierCapability_Script_LoadObjectsFromPlatformCommand com1 = new CarrierCapability_Script_LoadObjectsFromPlatformCommand(fCarrier, fPlatform);
        CarrierCapability_Script_UnloadObjectToPointCommand com2 = new CarrierCapability_Script_UnloadObjectToPointCommand(fCarrier, fPoint);
        AddCommand(com1);
        AddCommand(com2);
    }
}

[System.Serializable]
public class CarrierCapability_Script_LoadObjectToPlatform: CarrierCapability_Script_BaseCommandPreset
{
    [SerializeField]
    protected UnitObject_Script fObject = null;
    [SerializeField]
    protected CarrierLoadingPlatform_Script fPlatform = null;

    public CarrierCapability_Script_LoadObjectToPlatform(CarrierCapability_Script carrier, UnitObject_Script _object, CarrierLoadingPlatform_Script platform): base(carrier)
    {
        fObject = _object;
        fPlatform = platform;
    }

    public override void PushPreset()
    {
        base.PushPreset();
        CarrierCapability_Script_PickupObjectCommand com1 = new CarrierCapability_Script_PickupObjectCommand(fCarrier, fObject);
        CarrierCapability_Script_UnloadObjectsToPlatformCommand com2 = new CarrierCapability_Script_UnloadObjectsToPlatformCommand(fCarrier, fPlatform);
        AddCommand(com1);
        AddCommand(com2);
    }
}

[System.Serializable]
public class CarrierCapability_Script_OrderToPlatform: CarrierCapability_Script_BaseCommandPreset
{
    [SerializeField]
    protected CarrierLoadingPlatform_Script fPlatform = null;
    [SerializeField]
    protected List<string> fOrderList = new List<string>();

    public CarrierCapability_Script_OrderToPlatform(CarrierCapability_Script carrier, CarrierLoadingPlatform_Script platform, List<string> order_list): base(carrier)
    {
        fPlatform = platform;
        fOrderList.AddRange(order_list);
    }

    public override void PushPreset()
    {
        base.PushPreset();
        CarrierCapability_Script_OrderObjects com1 = new CarrierCapability_Script_OrderObjects(fCarrier, fOrderList);
        CarrierCapability_Script_UnloadObjectsToPlatformCommand com2 = new CarrierCapability_Script_UnloadObjectsToPlatformCommand(fCarrier, fPlatform);
        AddCommand(com1);
        AddCommand(com2);
    }
}

[System.Serializable]
public class CarrierCapability_Script_OrderToPoint : CarrierCapability_Script_BaseCommandPreset
{
    [SerializeField]
    protected Vector3 fPoint = Vector3.zero;
    [SerializeField]
    protected List<string> fOrderList = new List<string>();

    public CarrierCapability_Script_OrderToPoint(CarrierCapability_Script carrier, Vector3 point, List<string> order_list): base(carrier)
    {
        fPoint = point;
        fOrderList.AddRange(order_list);
    }

    public override void PushPreset()
    {
        base.PushPreset();
        CarrierCapability_Script_OrderObjects com1 = new CarrierCapability_Script_OrderObjects(fCarrier, fOrderList);
        CarrierCapability_Script_UnloadObjectToPointCommand com2 = new CarrierCapability_Script_UnloadObjectToPointCommand(fCarrier, fPoint);
        AddCommand(com1);
        AddCommand(com2);
    }
}

[System.Serializable]
public class CarrierCapability_Script_TranslateObjectToPoint: CarrierCapability_Script_BaseCommandPreset
{
    [SerializeField]
    protected UnitObject_Script fObject;
    [SerializeField]
    protected Vector3 fPoint;

    public CarrierCapability_Script_TranslateObjectToPoint(CarrierCapability_Script carrier, UnitObject_Script _object, Vector3 point) : base(carrier)
    {
        fObject = _object;
        fPoint = point;
    }

    public override void PushPreset()
    {
        base.PushPreset();
        CarrierCapability_Script_PickupObjectCommand com1 = new CarrierCapability_Script_PickupObjectCommand(fCarrier, fObject);
        CarrierCapability_Script_UnloadObjectToPointCommand com2 = new CarrierCapability_Script_UnloadObjectToPointCommand(fCarrier, fPoint);
        AddCommand(com1);
        AddCommand(com2);
    }
}

[System.Serializable]
public class CarrierCapability_Script_UnloadObjectsToPoint: CarrierCapability_Script_BaseCommandPreset
{
    [SerializeField]
    protected Vector3 fPoint;

    public CarrierCapability_Script_UnloadObjectsToPoint(CarrierCapability_Script carrier, Vector3 point): base(carrier)
    {
        fPoint = point;
    }

    public override void PushPreset()
    {
        base.PushPreset();
        CarrierCapability_Script_UnloadObjectToPointCommand com1 = new CarrierCapability_Script_UnloadObjectToPointCommand(fCarrier, fPoint);
        AddCommand(com1);
    }
}

[System.Serializable]
public class CarrierCapability_Script_CommandProcessor
{
    public NotifyEvent<CarrierCapability_Script_CommandProcessor> OnProcessStart = new NotifyEvent<CarrierCapability_Script_CommandProcessor>();
    public NotifyEvent<CarrierCapability_Script_CommandProcessor> OnProcessComplete = new NotifyEvent<CarrierCapability_Script_CommandProcessor>();

    protected CarrierCapability_Script fCarrier = null;
    [SerializeField]
    protected List<CarrierCapability_Script_BaseCommand> fCommands = new List<CarrierCapability_Script_BaseCommand>();
    [SerializeField]
    protected bool fProcessing = false;
    [SerializeField]
    protected CarrierCapability_Script_BaseCommand fCurrentCommand = null;
    [SerializeField]
    protected bool fCorStarted = false;


    public bool Processing
    {
        get
        {
            return fProcessing;
        }
    }

    public CarrierCapability_Script_CommandProcessor(CarrierCapability_Script carrier)
    {
        fCarrier = carrier;
    }

    public void PushCommand(CarrierCapability_Script_BaseCommand command)
    {
        if (!AddCommand(command))
            return;

        if (!fProcessing)
        {
            OnProcessStart.Invoke(this);
            Process();
        }
    }

    public void Event_OnCommandComplete(CarrierCapability_Script_BaseCommand sender)
    {
        RemoveCommand(sender);
        Process();
    }

    public void Event_OnCommandCancel(CarrierCapability_Script_BaseCommand sender)
    {
        RemoveCommand(sender);
        Process();
    }

    protected bool AddCommand(CarrierCapability_Script_BaseCommand command)
    {
        if (fCommands.Contains(command))
            return false;

        command.OnCompleted.AddListener(Event_OnCommandComplete);
        command.OnCancel.AddListener(Event_OnCommandCancel);
        fCommands.Add(command);
        return true;
    }

    protected void RemoveCommand(CarrierCapability_Script_BaseCommand command)
    {
        command.OnCompleted.RemoveListener(Event_OnCommandComplete);
        command.OnCancel.RemoveListener(Event_OnCommandCancel);
        fCommands.Remove(command);
    }

    protected IEnumerator DoProcess()
    {
        yield return null;
        fCorStarted = false;

        if (fCommands.Count > 0)
        {
            fProcessing = true;
            fCurrentCommand = fCommands[0];
            fCurrentCommand.Perform();
        }
        else
        {
            fCurrentCommand = null;
            fProcessing = false;
            OnProcessComplete.Invoke(this);
        }
    }

    protected void Process()
    {
        if (!fCorStarted)
        {
            fCorStarted = true;
            fCarrier.StartCoroutine(DoProcess());
        }
    }

}

[RequireComponent(typeof(UnitObject_Script))]
public class CarrierCapability_Script : MonoBehaviour {
    public NotifyEvent<CarrierCapability_Script> OnCarrierDie = new NotifyEvent<CarrierCapability_Script>();
    public NotifyEvent_2P<CarrierCapability_Script, CarrierCapability_Script_NotifyOrderStruct> OnOrderComplete = new NotifyEvent_2P<CarrierCapability_Script, CarrierCapability_Script_NotifyOrderStruct>();
    public NotifyEvent_2P<CarrierCapability_Script, CarrierCapability_Script_NotifyOrderStruct> OnOrderCanceled = new NotifyEvent_2P<CarrierCapability_Script, CarrierCapability_Script_NotifyOrderStruct>();
    public NotifyEvent<CarrierCapability_Script> OnCommandProcessing = new NotifyEvent<CarrierCapability_Script>();
    public NotifyEvent<CarrierCapability_Script> OnCarrierRelease = new NotifyEvent<CarrierCapability_Script>();

    public static NotifyEvent_2P<CarrierCapability_Script, UnitObject_Script> Global_OnCarrierLoadObject = new NotifyEvent_2P<CarrierCapability_Script, UnitObject_Script>();
    public static NotifyEvent_2P<CarrierCapability_Script, UnitObject_Script> Global_OnCarrierUnloadObject = new NotifyEvent_2P<CarrierCapability_Script, UnitObject_Script>();

    protected UnitObject_Script fUnit = null;
    protected CarrierObjectMoving_Script fMovind_Comp = null;
    protected bool fCarrying = false;
    [SerializeField]
    protected UnitGroup_Script fCarriedObjects = new UnitGroup_Script();
    [SerializeField]
    protected CarrierCapability_Script_CommandProcessor fCommandProcessor = null;
    [SerializeField]
    protected bool fInit = true;

    public UnitObject_Script Unit
    {
        get
        {
            if (fUnit == null)
                fUnit = this.GetComponent<UnitObject_Script>();

            return fUnit;
        }
    }

    public CarrierObjectMoving_Script Moving_Comp
    {
        get
        {
            return fMovind_Comp;
        }
    }

    public UnitGroup_Script CarriedObjects
    {
        get
        {
            return fCarriedObjects;
        }
    }

    public CarrierCapability_Script_CommandProcessor CommandProcessor
    {
        get
        {
            return fCommandProcessor;
        }
    }

    public bool HasLoad
    {
        get
        {
            return fCarriedObjects.Count > 0;
        }
    }

    public bool IsBusy
    {
        get
        {
            return fCommandProcessor.Processing;
        }
    }

    public void FollowToPoint(Vector3 point, CarrierCapability_Script_BaseCommand command)
    {
        fMovind_Comp.Data = command;
        fMovind_Comp.SetTarget(point);
    }

    public void FollowToPlatform(CarrierLoadingPlatform_Script platform, CarrierCapability_Script_BaseCommand command)
    {
        fMovind_Comp.Data = command;
        fMovind_Comp.SetTarget(platform);
    }

    public void FollowToObject(UnitObject_Script _object, CarrierCapability_Script_BaseCommand command)
    {
        fMovind_Comp.Data = command;
        fMovind_Comp.SetTarget(_object.CompCollection);
    }
        
    public void AddLoadObject(UnitObject_Script _object)
    {
        if ((_object == null) ||
            fCarriedObjects.Contains(_object))
            return;

        _object.OnCarrier = true;
        _object.Direction = fUnit.CompCollection.GameObject_Comp.Body.right;
        _object.Visible = fCarriedObjects.Count == 0;

        fUnit.CompCollection.AttachingManager_Comp.AddAttach(_object.CompCollection.GameObject_Comp.Body, name + "_" + _object.name, ObjectAttaching_Script_AttachType.Parent, Vector3.zero);
        fCarriedObjects.Add(_object);

        Global_OnCarrierLoadObject.Invoke(this, _object);
    }

    public UnitObject_Script UnloadFirstObject()
    {
        UnitObject_Script _object = null;

        if (fCarriedObjects.Count > 0)
        {
            _object = fCarriedObjects[0];
            fUnit.CompCollection.AttachingManager_Comp.DeleteAttachByBody(_object.CompCollection.GameObject_Comp.Body);
            _object.OnCarrier = false;
            _object.Visible = true;
            fCarriedObjects.Remove(_object);

            if (fCarriedObjects.Count > 0)
            {
                fCarriedObjects[0].Visible = true;
            }

            Global_OnCarrierUnloadObject.Invoke(this, _object);
        }

        return _object;
    }

    public void DestroyLoad()
    {
        foreach (UnitObject_Script _object in fCarriedObjects)
            TypeObjectManager_Script.DestroyObjectInstance(_object.CompCollection.GameObject_Comp.Unit);
    }



    public void Event_OnTargetPointAchieved(CarrierObjectMoving_Script sender, Vector3 point)
    {
        if (!fCommandProcessor.Processing)
        {
            UpdateIdle();
            return;
        }

        OnOrderComplete.Invoke(this, new CarrierCapability_Script_NotifyOrderStruct(fMovind_Comp.Data, point));
    }

    public void Event_OnTargetObjectAchieved(CarrierObjectMoving_Script sender, ObjectComponentsCollection_Script _object)
    {
        if (!fCommandProcessor.Processing)
        {
            UpdateIdle();
            return;
        }

        OnOrderComplete.Invoke(this, new CarrierCapability_Script_NotifyOrderStruct(fMovind_Comp.Data, _object.Unit_Comp));
    }

    public void Event_OnTargetPlatformAchieved(CarrierObjectMoving_Script sender, CarrierLoadingPlatform_Script platform)
    {
        if (!fCommandProcessor.Processing)
        {
            UpdateIdle();
            return;
        }

        OnOrderComplete.Invoke(this, new CarrierCapability_Script_NotifyOrderStruct(fMovind_Comp.Data, platform));
    }

    public void Event_OnTargetCancel(HoldTargetBase_Script sender)
    {
        if (!fCommandProcessor.Processing)
        {
            UpdateIdle();
            return;
        }

        OnOrderCanceled.Invoke(this, new CarrierCapability_Script_NotifyOrderStruct(fMovind_Comp.Data));
    }

    public void Event_OnCarrierDie(UnitObject_Script sender)
    {
        OnCarrierDie.Invoke(this);
    }

    public void Event_OnProcessorStart(CarrierCapability_Script_CommandProcessor sender)
    {
        OnCommandProcessing.Invoke(this);
    }

    public void Event_OnProcessorFinish(CarrierCapability_Script_CommandProcessor sender)
    {
        OnCarrierRelease.Invoke(this);

        if (!fCommandProcessor.Processing)
        {
            UpdateIdle();
            return;
        }
    }

    protected void UpdateIdle()
    {         
        int axis = Random.Range(0, 10);

        if (axis < 5)
            axis = 0;
        else
            axis = 1;

        int border = Random.Range(0, 10);

        if (border < 5)
            border = 0;
        else
            border = 1;

        Vector2Int msize = GlobalCollector.Instance.Current_Map.GetMapSize();
        int value = Random.Range(0, Mathf.Max(msize.x, msize.y));
        Vector2Int pos = new Vector2Int(value * (1 - axis) + (msize.x * (1 - border) + -1 * border) * axis, 
                                        value * axis + (msize.y * (1 - border) + -1 * border) * (1 - axis));
        FollowToPoint(GlobalCollector.Instance.Current_Map.MapCoordToWorld(pos), null);
    }
    
    // Use this for initialization
    void Start () {
        fUnit = this.GetComponent<UnitObject_Script>();
        fMovind_Comp = this.GetComponent<CarrierObjectMoving_Script>();
        fMovind_Comp.OnTargetPointAchieved.AddListener(Event_OnTargetPointAchieved);
        fMovind_Comp.OnTargetObjectAchieved.AddListener(Event_OnTargetObjectAchieved);
        fMovind_Comp.OnTargetPlatformAchieved.AddListener(Event_OnTargetPlatformAchieved);
        fMovind_Comp.OnTargetCancel.AddListener(Event_OnTargetCancel);
        fUnit.OnDie.AddListener(Event_OnCarrierDie);
        fCommandProcessor = new CarrierCapability_Script_CommandProcessor(this);
        fCommandProcessor.OnProcessStart.AddListener(Event_OnProcessorStart);
        fCommandProcessor.OnProcessComplete.AddListener(Event_OnProcessorFinish);
    }
	
	// Update is called once per frame
	void Update () {
		if (fInit)
        {
            fInit = false;
            if (!fCommandProcessor.Processing)
                UpdateIdle();
        }
	}
}
